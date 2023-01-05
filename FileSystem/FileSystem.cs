using System;
using System.Collections;
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

namespace FileSystemNS
{
    public sealed class FileSystem : IDisposable
    {
        private const int RESILIENCY_FACTOR = 64;

        private readonly HashSet_<long> _badSectors;
        private readonly byte[] _reuseAddressArray = new byte[ADDRESS_BYTES];
        private readonly FileStream _stream;
        private readonly BitArray_ _bitMap;

        internal long SectorCount { get; }
        internal long RootAddress { get; }
        internal ushort SectorSize { get; }
        internal ushort NextSectorIndex { get; }
        internal ushort ResiliencyBytes { get; }
        internal ushort ResiliencyIndex { get; }
        internal ushort FirstSectorInfoSize { get; }
        internal ushort SectorInfoSize => NextSectorIndex;
        internal long BitmapBytes => _bitMap.ByteCount;
        internal int FreeSectors => _bitMap.UnsetBits;

        public Directory RootDirectory { get; private set; }
        public long TotalSize { get; }

        // Partial initialization. RootDirectory required.
        private FileSystem(FileStream fileStream, long totalSize, ushort sectorSize, long sectorCount, BitArray_ bitmap)
        {
            _stream = fileStream;
            TotalSize = totalSize;
            SectorSize = sectorSize;
            SectorCount = sectorCount;
            ResiliencyBytes = (ushort)(SectorSize / RESILIENCY_FACTOR);
            ResiliencyIndex = (ushort)(SectorSize - ResiliencyBytes);
            NextSectorIndex = (ushort)(ResiliencyIndex - ADDRESS_BYTES);
            FirstSectorInfoSize = (ushort)(SectorInfoSize - 2 - NAME_BYTES - ADDRESS_BYTES - ResiliencyBytes);
            _badSectors = new HashSet_<long>();
            _bitMap = bitmap;
            int rquiredBytes = BITMAP_INDEX + _bitMap.ByteCount;
            RootAddress = Math_.DivCeiling(rquiredBytes, SectorSize) * SectorSize;
        }

        public static FileSystem Create(FileStream fileStream, string name, long totalSize, ushort sectorSize)
        {
            long sectorCount = totalSize / sectorSize;
            var bitMap = new BitArray_((int)sectorCount); // BitmapBytes may be too large

            FileSystem fileSystem = new FileSystem(
                fileStream ?? throw new ArgumentNullException(nameof(fileStream)),
                totalSize, sectorSize, sectorCount, bitMap);

            fileSystem.RootDirectory = Directory.CreateRoot(fileSystem, name);

            byte[] bootSectorBytes = new byte[BOOT_SECTOR_SIZE];
            totalSize.GetBytes(bootSectorBytes, TOTAL_SIZE_INDEX);
            sectorSize.GetBytes(bootSectorBytes, SECTOR_SIZE_INDEX);
            sectorCount.GetBytes(bootSectorBytes, SECTOR_COUNT_INDEX);

            fileStream.WriteAt(0, bootSectorBytes, 0, BOOT_SECTOR_SIZE);
            fileStream.WriteAt(BITMAP_INDEX, bitMap.GetBytes(), 0, bitMap.ByteCount);

            fileSystem.AllocateSectorAt(fileSystem.RootAddress);
            fileStream.WriteByteAt(0, (byte)BootByte.All); // Finally set the bits for successful initialization

            return fileSystem;
        }

        public static FileSystem Open(FileStream fileStream)
        {
            if (fileStream is null) throw new ArgumentNullException(nameof(fileStream));

            byte[] bytes = new byte[BOOT_SECTOR_SIZE];
            fileStream.ReadAt(0, bytes, 0, BOOT_SECTOR_SIZE);

            long totalSize = bytes.GetLong(TOTAL_SIZE_INDEX);
            ushort sectorSize = bytes.GetUShort(SECTOR_SIZE_INDEX);
            long sectorCount = bytes.GetLong(SECTOR_COUNT_INDEX);
            bytes = new byte[Math_.DivCeiling(sectorCount, BYTE_BITS)];

            fileStream.ReadAt(BITMAP_INDEX, bytes, 0, bytes.Length);
            BitArray_ bitmap = new BitArray_(bytes, checked((int)sectorCount));

            FileSystem fileSystem = new FileSystem(fileStream, totalSize, sectorSize, sectorCount, bitmap);

            fileSystem.RootDirectory = Directory.LoadRoot(fileSystem);

            // In case of forced shutdown the bit will be true
            fileStream.WriteByteAt(0, (byte)(fileStream.ReadByteAt(0) | (byte)BootByte.ForcedShutdown));

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

            if (fs.HasFlag(FS.Root))
            {
                strs.Add(((byte)_stream.ReadByteAt(RootDirectory.Address)).ToHex_());
                strs.Add(((byte)_stream.ReadByteAt(RootDirectory.Address + 1)).ToHex_());

                byte[] bytes = new byte[NAME_BYTES];
                _stream.ReadAt(RootDirectory.Address + 1, bytes, 0, bytes.Length);
                strs.Add(bytes.ToHex_());

                bytes = new byte[LONG_BYTES];
                _stream.ReadAt(RootDirectory.Address + BYTE_COUNT_INDEX, bytes, 0, bytes.Length);
                strs.Add(bytes.ToHex_());

                bytes = new byte[FirstSectorInfoSize];
                _stream.ReadAt(RootDirectory.Address + INFO_BYTES_INDEX, bytes, 0, bytes.Length);
                strs.Add(bytes.ToHex_());

                bytes = new byte[ADDRESS_BYTES];
                _stream.ReadAt(RootDirectory.Address + NextSectorIndex, bytes, 0, bytes.Length);
                strs.Add(bytes.ToHex_());

                bytes = new byte[ResiliencyBytes];
                _stream.ReadAt(RootDirectory.Address + ResiliencyIndex, bytes, 0, bytes.Length);
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

            if (fs.HasFlag(FS.Root))
            {
                strs.Add(((byte)_stream.ReadByteAt(RootDirectory.Address)).ToBin_());
                strs.Add(((byte)_stream.ReadByteAt(RootDirectory.Address + 1)).ToBin_());

                byte[] bytes = new byte[NAME_BYTES];
                _stream.ReadAt(RootDirectory.Address + NAME_INDEX, bytes, 0, bytes.Length);
                strs.Add(bytes.ToBin_());

                bytes = new byte[LONG_BYTES];
                _stream.ReadAt(RootDirectory.Address + BYTE_COUNT_INDEX, bytes, 0, bytes.Length);
                strs.Add(bytes.ToBin_());

                bytes = new byte[FirstSectorInfoSize];
                _stream.ReadAt(RootDirectory.Address + INFO_BYTES_INDEX, bytes, 0, bytes.Length);
                strs.Add(bytes.ToBin_());

                bytes = new byte[ADDRESS_BYTES];
                _stream.ReadAt(RootDirectory.Address + NextSectorIndex, bytes, 0, bytes.Length);
                strs.Add(bytes.ToBin_());

                bytes = new byte[ResiliencyBytes];
                _stream.ReadAt(RootDirectory.Address + ResiliencyIndex, bytes, 0, bytes.Length);
                strs.Add(bytes.ToBin_());
            }

            return strs;
        }

        public void Close()
        {
            _stream.WriteByteAt(0, (byte)(_stream.ReadByteAt(0) & ~(byte)BootByte.ForcedShutdown)); // Signal correct shut down
            _stream.Close();
        }

        internal bool TryGetSector(long address, out Sector sector) =>
            Sector.TryGet(this, address, out sector);

        internal FSResult TryCreateDirectory(Directory parent, out Sector sector)
        {
            sector = new Sector();
            if (!parent.TryGetSector(out Sector parentSector))
                return FSResult.BadSectorFound;

            Sector lastSector;
            Sector dirWriteSector;
            Sector nextSector = new Sector(); // Only used if new sector is needed for expansion
            int dirSectorCount = FullSectorsAndByteIndex(parent.SubDirectories.Count * ADDRESS_BYTES, out long dirIndex);

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
                            SectorInfoSize - dirIndex,
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
            sector = new Sector();
            if (!parent.TryGetSector(out Sector parentSector))
                return FSResult.BadSectorFound;

            Sector lastSector;
            Sector nextSector = new Sector(); // Only used if new sector is needed for expansion
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

        internal void RemoveSubdirectory(Directory parent, int subdirIndex)
        {
            byte[] bytes = new byte[ADDRESS_BYTES];
            int dirSectorIndex = FullSectorsAndByteIndex(subdirIndex * ADDRESS_BYTES, out long dirShortIndex);
            long dirSectorAddress = EnumerateSectors(parent.Address, dirSectorIndex, true).Last_();
            long dirWriteAddress = dirSectorAddress + dirShortIndex;
            long lastSector = EnumerateSectors(
                    dirSectorAddress,
                    FullSectorsAndByteIndex(parent.SubDirectories.Count * ADDRESS_BYTES - ADDRESS_BYTES, out long lastDirIndex) - dirSectorIndex)
                .Last_();
            long dirReadAddress = lastSector + lastDirIndex;
            long lastFileSectorAddress = EnumerateSectors(
                    lastSector,
                    FullSectorsAndByteIndex(lastDirIndex, SectorInfoSize - lastDirIndex, parent.Files.Count * ADDRESS_BYTES, out long lastFileIndex))
                .Last_();

            _stream.ReadAt(dirReadAddress, bytes, 0, bytes.Length);
            _stream.WriteAt(dirWriteAddress, bytes, 0, bytes.Length);
            _stream.ReadAt(lastFileSectorAddress + lastFileIndex, bytes, 0, bytes.Length);
            _stream.WriteAt(dirReadAddress, bytes, 0, bytes.Length);

            if (lastFileIndex % SectorSize == 0) FreeSectorAt(lastFileIndex);
        }

        internal FSResult RemoveFile(Directory parent, int fileIndex)
        {
            int fileSectorIndex = FullSectorsAndByteIndex(
                (parent.SubDirectories.Count + fileIndex) * ADDRESS_BYTES,
                out long fileWriteIndex);

            if (!parent.TryGetSector(out Sector parentSector))
                return FSResult.BadSectorFound;

            if (!parentSector.TryGetLast(fileSectorIndex, out Sector fileWriteSector))
                return FSResult.BadSectorFound;

            int remainingSectors = FullSectorsAndByteIndex(
                fileWriteIndex,
                SectorInfoSize - fileWriteIndex,
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

        internal void FreeSectorsOf<T>(T obj, bool removeFirst = true) where T : Object
        {
            if (obj is Directory dir)
                foreach (var subObj in dir.EnumerateObjects())
                    FreeSectorsOf(subObj);

            foreach (long address in EnumerateSectors(obj.Address, FullSectorsAndByteIndex(obj.ByteCount, out _), removeFirst))
                FreeSectorAt(address);
        }

        private void AllocateSectorAt(long address)
        {
            int index = (int)((address - RootAddress) / SectorSize);
            int byteIndex = index / BYTE_BITS;

            if (index >= SectorCount) throw new ArgumentOutOfRangeException(nameof(address), $"{nameof(address)} exceeds the file system capacity.");

            _bitMap[index] = _bitMap[index]
                ? throw new ArgumentException("Sector is already in use.", nameof(address))
                : true;
            _stream.WriteByteAt(BITMAP_INDEX + byteIndex, _bitMap.GetByte(byteIndex));
        }

        private void FreeSectorAt(long address)
        {
            int index = (int)((address - RootAddress) / SectorSize);
            int byteIndex = index / BYTE_BITS;

            if (index >= SectorCount) throw new ArgumentOutOfRangeException(nameof(address), $"{nameof(address)} exceeds the file system capacity.");

            _bitMap[index] = _bitMap[index]
                ? false
                : throw new ArgumentException("Sector is already free.", nameof(address));
            _stream.WriteByteAt(BITMAP_INDEX + byteIndex, _bitMap.GetByte(byteIndex));
        }

        private int FullSectorsAndByteIndex(long byteCount, out long byteIndex)
        {
            byteIndex = INFO_BYTES_INDEX + byteCount;
            return byteCount < FirstSectorInfoSize
                ? 0
                : 1 + (int)Math.DivRem(byteIndex - FirstSectorInfoSize, SectorInfoSize, out byteIndex);
        }

        private int FullSectorsAndByteIndex(long startIndex, long firstSectorSpace, long byteCount, out long byteIndex)
        {
            byteIndex = startIndex + byteCount;
            return byteCount < firstSectorSpace
                ? 0
                : 1 + (int)Math.DivRem(byteIndex - firstSectorSpace, SectorInfoSize, out byteIndex);
        }

        /// <summary>
        /// Iterates over the next sectorCount sectorts and yields them.
        /// </summary>
        /// <returns>Returns sector addresses or -1 if a bad sector is encountered</returns>
        private IEnumerable<long> EnumerateSectors(long address, int sectorCount, bool yieldGivenAddress = true)
        {
            if (yieldGivenAddress)
            {
                if (IsBadSector(address))
                {
                    yield return -1;
                    yield break;
                }

                yield return address;
            }
            if (sectorCount == 0) yield break;

            byte[] bytes = new byte[ADDRESS_BYTES];
            long currAddress = address;

            for (int i = 0; i < sectorCount; i++)
            {
                _stream.ReadAt(currAddress + NextSectorIndex, bytes, 0, bytes.Length);
                currAddress = bytes.GetLong(0);

                if (IsBadSector(currAddress))
                {
                    yield return -1;
                    yield break;
                }

                yield return currAddress;
            }
        }

        private bool IsBadSector(long address)
        {
            byte[] sectorBytes = new byte[SectorSize];
            _stream.ReadAt(address, sectorBytes, 0, sectorBytes.Length);
            byte[] bytes = new byte[ResiliencyBytes];

            for (int i = 0; i < RESILIENCY_FACTOR; i++)
                for (int y = 0; y < ResiliencyBytes; y++)
                    bytes[y] ^= sectorBytes[i * RESILIENCY_FACTOR + y];

            for (int i = 0; i < bytes.Length; i++)
                if (bytes[i] != 0)
                {
                    _badSectors.Add(address);
                    AllocateSectorAt(address);
                    ValidateSectors();
                    return true;
                }

            return false;
        }

        private void ValidateSectors()
        {

        }

        void IDisposable.Dispose() => Close();



        internal readonly ref struct Sector
        {
            private readonly FileSystem _fs;

            private byte[] ReuseAddressArray => _fs._reuseAddressArray;
            private FileStream Stream => _fs._stream;
            private BitArray_ BitMap => _fs._bitMap;
            private ushort FirstSectorInfoSize => _fs.FirstSectorInfoSize;
            private ushort SectorInfoSize => _fs.SectorInfoSize;

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
            public bool IsEmpty => _fs is null;

            private Sector(FileSystem fileSystem, long address)
            {
                _fs = fileSystem;
                Address = address;
            }

            public static bool TryGet(FileSystem fileSystem, long address, out Sector sector)
            {
                if (fileSystem is null) throw new ArgumentNullException(nameof(fileSystem));
                if (address % fileSystem.SectorSize != 0) throw new ArgumentOutOfRangeException(nameof(address));

                if (fileSystem.IsBadSector(address))
                {
                    sector = new Sector();
                    return false;
                }

                sector = new Sector(fileSystem, address);
                return true;
            }

            public bool TryGetNext(out Sector sector)
            {
                Stream.ReadAt(Address + _fs.NextSectorIndex, ReuseAddressArray, 0, ADDRESS_BYTES);
                return TryGet(_fs, ReuseAddressArray.GetLong(0), out sector);
            }

            public void SetNext(Sector next)
            {
                next.Address.GetBytes(ReuseAddressArray, 0);
                Stream.WriteAt(Address + _fs.NextSectorIndex, ReuseAddressArray, 0, ADDRESS_BYTES);
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
                Stream.ReadAt(INFO_BYTES_INDEX, bytes, 0, countFirstSector);
                count -= countFirstSector;
                if (count == 0) return true;

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
                ByteCount = count;

                if (count == 0) return true;

                int index = 0;
                int countFirstSector = Math.Min(count, FirstSectorInfoSize);
                Stream.WriteAt(INFO_BYTES_INDEX, bytes, index, countFirstSector);
                count -= countFirstSector;

                if (count == 0) return true;

                index += countFirstSector;
                int fullSectors = Math.DivRem(count, SectorInfoSize, out int remaining);

                Sector sector;

                for (int i = 0; i < fullSectors; i++)
                {
                    if (!TryFindFree(out sector))
                    {
                        FreeChain(i);
                        return false;
                    }
                    Stream.WriteAt(sector.Address, bytes, index, SectorInfoSize);
                    index += SectorInfoSize;
                    Allocate();
                }

                if (remaining == 0) return true;

                if (!TryFindFree(out sector))
                {
                    FreeChain(fullSectors + 1);
                    return false;
                }
                Stream.WriteAt(sector.Address, bytes, index, remaining);
                Allocate();
                return true;
            }

            public void UpdateResiliancy()
            {
                byte[] sectorBytes = new byte[_fs.SectorSize];
                Stream.ReadAt(Address, sectorBytes, 0, sectorBytes.Length);
                byte[] bytes = new byte[_fs.ResiliencyBytes];
                for (int i = 0; i < RESILIENCY_FACTOR; i++)
                    for (int y = 0; y < _fs.ResiliencyBytes; y++)
                        bytes[y] ^= sectorBytes[i * RESILIENCY_FACTOR + y];

                Stream.WriteAt(Address + _fs.ResiliencyIndex, bytes, 0, bytes.Length);
            }

            public bool TryFindFree(out Sector sector)
            {
                int index = 0;
                bool isBad;
                do
                {
                    index = BitMap.IndexOf(false, index);
                    isBad = !TryGet(_fs, index, out sector);
                }
                while (index != -1 && isBad);
                return index != -1;
            }

            public void Allocate() => _fs.AllocateSectorAt(Address);

            public void Free()
            {
                int index = (int)((Address - _fs.RootAddress) / _fs.SectorSize);
                int byteIndex = index / BYTE_BITS;

                if (index >= _fs.SectorCount) throw new ArgumentOutOfRangeException(nameof(Address), $"{nameof(Address)} exceeds the file system capacity.");

                BitMap[index] = BitMap[index]
                    ? false
                    : throw new ArgumentException("Sector is already free.", nameof(Address));
                Stream.WriteByteAt(BITMAP_INDEX + byteIndex, BitMap.GetByte(byteIndex));
            }

            public void FreeChain(int count)
            {
                Sector sector = this;
                while (count-- > 0)
                {
                    sector.Free();
                    sector.TryGetNext(out sector);
                }
            }
        }
    }
}
