using System;
using System.IO;
using System.Text;
using CustomCollections;
using ExceptionsNS;
using Text;
using static Core.Constants;
using static FileSystemNS.Constants;

namespace FileSystemNS
{
    public sealed class FileSystem : IDisposable
    {
        private readonly FileStream _stream;
        private readonly BitArray_ _bitMap;

        internal ushort SectorSize { get; }
        internal long SectorCount { get; }
        internal long RootAddress { get; }
        internal long AddressNextSectorBytesIndex { get; }
        internal long BitmapBytes => _bitMap.ByteCount;

        public long TotalSize { get; }
        public Directory RootDir { get; private set; }

        // Partial initialization. RootDir required.
        private FileSystem(FileStream fileStream, long totalSize, ushort sectorSize, long sectorCount, BitArray_ bitmap)
        {
            _stream = fileStream;
            TotalSize = totalSize;
            SectorSize = sectorSize;
            SectorCount = sectorCount;
            _bitMap = bitmap;
            int rquiredBytes = BITMAP_INDEX + _bitMap.ByteCount;
            RootAddress = (rquiredBytes / SectorSize + rquiredBytes % SectorSize > 0 ? 1 : 0) * SectorSize;
            AddressNextSectorBytesIndex = SectorCount - ADDRESS_BYTES;
        }

        public static FileSystem Create(FileStream fileStream, long totalSize, ushort sectorSize)
        {
            long sectorCount = totalSize / sectorSize;
            var bitMap = new BitArray_(sectorCount / BYTE_BITS + sectorCount % BYTE_BITS > 0 ? 1 : 0); // BitmapBytes may be too large

            FileSystem fileSystem = new FileSystem(
                fileStream ?? throw new ArgumentNullException(nameof(fileStream), Exceptions.CANNOT_BE_NULL),
                totalSize, sectorSize, sectorCount, bitMap);

            fileSystem.RootDir = Directory.CreateRoot(fileSystem);

            byte[] bootSectorBytes = new byte[BOOT_SECTOR_SIZE];
            totalSize.GetBytes(bootSectorBytes, TOTAL_SIZE_INDEX);
            sectorSize.GetBytes(bootSectorBytes, SECTOR_SIZE_INDEX);
            sectorCount.GetBytes(bootSectorBytes, SECTOR_COUNT_INDEX);

            fileStream.WriteAt(0, bootSectorBytes, 0, BOOT_SECTOR_SIZE);
            fileStream.WriteAt(BITMAP_INDEX, bitMap.GetBytes(), 0, bitMap.ByteCount);
            fileStream.WriteByteAt(0, (byte)BootByte.All); // Finally set the bits for successful initialization

            return fileSystem;
        }

        public static FileSystem Open(FileStream fileStream)
        {
            if (fileStream is null) throw new ArgumentNullException(nameof(fileStream), Exceptions.CANNOT_BE_NULL);

            byte[] bytes = new byte[BOOT_SECTOR_SIZE];
            fileStream.ReadAt(0, bytes, 0, BOOT_SECTOR_SIZE);

            long totalSize = bytes.GetLong(TOTAL_SIZE_INDEX);
            ushort sectorSize = bytes.GetUShort(SECTOR_SIZE_INDEX);
            long sectorCount = bytes.GetLong(SECTOR_COUNT_INDEX);
            bytes = new byte[sectorCount / BYTE_BITS + sectorCount % BYTE_BITS > 0 ? 1 : 0];

            fileStream.ReadAt(BITMAP_INDEX, bytes, 0, bytes.Length);
            BitArray_ bitmap = new BitArray_(bytes);

            FileSystem fileSystem = new FileSystem(fileStream, totalSize, sectorSize, sectorCount, bitmap);

            fileSystem.RootDir = Directory.LoadRoot(fileSystem);

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
                strs.Add(((byte)_stream.ReadByteAt(RootDir.Address)).ToHex_());

                byte[] bytes = new byte[NAME_BYTES];
                _stream.ReadAt(RootDir.Address + 1, bytes, 0, bytes.Length);
                strs.Add(bytes.ToHex_());

                bytes = new byte[LONG_BYTES];
                _stream.ReadAt(RootDir.Address + BYTE_COUNT_INDEX, bytes, 0, bytes.Length);
                strs.Add(bytes.ToHex_());

                bytes = new byte[SectorSize - 1 - NAME_BYTES - LONG_BYTES - ADDRESS_BYTES];
                _stream.ReadAt(RootDir.Address + INFO_BYTES_INDEX, bytes, 0, bytes.Length);
                strs.Add(bytes.ToHex_());

                bytes = new byte[ADDRESS_BYTES];
                _stream.ReadAt(RootDir.Address + AddressNextSectorBytesIndex, bytes, 0, bytes.Length);
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
                strs.Add(((byte)_stream.ReadByteAt(RootDir.Address)).ToBin_());

                byte[] bytes = new byte[NAME_BYTES];
                _stream.ReadAt(RootDir.Address + NAME_INDEX, bytes, 0, bytes.Length);
                strs.Add(bytes.ToBin_());

                bytes = new byte[LONG_BYTES];
                _stream.ReadAt(RootDir.Address + BYTE_COUNT_INDEX, bytes, 0, bytes.Length);
                strs.Add(bytes.ToBin_());

                bytes = new byte[SectorSize - 1 - NAME_BYTES - LONG_BYTES - ADDRESS_BYTES];
                _stream.ReadAt(RootDir.Address + INFO_BYTES_INDEX, bytes, 0, bytes.Length);
                strs.Add(bytes.ToBin_());

                bytes = new byte[ADDRESS_BYTES];
                _stream.ReadAt(RootDir.Address + AddressNextSectorBytesIndex, bytes, 0, bytes.Length);
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
            return Encoding.Unicode.GetString(bytes);
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

        internal void SerializeProperties(Object obj)
        {
            SerializeObjectFlags(obj);
            SerializeName(obj);
            SerializeByteCount(obj);
        }

        internal void SerializeByteCount(Object obj) => SetByteCountAt(obj.Address, obj.ByteCount);
        internal void SetByteCountAt(long address, long byteCount)
        {
            byte[] bytes = new byte[LONG_BYTES];
            byteCount.GetBytes(bytes, 0);
            _stream.WriteAt(address + BYTE_COUNT_INDEX, bytes, 0, LONG_BYTES);
        }

        internal void DeserializeAllInfoBytes(Object obj) => obj.DeserializeBytes(GetAllInfoBytesAt(obj.Address, checked((int)obj.ByteCount)));
        internal byte[] GetAllInfoBytesAt(long address) => GetAllInfoBytesAt(address, checked((int)GetByteCountAt(address)));
        internal byte[] GetAllInfoBytesAt(long address, int count)
        {
            byte[] bytes = new byte[count];
            GetAllInfoBytesAt(address, bytes);
            return bytes;
        }
        internal void GetAllInfoBytesAt(long address, byte[] bytes) => GetAllInfoBytesAt(address, bytes, 0, bytes.Length);
        internal void GetAllInfoBytesAt(long address, byte[] bytes, int offset, int count) => _stream.ReadAt(address + INFO_BYTES_INDEX, bytes, offset, count);

        internal void SerializeAllInfoBytes(Object obj)
        {
            byte[] bytes = obj.SerializeBytes();
            SetByteCountAt(obj.Address, bytes.Length);
            SetInfoBytesAt(obj.Address, bytes);
        }

        internal void SetInfoBytesAt(long address, byte[] bytes) => SetInfoBytesAt(address, bytes, 0, bytes.Length);
        internal void SetInfoBytesAt(long address, byte[] bytes, int offset, int count) => _stream.WriteAt(address + INFO_BYTES_INDEX, bytes, offset, count);

        internal long FindFreeSector() => (long)_bitMap.IndexOf(false) * SectorSize;

        internal void AllocateSectorAt(long address)
        {
            int index = (int)((address - RootAddress) / SectorSize);
            _bitMap[index] = _bitMap[index]
                ? throw new ArgumentException("Sector is already in use.", nameof(address))
                : true;
            _stream.WriteByteAt(BITMAP_INDEX + index, _bitMap.GetByte(index));
        }

        internal void FreeSectorAt(long address)
        {
            int index = (int)((address - RootAddress) / SectorSize);
            _bitMap[index] = _bitMap[index]
                ? false
                : throw new ArgumentException("Sector is already free.", nameof(address));
            _stream.WriteByteAt(BITMAP_INDEX + index, _bitMap.GetByte(index));
        }

        void IDisposable.Dispose() => Close();
    }
}
