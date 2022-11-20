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
        private readonly BitArray_ _bitMap;
        internal FileStream Stream { get; }
        public ushort SectorSize { get; }
        public ulong SectorCount { get; }
        public ulong TotalSize { get; }
        public ulong BitmapBytes { get; }
        public Directory RootDir { get; }

        internal FileSystem(FileStream fileStream)
        {
            Stream = fileStream ?? throw new ArgumentNullException(nameof(fileStream), Exceptions.CANNOT_BE_NULL);
            byte[] bytes = new byte[BOOT_SECTOR_SIZE];
            fileStream.Position = 0;
            fileStream.Read(bytes, 0, BOOT_SECTOR_SIZE);

            SectorSize = bytes.ToUShort(SECTOR_SIZE_INDEX);
            SectorCount = bytes.ToULong(SECTOR_COUNT_INDEX);
            TotalSize = bytes.ToULong(TOTAL_SIZE_INDEX);

            BitmapBytes += SectorCount / BYTE_BITS; // Reserved for bitmap.
            if (SectorCount % BYTE_BITS > 0) BitmapBytes++; // Giving a whole byte for the reminder.

            bytes = new byte[(int)BitmapBytes];
            fileStream.Read(bytes, 0, (int)BitmapBytes); // BitmapBytes may be too large

            _bitMap = new BitArray_(bytes, (int)SectorCount);
            RootDir = _bitMap[0] 
                ? Directory.LoadRoot(this) 
                : Directory.CreateRoot(this);
        }


        public string GetHex()
        {
            StringBuilder_ sb = new StringBuilder_();
            Stream.Position = 0;
            int hex = Stream.ReadByte();
            while (hex != -1)
            {
                sb.Append(((byte)hex).ToHex_()).Append(" ");
                hex = Stream.ReadByte();
            }
            return sb.ToString();
        }

        public string GetBin()
        {
            StringBuilder_ sb = new StringBuilder_();
            Stream.Position = 0;
            int bin = Stream.ReadByte();
            while (bin != -1)
            {
                sb.Append(((byte)bin).ToBin_()).Append(" ");
                bin = Stream.ReadByte();
            }
            return sb.ToString();
        }

        public void Close() => Stream.Close();

        internal long FindFreeSector() => (long)_bitMap.IndexOf(false) * SectorSize;

        internal void AllocateSector(long address)
        {
            int index = (int)(address / SectorSize);
            _bitMap[index] = _bitMap[index]
                ? throw new ArgumentException("Sector is already in use.", nameof(address))
                : true;
            Stream.Position = BITMAP_INDEX + index;
            Stream.WriteByte(_bitMap.GetByte(index));
        }

        void IDisposable.Dispose() => Close();
    }    
}
