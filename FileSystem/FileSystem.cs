using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using CustomCollections;
using CustomQuery;
using ExceptionsNS;
using Text;
using Core;
using static Core.Constants;
using static FileSystemNS.Constants;
using System.Threading.Tasks;

namespace FileSystemNS
{
    public sealed class FileSystem : IDisposable
    {
        private const int RESILIENCY_FACTOR = 64;

        private readonly BitArray_ _bitMap;
        private readonly BitArray_ _badSectors;
        private readonly byte[] _reuseAddressArray = new byte[ADDRESS_BYTES];
        private readonly byte[] _reuseSectorArray;
        private readonly FileStream _stream;
        private readonly TaskInfo _taskInfo = new TaskInfo();
        private readonly long _badSectorsAddress;
        private BootByte _bootByte;

        internal long SectorCount { get; }
        internal long RootAddress { get; }
        internal ushort SectorSize { get; }
        internal ushort NextSectorIndex { get; }
        internal ushort ResiliencyByteCount { get; }
        internal ushort ResiliencyIndex { get; }
        internal ushort FirstSectorInfoSize { get; }
        internal ushort SectorInfoSize => NextSectorIndex;
        internal long BitmapByteCount => _bitMap.ByteCount;
        internal int FreeSectorCount => _bitMap.UnsetBits;
        internal bool IsRootCorrupted => _bootByte.HasFlag(BootByte.RootCorrupted);

        public Directory RootDirectory { get; private set; }
        public ReadOnlyTaskInfo TaskInfo { get; }
        public long TotalSize { get; }

        // Partial initialization. RootDirectory required.
        private FileSystem(FileStream fileStream, BootByte bootByte, long totalSize, ushort sectorSize, long sectorCount, BitArray_ bitMap, BitArray_ badSectors)
        {
            TaskInfo = new ReadOnlyTaskInfo(_taskInfo);

            _stream = fileStream;
            _bootByte = bootByte;
            TotalSize = totalSize;
            SectorSize = sectorSize;
            SectorCount = sectorCount;
            _bitMap = bitMap;
            _badSectors = badSectors;

            if (_bootByte.HasFlag(BootByte.ForcedShutdown))
                ValidateSectors()
                    .ContinueWith(validateSectorsTask => _bootByte &= ~BootByte.ForcedShutdown);

            _reuseSectorArray = new byte[SectorSize];
            ResiliencyByteCount = (ushort)(SectorSize / RESILIENCY_FACTOR);
            ResiliencyIndex = (ushort)(SectorSize - ResiliencyByteCount);
            NextSectorIndex = (ushort)(ResiliencyIndex - ADDRESS_BYTES);
            FirstSectorInfoSize = (ushort)(SectorInfoSize - 2 - NAME_BYTES - ADDRESS_BYTES - ResiliencyByteCount);
            _badSectorsAddress = BITMAP_INDEX + _bitMap.ByteCount;
            RootAddress = Math_.DivCeiling(_badSectorsAddress + _badSectors.ByteCount, SectorSize) * SectorSize;
        }

        public static FileSystem Create(FileStream fileStream, string name, long totalSize, ushort sectorSize)
        {
            long sectorCount = totalSize / sectorSize;
            var bitMap = new BitArray_(checked((int)sectorCount)); // BitmapBytes may cause overflow
            var badSectors = new BitArray_(checked((int)sectorCount));

            FileSystem fileSystem = new FileSystem(
                fileStream ?? throw new ArgumentNullException(nameof(fileStream)),
                BootByte.InitSuccess, totalSize, sectorSize, sectorCount, bitMap, badSectors);

            fileSystem.RootDirectory = Directory.CreateRoot(fileSystem, name);

            byte[] bootSectorBytes = new byte[BOOT_SECTOR_SIZE];
            totalSize.GetBytes(bootSectorBytes, TOTAL_SIZE_INDEX);
            sectorSize.GetBytes(bootSectorBytes, SECTOR_SIZE_INDEX);
            sectorCount.GetBytes(bootSectorBytes, SECTOR_COUNT_INDEX);
            fileStream.WriteAt(0, bootSectorBytes, 0, BOOT_SECTOR_SIZE);

            int reservedSectors = checked((int)fileSystem.RootAddress / fileSystem.SectorSize);
            for (int i = 0; i < reservedSectors; i++)
            {
                bitMap[i] = true;
                badSectors[i] = true;
            }
            fileStream.WriteAt(BITMAP_INDEX, bitMap.GetBytes(), 0, bitMap.ByteCount);
            fileStream.WriteAt(fileSystem._badSectorsAddress, badSectors.GetBytes(), 0, bitMap.ByteCount);
            fileStream.WriteByteAt(0, (byte)BootByte.InitSuccess); // Finally set the bits for successful initialization

            return fileSystem;
        }

        public static FileSystem Open(FileStream fileStream)
        {
            if (fileStream is null) throw new ArgumentNullException(nameof(fileStream));

            byte[] bootSectorbytes = new byte[BOOT_SECTOR_SIZE];
            fileStream.ReadAt(0, bootSectorbytes, 0, BOOT_SECTOR_SIZE);

            BootByte bootByte = (BootByte)bootSectorbytes[0];
            long totalSize = bootSectorbytes.GetLong(TOTAL_SIZE_INDEX);
            ushort sectorSize = bootSectorbytes.GetUShort(SECTOR_SIZE_INDEX);
            long sectorCount = bootSectorbytes.GetLong(SECTOR_COUNT_INDEX);

            byte[] bytes = new byte[Math_.DivCeiling(sectorCount, BYTE_BITS)];
            fileStream.ReadAt(BITMAP_INDEX, bytes, 0, bytes.Length);
            BitArray_ bitMap = new BitArray_(bytes, checked((int)sectorCount));

            fileStream.ReadAt(BITMAP_INDEX + bytes.Length, bytes, 0, bytes.Length);
            BitArray_ badSectors = new BitArray_(bytes, checked((int)sectorCount));

            FileSystem fileSystem = new FileSystem(fileStream, bootByte, totalSize, sectorSize, sectorCount, bitMap, badSectors);

            fileSystem.RootDirectory = Directory.LoadRoot(fileSystem);

            // In case of forced shutdown the bit will be true
            fileStream.WriteByteAt(0, (byte)(bootByte | BootByte.ForcedShutdown));

            return fileSystem;
        }

        public string GetHex()
        {
            StringBuilder_ sb = new StringBuilder_();
            int hex = _stream.ReadByteAt(0);
            while (hex != -1)
            {
                sb.Append(((byte)hex).ToHex_()).Append(" ");
                hex = _stream.ReadByte();
            }
            return sb.ToString();
        }

        public List_<string> GetHex(FS fs)
        {
            List_<string> strs = new List_<string>();

            if (fs.HasFlag(FS.BootByte))
                strs.Add(((byte)_stream.ReadByteAt(0)).ToHex_());

            if (fs.HasFlag(FS.FSInfo))
            {
                byte[] bytes = new byte[LONG_BYTES];
                _stream.ReadAt(TOTAL_SIZE_INDEX, bytes, 0, bytes.Length);
                strs.Add(bytes.ToHex_());

                bytes = new byte[USHORT_BYTES];
                _stream.ReadAt(SECTOR_SIZE_INDEX, bytes, 0, bytes.Length);
                strs.Add(bytes.ToHex_());

                bytes = new byte[LONG_BYTES];
                _stream.ReadAt(SECTOR_COUNT_INDEX, bytes, 0, bytes.Length);
                strs.Add(bytes.ToHex_());
            }

            if (fs.HasFlag(FS.BitMap))
                strs.Add(_bitMap.GetBytes().ToHex_());

            if (fs.HasFlag(FS.BadSectors))
                strs.Add(_badSectors.GetBytes().ToHex_());

            if (fs.HasFlag(FS.Root))
            {
                strs.Add(((byte)_stream.ReadByteAt(RootAddress)).ToHex_());
                strs.Add(((byte)_stream.ReadByteAt(RootAddress + 1)).ToHex_());

                byte[] bytes = new byte[NAME_BYTES];
                _stream.ReadAt(RootAddress + 1, bytes, 0, bytes.Length);
                strs.Add(bytes.ToHex_());

                bytes = new byte[LONG_BYTES];
                _stream.ReadAt(RootAddress + BYTE_COUNT_INDEX, bytes, 0, bytes.Length);
                strs.Add(bytes.ToHex_());

                bytes = new byte[FirstSectorInfoSize];
                _stream.ReadAt(RootAddress + INFO_BYTES_INDEX, bytes, 0, bytes.Length);
                strs.Add(bytes.ToHex_());

                bytes = new byte[ADDRESS_BYTES];
                _stream.ReadAt(RootAddress + NextSectorIndex, bytes, 0, bytes.Length);
                strs.Add(bytes.ToHex_());

                bytes = new byte[ResiliencyByteCount];
                _stream.ReadAt(RootAddress + ResiliencyIndex, bytes, 0, bytes.Length);
                strs.Add(bytes.ToHex_());
            }

            return strs;
        }

        public string GetBin()
        {
            StringBuilder_ sb = new StringBuilder_();
            int bin = _stream.ReadByteAt(0);
            while (bin != -1)
            {
                sb.Append(((byte)bin).ToBin_()).Append(" ");
                bin = _stream.ReadByte();
            }
            return sb.ToString();
        }

        public List_<string> GetBin(FS fs)
        {
            List_<string> strs = new List_<string>();

            if (fs.HasFlag(FS.BootByte))
                strs.Add(((byte)_stream.ReadByteAt(0)).ToBin_());

            if (fs.HasFlag(FS.FSInfo))
            {
                byte[] bytes = new byte[LONG_BYTES];
                _stream.ReadAt(TOTAL_SIZE_INDEX, bytes, 0, bytes.Length);
                strs.Add(bytes.ToBin_());

                bytes = new byte[USHORT_BYTES];
                _stream.ReadAt(SECTOR_SIZE_INDEX, bytes, 0, bytes.Length);
                strs.Add(bytes.ToBin_());

                bytes = new byte[LONG_BYTES];
                _stream.ReadAt(SECTOR_COUNT_INDEX, bytes, 0, bytes.Length);
                strs.Add(bytes.ToBin_());
            }

            if (fs.HasFlag(FS.BitMap))
                strs.Add(_bitMap.GetBytes().ToBin_());

            if (fs.HasFlag(FS.BadSectors))
                strs.Add(_badSectors.GetBytes().ToBin_());

            if (fs.HasFlag(FS.Root))
            {
                strs.Add(((byte)_stream.ReadByteAt(RootAddress)).ToBin_());
                strs.Add(((byte)_stream.ReadByteAt(RootAddress + 1)).ToBin_());

                byte[] bytes = new byte[NAME_BYTES];
                _stream.ReadAt(RootAddress + NAME_INDEX, bytes, 0, bytes.Length);
                strs.Add(bytes.ToBin_());

                bytes = new byte[LONG_BYTES];
                _stream.ReadAt(RootAddress + BYTE_COUNT_INDEX, bytes, 0, bytes.Length);
                strs.Add(bytes.ToBin_());

                bytes = new byte[FirstSectorInfoSize];
                _stream.ReadAt(RootAddress + INFO_BYTES_INDEX, bytes, 0, bytes.Length);
                strs.Add(bytes.ToBin_());

                bytes = new byte[ADDRESS_BYTES];
                _stream.ReadAt(RootAddress + NextSectorIndex, bytes, 0, bytes.Length);
                strs.Add(bytes.ToBin_());

                bytes = new byte[ResiliencyByteCount];
                _stream.ReadAt(RootAddress + ResiliencyIndex, bytes, 0, bytes.Length);
                strs.Add(bytes.ToBin_());
            }

            return strs;
        }

        public void Close()
        {
            _stream.WriteByteAt(0, (byte)_bootByte);
            _stream.Close();
        }

        internal Sector GetEmptySector() => Sector.GetEmpty(this);

        internal bool ValidateSectorAt(long address) => Sector.ValidateAt(this, address);

        internal bool TryGetSector(long address, out Sector sector) =>
            Sector.TryGet(this, address, out sector);

        internal bool TryUpdateSector(Object obj, Sector sector)
        {
            if (!obj.Parent.TryGetSector(out Sector parentSector))
                return false;

            int sectorCount = FullSectorsAndByteIndex(obj.GetIndexInParent() * ADDRESS_BYTES, out long objIndex);

            if (!parentSector.TryGetLast(sectorCount, out Sector targetSector))
                return false;

            sector.Address.GetBytes(_reuseAddressArray, 0);
            _stream.WriteAt(targetSector.Address + objIndex, _reuseAddressArray, 0, ADDRESS_BYTES);
            targetSector.UpdateResiliancy();
            return true;
        }

        internal FSResult TryCreateDirectory(Directory parent, out Sector sector)
        {
            sector = GetEmptySector();

            if (!parent.TryGetSector(out Sector parentSector))
                return FSResult.BadSectorFound;

            Sector lastSector;
            Sector dirWriteSector;
            Sector nextSector = GetEmptySector(); // Only used if new sector is needed for expansion
            int dirSectorCount = FullSectorsAndByteIndex(parent.Directories.Count * ADDRESS_BYTES, out long dirIndex);

            if (parent.Files.Count == 0)
            {
                if (dirIndex == 0)
                {
                    if (!parentSector.TryGetLast(dirSectorCount - 1, out lastSector))
                        return FSResult.BadSectorFound;

                    if (!sector.TryFindFree(out nextSector))
                        return FSResult.NotEnoughSpace;

                    lastSector.SetNext(nextSector);
                    lastSector.UpdateResiliancy();
                    lastSector = nextSector;
                    nextSector.Allocate();
                }
                else if (!parentSector.TryGetLast(dirSectorCount, out lastSector))
                    return FSResult.BadSectorFound;

                dirWriteSector = lastSector;
            }
            else
            {
                if (!parentSector.TryGetLast(dirSectorCount, out dirWriteSector))
                    return FSResult.BadSectorFound;

                int fileSectorCount = FullSectorsAndByteIndex(
                            dirIndex,
                            parent.Files.Count * ADDRESS_BYTES,
                            out long fileIndex);

                if (fileIndex == 0)
                {
                    if (!dirWriteSector.TryGetLast(fileSectorCount - 1, out lastSector))
                        return FSResult.BadSectorFound;

                    if (sector.TryFindFree(out nextSector))
                        return FSResult.NotEnoughSpace;

                    lastSector.SetNext(nextSector);
                    lastSector.UpdateResiliancy();
                    lastSector = nextSector;
                    nextSector.Allocate();
                }
                else if (!dirWriteSector.TryGetLast(fileSectorCount, out lastSector))
                    return FSResult.BadSectorFound;

                _stream.ReadAt(dirWriteSector.Address + dirIndex, _reuseAddressArray, 0, ADDRESS_BYTES);
                _stream.WriteAt(lastSector.Address + fileIndex, _reuseAddressArray, 0, ADDRESS_BYTES);
                lastSector.UpdateResiliancy();
            }

            if (!sector.TryFindFree(out sector))
            {
                if (!nextSector.IsEmpty)
                    nextSector.Free();

                return FSResult.NotEnoughSpace;
            }

            sector.Address.GetBytes(_reuseAddressArray, 0);
            _stream.WriteAt(dirWriteSector.Address + dirIndex, _reuseAddressArray, 0, ADDRESS_BYTES);
            dirWriteSector.UpdateResiliancy();
            sector.Allocate();
            parentSector.ByteCount = parent.ByteCount + ADDRESS_BYTES;
            parentSector.UpdateResiliancy();
            return FSResult.Success;
        }

        internal FSResult TryCreateFile(Directory parent, out Sector sector)
        {
            sector = GetEmptySector();

            if (!parent.TryGetSector(out Sector parentSector))
                return FSResult.BadSectorFound;

            Sector lastSector;
            Sector nextSector = GetEmptySector(); // Only used if new sector is needed for expansion
            int sectorCount = FullSectorsAndByteIndex(parent.ObjectCount * ADDRESS_BYTES, out long fileIndex);

            if (fileIndex == 0)
            {
                if (!parentSector.TryGetLast(sectorCount - 1, out lastSector))
                    return FSResult.BadSectorFound;

                if (!lastSector.TryFindFree(out nextSector))
                    return FSResult.NotEnoughSpace;

                lastSector.SetNext(nextSector);
                lastSector.UpdateResiliancy();
                lastSector = nextSector;
                nextSector.Allocate();
            }
            else if (!parentSector.TryGetLast(sectorCount, out lastSector))
                return FSResult.BadSectorFound;

            if (!sector.TryFindFree(out sector))
            {
                if (!nextSector.IsEmpty)
                    nextSector.Free();

                return FSResult.NotEnoughSpace;
            }

            sector.Address.GetBytes(_reuseAddressArray, 0);
            _stream.WriteAt(lastSector.Address + fileIndex, _reuseAddressArray, 0, ADDRESS_BYTES);
            lastSector.UpdateResiliancy();
            sector.Allocate();
            parentSector.ByteCount = parent.ByteCount + ADDRESS_BYTES;
            parentSector.UpdateResiliancy();
            return FSResult.Success;
        }

        internal FSResult TryRemoveDirectory(Directory parent, int dirIndex)
        {
            if (parent.TryGetSector(out Sector parentSector))
                return FSResult.BadSectorFound;

            int dirSectorCount = FullSectorsAndByteIndex(dirIndex * ADDRESS_BYTES, out long dirShortIndex);

            if (parentSector.TryGetLast(dirSectorCount, out Sector dirSector))
                return FSResult.BadSectorFound;

            int remainingDirSectors = FullSectorsAndByteIndex((parent.Directories.Count - 1) * ADDRESS_BYTES, out long lastDirIndex) - dirSectorCount;

            if (dirSector.TryGetLast(remainingDirSectors, out Sector lastDirSector))
                return FSResult.BadSectorFound;

            int fileSectors = FullSectorsAndByteIndex(lastDirIndex, parent.Files.Count * ADDRESS_BYTES, out long lastFileIndex);

            if (lastDirSector.TryGetLast(fileSectors, out Sector lastFileSector))
                return FSResult.BadSectorFound;

            _stream.ReadAt(lastDirSector.Address + lastDirIndex, _reuseAddressArray, 0, ADDRESS_BYTES);
            _stream.WriteAt(dirSector.Address + dirShortIndex, _reuseAddressArray, 0, ADDRESS_BYTES);
            dirSector.UpdateResiliancy();
            _stream.ReadAt(lastFileSector.Address + lastFileIndex, _reuseAddressArray, 0, ADDRESS_BYTES);
            _stream.WriteAt(lastDirSector.Address + lastDirIndex, _reuseAddressArray, 0, ADDRESS_BYTES);
            lastDirSector.UpdateResiliancy();

            if (lastFileIndex % SectorSize == 0) lastFileSector.Free();
            parentSector.ByteCount = parent.ByteCount - ADDRESS_BYTES;
            parentSector.UpdateResiliancy();
            return FSResult.Success;
        }

        internal FSResult TryRemoveFile(Directory parent, int fileIndex)
        {
            if (!parent.TryGetSector(out Sector parentSector))
                return FSResult.BadSectorFound;

            int fileSectorIndex = FullSectorsAndByteIndex(
                (parent.Directories.Count + fileIndex) * ADDRESS_BYTES,
                out long fileWriteIndex);

            if (!parentSector.TryGetLast(fileSectorIndex, out Sector fileWriteSector))
                return FSResult.BadSectorFound;

            int remainingSectors = FullSectorsAndByteIndex(
                fileWriteIndex,
                (parent.Files.Count - 1 - fileIndex) * ADDRESS_BYTES,
                out long fileReadIndex);

            if (!fileWriteSector.TryGetLast(remainingSectors, out Sector fileReadSector))
                return FSResult.BadSectorFound;

            _stream.ReadAt(fileReadSector.Address + fileReadIndex, _reuseAddressArray, 0, ADDRESS_BYTES);
            _stream.WriteAt(fileWriteSector.Address + fileWriteIndex, _reuseAddressArray, 0, ADDRESS_BYTES);
            fileWriteSector.UpdateResiliancy();

            if (fileReadIndex % SectorSize == 0) fileReadSector.Free();
            parentSector.ByteCount = parent.ByteCount - ADDRESS_BYTES;
            parentSector.UpdateResiliancy();
            return FSResult.Success;
        }

        internal FSResult FreeSectorsOf<T>(T obj, bool removeFirst = true) where T : Object
        {
            if (obj is Directory dir)
                foreach (var subObj in dir.EnumerateObjects())
                    FreeSectorsOf(subObj);

            return obj.TryGetSector(out Sector sector) &&
                sector.TryFreeChain(TotalSectors(obj.ByteCount), removeFirst)
                    ? FSResult.Success
                    : FSResult.BadSectorFound;
        }

        private int TotalSectors(long byteCount) => FullSectorsAndByteIndex(byteCount, out long byteIndex) + byteIndex > 0 ? 1 : 0;

        private int FullSectorsAndByteIndex(long byteCount, out long byteIndex)
        {
            byteIndex = INFO_BYTES_INDEX + byteCount;
            return byteCount < FirstSectorInfoSize
                ? 0
                : 1 + (int)Math.DivRem(byteIndex - FirstSectorInfoSize, SectorInfoSize, out byteIndex);
        }

        private int FullSectorsAndByteIndex(long startIndex, long byteCount, out long byteIndex)
        {
            byteIndex = startIndex + byteCount;
            long firstSectorSpace = SectorInfoSize - startIndex;
            return byteCount < firstSectorSpace
                ? 0
                : 1 + (int)Math.DivRem(byteIndex - firstSectorSpace, SectorInfoSize, out byteIndex);
        }

        private Task ValidateSectors()
        {
            if (_taskInfo.IsRunning)
                _taskInfo.Cancel().Wait();

            return _taskInfo.Run(() =>
            {
                lock (this)
                {
                    _taskInfo.Name = "Validating sectors";
                    BitArray_ newBitMap = new BitArray_(_bitMap.ByteCount);
                    int validated = 0;

                    int lastIndex = 0;
                    for (int i = 0; i < _badSectors.Count; i++)
                    {
                        if (_taskInfo.IsCancellationRequested) return;

                        lastIndex = _badSectors.IndexOf(true, lastIndex);
                        newBitMap[lastIndex] = true;
                        _taskInfo.Progress = validated++ / SectorCount;
                    }

                    Queue_<Directory> directoryQueue = new Queue_<Directory>();
                    directoryQueue.Enque(RootDirectory);

                    while (directoryQueue.Count > 0)
                    {
                        if (_taskInfo.IsCancellationRequested) return;

                        Directory curDir = directoryQueue.Deque();

                        ValidateSectorsOf(curDir);

                        for (int i = 0; i < curDir.Files.Count; i++)
                        {
                            if (_taskInfo.IsCancellationRequested) return;

                            File file = curDir.Files[i];

                            ValidateSectorsOf(file);
                        }

                        for (int i = 0; i < curDir.Directories.Count; i++)
                        {
                            if (_taskInfo.IsCancellationRequested) return;

                            directoryQueue.Enque(curDir.Directories[i]);
                        }
                    }

                    byte[] bytes = new byte[_bitMap.ByteCount];
                    newBitMap.CopyTo(bytes, 0);
                    _bitMap.CopyFrom(bytes);
                    _stream.WriteAt(BITMAP_INDEX, bytes, 0, bytes.Length);
                    _taskInfo.Progress = 1;

                    bool ValidateSectorsOf(Object obj)
                    {
                        return true;
                    }
                }
            });
        }

        void IDisposable.Dispose() => Close();



        internal readonly ref struct Sector
        {
            private readonly FileSystem _fs;

            private byte[] ReuseAddressArray => _fs._reuseAddressArray;
            private FileStream Stream => _fs._stream;
            private BitArray_ BitMap => _fs._bitMap;
            private ushort SectorSize => _fs.SectorSize;
            private ushort FirstSectorInfoSize => _fs.FirstSectorInfoSize;
            private ushort SectorInfoSize => _fs.SectorInfoSize;
            private ushort NextSectorIndex => _fs.NextSectorIndex;
            private long SectorCount => _fs.SectorCount;

            public ObjectFlags ObjectFlags
            {
                get => (ObjectFlags)Stream.ReadByteAt(Address);
                set => Stream.WriteByteAt(Address, (byte)value);
            }

            public string Name
            {
                get
                {
                    byte[] bytes = new byte[NAME_BYTES];
                    Stream.ReadAt(Address + NAME_INDEX, bytes, 0, NAME_BYTES);
                    return Encoding.Unicode.GetString(bytes).TrimEnd_('\0');
                }
                set
                {
                    byte[] bytes = new byte[NAME_BYTES];
                    Encoding.Unicode.GetBytes(value, 0, value.Length, bytes, 0);
                    Stream.WriteAt(Address + NAME_INDEX, bytes, 0, bytes.Length);
                }
            }

            public long ByteCount
            {
                get
                {
                    byte[] bytes = new byte[LONG_BYTES];
                    Stream.ReadAt(Address + BYTE_COUNT_INDEX, bytes, 0, bytes.Length);
                    return bytes.GetLong(0);
                }
                set
                {
                    byte[] bytes = new byte[LONG_BYTES];
                    value.GetBytes(bytes, 0);
                    Stream.WriteAt(Address + BYTE_COUNT_INDEX, bytes, 0, bytes.Length);
                }
            }

            public long Address { get; }
            public bool IsEmpty => Address == 0;

            private Sector(FileSystem fileSystem, long address)
            {
                _fs = fileSystem;
                Address = address;
            }

            public static Sector GetEmpty(FileSystem fileSystem) => new Sector(fileSystem, 0);

            public static bool ValidateAt(FileSystem fileSystem, long address)
            {
                if (fileSystem is null) throw new ArgumentNullException(nameof(fileSystem));
                if (address % fileSystem.SectorSize != 0 ||
                    address < fileSystem.RootAddress) throw new ArgumentOutOfRangeException(nameof(address));

                int resiliencyByteCount = fileSystem.ResiliencyByteCount;
                int resiliencyindex = fileSystem.ResiliencyIndex;
                byte[] sectorBytes = fileSystem._reuseSectorArray;
                fileSystem._stream.ReadAt(address, sectorBytes, 0, sectorBytes.Length);

                for (int i = 0; i < RESILIENCY_FACTOR; i++)
                    for (int y = 0; y < resiliencyByteCount; y++)
                        sectorBytes[resiliencyindex + y] ^= sectorBytes[i * resiliencyByteCount + y];

                for (int i = 0; i < resiliencyByteCount; i++)
                    if (sectorBytes[resiliencyindex + i] != 0)
                    {
                        new Sector(fileSystem, address).MarkAsBad();
                        return false;
                    }

                return true;
            }

            public static bool TryGet(FileSystem fileSystem, long address, out Sector sector)
            {
                if (ValidateAt(fileSystem, address))
                {
                    sector = new Sector(fileSystem, address);
                    return true;
                }
                else
                {
                    if (address == fileSystem.RootAddress) fileSystem._bootByte |= BootByte.RootCorrupted;
                    else fileSystem.ValidateSectors();
                    sector = GetEmpty(fileSystem);
                    return false;
                }
            }

            public bool TryGetNext(out Sector sector)
            {
                Stream.ReadAt(Address + NextSectorIndex, ReuseAddressArray, 0, ADDRESS_BYTES);
                return TryGet(_fs, ReuseAddressArray.GetLong(0), out sector);
            }

            public void SetNext(Sector next)
            {
                next.Address.GetBytes(ReuseAddressArray, 0);
                Stream.WriteAt(Address + NextSectorIndex, ReuseAddressArray, 0, ADDRESS_BYTES);
            }

            public bool TryGetLast(int count, out Sector sector)
            {
                sector = this;
                while (count-- > 0)
                    if (!sector.TryGetNext(out sector))
                        return false;
                return true;
            }

            public void SetProperties(Object obj)
            {
                ObjectFlags = obj.ObjectFlags;
                Name = obj.Name;
                ByteCount = obj.ByteCount;
            }

            public bool TryDeserializeChainTo(Object obj)
            {
                int count = checked((int)obj.ByteCount);
                byte[] bytes = new byte[count];
                Sector sector = this;

                int countFirstSector = Math.Min(count, FirstSectorInfoSize);
                Stream.ReadAt(Address + INFO_BYTES_INDEX, bytes, 0, countFirstSector);
                count -= countFirstSector;

                if (count == 0)
                {
                    obj.TryDeserializeBytes(bytes);
                    return true;
                }

                int index = countFirstSector;
                int fullSectors = Math.DivRem(count, SectorInfoSize, out int remaining);

                for (int i = 0; i < fullSectors; i++)
                {
                    Stream.ReadAt(sector.Address, bytes, index, SectorInfoSize);
                    if (!TryGetNext(out sector))
                        return false;
                }
                Stream.ReadAt(sector.Address, bytes, index, remaining);

                obj.TryDeserializeBytes(bytes);
                return true;
            }

            public bool TrySerializeChainFrom(Object obj)
            {
                byte[] bytes = obj.SerializeBytes();
                int count = bytes.Length;
                if (_fs.FreeSectorCount < _fs.TotalSectors(count)) return false;

                ObjectFlags = obj.ObjectFlags;
                Name = obj.Name;
                ByteCount = count;
                Allocate();

                if (count == 0)
                {
                    UpdateResiliancy();
                    return true;
                }

                int index = 0;
                int countFirstSector = Math.Min(count, FirstSectorInfoSize);
                Stream.WriteAt(ADDRESS_BYTES + INFO_BYTES_INDEX, bytes, index, countFirstSector);
                count -= countFirstSector;

                if (count == 0)
                {
                    UpdateResiliancy();
                    return true;
                }

                index += countFirstSector;
                int fullSectorsAfterFirst = Math.DivRem(count, SectorInfoSize, out int remaining);

                Sector sector = this;
                Sector nextSector;

                for (int i = 1; i <= fullSectorsAfterFirst; i++)
                {
                    if (!TryFindFree(out nextSector))
                    {
                        TryFreeChain(i);
                        return false;
                    }

                    sector.SetNext(nextSector);
                    sector.UpdateResiliancy();
                    Stream.WriteAt(nextSector.Address, bytes, index, SectorInfoSize);
                    index += SectorInfoSize;
                    nextSector.Allocate();
                    sector = nextSector;
                }

                if (remaining == 0) return true;

                if (!TryFindFree(out nextSector))
                {
                    TryFreeChain(fullSectorsAfterFirst + 2);
                    return false;
                }

                sector.SetNext(nextSector);
                sector.UpdateResiliancy();
                Stream.WriteAt(nextSector.Address, bytes, index, remaining);
                nextSector.Allocate();
                nextSector.UpdateResiliancy();
                return true;
            }

            public void UpdateResiliancy()
            {
                int resiliencyByteCount = _fs.ResiliencyByteCount;
                int resiliencyindex = _fs.ResiliencyIndex;
                byte[] sectorBytes = _fs._reuseSectorArray;
                Stream.ReadAt(Address, sectorBytes, 0, resiliencyindex);
                Array.Clear(sectorBytes, resiliencyindex, resiliencyByteCount);

                for (int i = 0; i < RESILIENCY_FACTOR; i++)
                    for (int y = 0; y < resiliencyByteCount; y++)
                        sectorBytes[resiliencyindex + y] ^= sectorBytes[i * resiliencyByteCount + y];

                Stream.WriteAt(Address + resiliencyindex, sectorBytes, resiliencyindex, resiliencyByteCount);
            }

            public bool TryFindFree(out Sector sector)
            {
                sector = GetEmpty(_fs);
                int index = 0;
                Sector curr;
                do
                {
                    index = BitMap.IndexOf(false, index);
                    if (index == -1)
                        return false;

                    curr = new Sector(_fs, index * SectorSize);
                    curr.Allocate();
                }
                while (!ValidateAt(_fs, curr.Address));

                sector = curr;
                return true;
            }

            public void Allocate()
            {
                int index = checked((int)(Address / SectorSize));
                int byteIndex = index / BYTE_BITS;

                if (index >= SectorCount) throw new ArgumentOutOfRangeException(nameof(Address), $"{nameof(Address)} exceeds the file system capacity.");

                BitMap[index] = BitMap[index]
                    ? throw new ArgumentException("Sector is already in use.", nameof(Address))
                    : true;
                Stream.WriteByteAt(BITMAP_INDEX + byteIndex, BitMap.GetByte(byteIndex));
            }

            public void Free()
            {
                int index = checked((int)(Address / SectorSize));
                int byteIndex = index / BYTE_BITS;

                if (index >= SectorCount) throw new ArgumentOutOfRangeException(nameof(Address), $"{nameof(Address)} exceeds the file system capacity.");

                BitMap[index] = BitMap[index]
                    ? false
                    : throw new ArgumentException("Sector is already free.", nameof(Address));
                Stream.WriteByteAt(BITMAP_INDEX + byteIndex, BitMap.GetByte(byteIndex));
            }

            public bool TryFreeChain(int totalCount, bool freeThis = true)
            {
                if (freeThis) Free();

                Sector sector = this;
                while (--totalCount > 0)
                {
                    if (!sector.TryGetNext(out sector))
                        return false;
                    sector.Free();
                }
                return true;
            }

            private void MarkAsBad()
            {
                int index = checked((int)(Address / SectorSize));
                int byteIndex = index / BYTE_BITS;

                if (index >= SectorCount) throw new ArgumentOutOfRangeException(nameof(Address), $"{nameof(Address)} exceeds the file system capacity.");

                _fs._badSectors[index] = _fs._badSectors[index]
                    ? throw new ArgumentException("Sector is already marked as bad.", nameof(Address))
                    : true;
                Stream.WriteByteAt(_fs._badSectorsAddress + byteIndex, BitMap.GetByte(byteIndex));
            }
        }
    }
}
