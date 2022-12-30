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

        private readonly FileStream _stream;
        private readonly BitArray_ _bitMap;
        private readonly HashSet_<long> _badSectors;

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

        internal ObjectFlags GetObjectFlagsAt(long address) =>
            (ObjectFlags)_stream.ReadByteAt(address);

        internal void SerializeObjectFlags(Object obj) => SetObjectFlagsAt(obj.Address, obj.ObjectFlags);
        internal void SetObjectFlagsAt(long address, ObjectFlags objectFlags) =>
            _stream.WriteByteAt(address, (byte)objectFlags);

        internal string GetNameAt(long address)
        {
            byte[] bytes = new byte[NAME_BYTES];
            _stream.ReadAt(address + NAME_INDEX, bytes, 0, NAME_BYTES);
            return Encoding.Unicode.GetString(bytes).TrimEnd_('\0');
        }

        internal void SerializeName(Object obj) => SetNameAt(obj.Address, obj.Name);
        internal void SetNameAt(long address, string name)
        {
            byte[] bytes = new byte[NAME_BYTES];
            Encoding.Unicode.GetBytes(name, 0, name.Length, bytes, 0);
            _stream.WriteAt(address + NAME_INDEX, bytes, 0, bytes.Length);
        }

        internal long GetByteCountAt(long address)
        {
            byte[] bytes = new byte[LONG_BYTES];
            _stream.ReadAt(address + BYTE_COUNT_INDEX, bytes, 0, bytes.Length);
            return bytes.GetLong(0);
        }

        internal void SerializeByteCount(Object obj) => SetByteCountAt(obj.Address, obj.ByteCount);
        internal void SetByteCountAt(long address, long byteCount)
        {
            byte[] bytes = new byte[LONG_BYTES];
            byteCount.GetBytes(bytes, 0);
            _stream.WriteAt(address + BYTE_COUNT_INDEX, bytes, 0, bytes.Length);
        }

        internal void SerializeProperties(Object obj)
        {
            SerializeObjectFlags(obj);
            SerializeName(obj);
            SerializeByteCount(obj);
        }

        internal void DeserializeAllInfoBytes(Object obj) => obj.DeserializeBytes(GetAllInfoBytesAt(obj.Address, checked((int)obj.ByteCount)));
        internal byte[] GetAllInfoBytesAt(long address, int count)
        {
            byte[] bytes = new byte[count];
            GetAllInfoBytesAt(address, bytes);
            return bytes;
        }
        internal void GetAllInfoBytesAt(long address, byte[] bytes) => GetAllInfoBytesAt(address, bytes, 0, bytes.Length);
        internal void GetAllInfoBytesAt(long address, byte[] bytes, int index, int count)
        {
            int countFirstSector = Math.Min(count, FirstSectorInfoSize);
            _stream.ReadAt(address + INFO_BYTES_INDEX, bytes, index, countFirstSector);
            count -= countFirstSector;
            if (count == 0) return;

            index += countFirstSector;
            int fullSectors = Math.DivRem(count, SectorInfoSize, out int remaining);
            byte[] addressBytes = new byte[ADDRESS_BYTES];
            _stream.ReadAt(address + NextSectorIndex, addressBytes, 0, addressBytes.Length);

            long lastFullSectorAddress = -1;
            foreach (var currAddress in EnumerateSectors(addressBytes.GetLong(0), fullSectors, false))
            {
                _stream.ReadAt(currAddress, bytes, index, SectorInfoSize);
                index += SectorInfoSize;
                lastFullSectorAddress = currAddress;
            }

            _stream.ReadAt(lastFullSectorAddress + NextSectorIndex, addressBytes, 0, addressBytes.Length);
            _stream.ReadAt(addressBytes.GetLong(0) + remaining, bytes, index, remaining);
        }

        internal void SerializeAllInfoBytes(Object obj)
        {
            byte[] bytes = obj.SerializeBytes();
            SetByteCountAt(obj.Address, bytes.Length);
            SetInfoBytesAt(obj.Address + INFO_BYTES_INDEX, bytes);
        }
        internal void AppendInfoBytes(Object obj, byte[] bytes)
        {
            SetByteCountAt(obj.Address, obj.ByteCount + bytes.Length);
            SetInfoBytesAt(EnumerateSectors(obj.Address, FullSectorsAndByteIndex(obj.ByteCount, out long index)).Last_() + index, bytes);
        }
        internal void SetInfoBytesAt(long byteAddress, byte[] bytes) => SetInfoBytesAt(byteAddress, bytes, 0, bytes.Length);
        internal void SetInfoBytesAt(long byteAddress, byte[] bytes, int index, int count)
        {
            if (count == 0) return;

            long FirstSectorFreeSpace = SectorInfoSize - 1 - byteAddress % SectorInfoSize;
            int countFirstSector = (int)Math.Min(count, FirstSectorFreeSpace);
            _stream.WriteAt(byteAddress, bytes, index, countFirstSector);
            count -= countFirstSector;
            if (count == 0) return;

            index += countFirstSector;
            int fullSectors = Math.DivRem(count, SectorInfoSize, out int remaining);

            byte[] addressBytes = new byte[ADDRESS_BYTES];
            long lastFreeAddress = byteAddress + FirstSectorFreeSpace;
            long freeAddress;

            for (int i = 0; i < fullSectors; i++)// TODO: Check for avalible space
            {
                freeAddress = FindFreeSector();
                freeAddress.GetBytes(addressBytes, 0);
                _stream.WriteAt(lastFreeAddress, addressBytes, index, ADDRESS_BYTES);
                _stream.WriteAt(freeAddress, bytes, index, SectorInfoSize);
                index += SectorInfoSize;
                AllocateSectorAt(freeAddress);
                lastFreeAddress = freeAddress;
            }

            freeAddress = FindFreeSector();
            freeAddress.GetBytes(addressBytes, 0);
            _stream.WriteAt(lastFreeAddress, addressBytes, index, ADDRESS_BYTES);
            _stream.WriteAt(freeAddress, bytes, index, remaining);
            AllocateSectorAt(freeAddress);
        }

        internal long CreateSubdirectory(Directory parent)
        {
            long newDirSector;
            SetByteCountAt(parent.Address, parent.ByteCount + ADDRESS_BYTES);
            byte[] bytes = new byte[ADDRESS_BYTES];

            int dirSectorCount = FullSectorsAndByteIndex(parent.SubDirectories.Count * ADDRESS_BYTES, out long dirIndex);
            long dirSector;// = EnumerateSectors(parent.Address, dirSectorCount).Last_();
            long dirWriteAddress;// = dirSector + dirIndex;

            if (parent.Files.Count == 0)
            {
                if (dirIndex == 0)
                {
                    long lastSector = EnumerateSectors(parent.Address, dirSectorCount - 1).Last_();
                    long addressToNextSector = lastSector + NextSectorIndex;

                    long nextSector = FindFreeSector();
                    nextSector.GetBytes(bytes, 0);
                    _stream.WriteAt(addressToNextSector, bytes, 0, bytes.Length);
                    AllocateSectorAt(nextSector);
                    dirWriteAddress = nextSector;
                }
                else
                    dirWriteAddress = EnumerateSectors(parent.Address, dirSectorCount).Last_() + dirIndex;
            }
            else
            {
                dirSector = EnumerateSectors(parent.Address, dirSectorCount).Last_();
                dirWriteAddress = dirSector + dirIndex;
                int fileSectorCount = FullSectorsAndByteIndex(
                            dirIndex,
                            SectorInfoSize - dirIndex,
                            parent.Files.Count * ADDRESS_BYTES,
                            out long fileIndex);
                long fileWriteAddress;// = fileSector + fileIndex;

                if (fileIndex == 0)
                {
                    long lastSector = EnumerateSectors(dirSector, fileSectorCount - 1).Last_();
                    long addressToNextSector = lastSector + NextSectorIndex;

                    long nextSector = FindFreeSector();
                    nextSector.GetBytes(bytes, 0);
                    _stream.WriteAt(addressToNextSector, bytes, 0, bytes.Length);
                    AllocateSectorAt(nextSector);
                    fileWriteAddress = nextSector;
                }
                else
                    fileWriteAddress = EnumerateSectors(dirSector, fileSectorCount).Last_() + fileIndex;

                _stream.ReadAt(dirWriteAddress, bytes, 0, bytes.Length);
                _stream.WriteAt(fileWriteAddress, bytes, 0, bytes.Length);
            }

            newDirSector = FindFreeSector();
            newDirSector.GetBytes(bytes, 0);
            _stream.WriteAt(dirWriteAddress, bytes, 0, bytes.Length);
            AllocateSectorAt(newDirSector);
            return newDirSector;
        }

        internal long CreatFile(Directory parent)
        {
            SetByteCountAt(parent.Address, parent.ByteCount + ADDRESS_BYTES);
            byte[] bytes = new byte[ADDRESS_BYTES];

            int sectorCount = FullSectorsAndByteIndex(parent.ObjectCount * ADDRESS_BYTES, out long fileIndex);
            long fileWriteAddress;

            if (fileIndex == 0)
            {
                long lastSector = EnumerateSectors(parent.Address, sectorCount - 1).Last_();
                long addressToNextSector = lastSector + NextSectorIndex;

                long nextSector = FindFreeSector();
                nextSector.GetBytes(bytes, 0);
                _stream.WriteAt(addressToNextSector, bytes, 0, bytes.Length);
                AllocateSectorAt(nextSector);
                fileWriteAddress = nextSector;
            }
            else
                fileWriteAddress = EnumerateSectors(parent.Address, sectorCount).Last_() + fileIndex;

            long newFileSector = FindFreeSector();
            newFileSector.GetBytes(bytes, 0);
            _stream.WriteAt(fileWriteAddress, bytes, 0, bytes.Length);
            AllocateSectorAt(newFileSector);
            return newFileSector;
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

        internal void RemoveFile(Directory parent, int fileIndex)
        {
            byte[] bytes = new byte[ADDRESS_BYTES];
            int fileSectorIndex = FullSectorsAndByteIndex((parent.SubDirectories.Count + fileIndex) * ADDRESS_BYTES, out long fileShortIndex);
            long fileWriteSector = EnumerateSectors(parent.Address, fileSectorIndex, true).Last_();
            long fileWriteAddress = fileWriteSector + fileShortIndex;
            long fileReadAddress = EnumerateSectors(
                    fileWriteSector,
                    FullSectorsAndByteIndex(fileShortIndex, SectorInfoSize - fileShortIndex, (parent.Files.Count - 1 - fileIndex) * ADDRESS_BYTES, out long fileReadIndex))
                .Last_() + fileReadIndex;

            _stream.ReadAt(fileReadAddress, bytes, 0, bytes.Length);
            _stream.WriteAt(fileWriteAddress, bytes, 0, bytes.Length);

            if (fileReadAddress % SectorSize == 0) FreeSectorAt(fileReadAddress);
        }

        internal void FreeSectorsOf<T>(T obj, bool removeFirst = true) where T : Object
        {
            if (obj is Directory dir)
                foreach (var subObj in dir.EnumerateObjects())
                    FreeSectorsOf(subObj);

            foreach (long address in EnumerateSectors(obj.Address, FullSectorsAndByteIndex(obj.ByteCount, out _), removeFirst))
                FreeSectorAt(address);
        }

        private long FindFreeSector()
        {
            int index = _bitMap.IndexOf(false);
            return index == -1
                ? throw new OutOfMemoryException("No more free sectors")
                : index * SectorSize + RootAddress;
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
                    return false;

            _badSectors.Add(address);
            CheckAllSectors();
            return true;
        }

        private void UpdateResiliancy(long address)
        {
            byte[] sectorBytes = new byte[SectorSize];
            _stream.ReadAt(address, sectorBytes, 0, sectorBytes.Length);
            byte[] bytes = new byte[ResiliencyBytes];
            for (int i = 0; i < RESILIENCY_FACTOR; i++)
                for (int y = 0; y < ResiliencyBytes; y++)
                    bytes[y] ^= sectorBytes[i * RESILIENCY_FACTOR + y];

            _stream.WriteAt(address + ResiliencyIndex, bytes, 0, bytes.Length);
        }

        private void CheckAllSectors()
        {

        }

        void IDisposable.Dispose() => Close();

        internal readonly struct Sector : IDisposable
        {
            private readonly FileSystem _fileSystem;
            public long Address { get; }

            public Sector(FileSystem fileSystem, long address)
            {
                _fileSystem = fileSystem;
                Address = address;
            }

            public void Dispose()
            {
                _fileSystem.UpdateResiliancy(Address);
            }
        }
    }
}
