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
    public static class FileSystem
    {
        private static FileStream _stream;
        public static ushort SectorBytes { get; private set; }
        public static ulong SectorCount { get; private set; }
        public static ulong BitmapBytes { get; private set; }
        public static Directory RootDir { get; private set; }

        public static void Open(FileStream fileStream)
        {
            _stream = fileStream;
            byte[] bytes = new byte[BOOT_SECTOR_SIZE];
            fileStream.Read(bytes, 0, BOOT_SECTOR_SIZE);

            SectorBytes = bytes.ToUShort(SECTOR_SIZE_INDEX);
            SectorCount = bytes.ToUInt(SECTOR_COUNT_INDEX);
            BitmapBytes += SectorCount / BYTE_BITS; // Reserved for bitmap.
            if (BitmapBytes % BYTE_BITS > 0) BitmapBytes++; // Giving a whole byte for the reminder.
            long rootAddress = BOOT_SECTOR_SIZE + (long)BitmapBytes;
            RootDir = new Directory(rootAddress);
        }

        public static string ToHex()
        {
            StringBuilder_ sb = new StringBuilder_();
            _stream.Position = 0;
            int hex = _stream.ReadByte();
            while (hex != -1)
            {
                sb.Append(((byte)hex).ToHex_()).Append(" ");
                hex = _stream.ReadByte();
            }
            return sb.ToString();
        }

        public static string ToBin()
        {
            StringBuilder_ sb = new StringBuilder_();
            _stream.Position = 0;
            int bin = _stream.ReadByte();
            while (bin != -1)
            {
                sb.Append(((byte)bin).ToBin_()).Append(" ");
                bin = _stream.ReadByte();
            }
            return sb.ToString();
        }

        public static void Close() => _stream.Close();

        public class Object : IDisposable
        {
            public const int NAME_INDEX = 1;
            public const int OBJECT_FLAGS_INDEX = 0;

            private string _name;
            private ObjectFlags _objectFlags;
            public string Name => _name ?? DeserializeName();
            public long Adress { get; }
            public ObjectFlags ObjectFlags => ObjectFlags == default ? DeserializeObjectFlags() : _objectFlags;

            public Object(long adress) => 
                Adress = adress >= 0 ? adress : throw new ArgumentOutOfRangeException(nameof(adress), Exceptions.CANNOT_BE_NEGATIVE);

            public Object(long adress, string name, ObjectFlags objectFlags)
            {
                Adress = adress >= 0 ? adress : throw new ArgumentOutOfRangeException(nameof(adress), Exceptions.CANNOT_BE_NEGATIVE);
                _name = name.Length <= NAME_MAX_LENGTH ? name : throw new ArgumentOutOfRangeException($"{nameof(name)}.{nameof(name.Length)}", Exceptions.ARR_MAX_CAPACITY_EXCEEDED);
                _objectFlags = objectFlags;
            }

            private string DeserializeName()
            {
                byte[] name = new byte[NAME_BYTES];
                _stream.Position = Adress + NAME_INDEX;
                _stream.Read(name, 0, NAME_BYTES);
                _name = Encoding.Unicode.GetString(name);
                return _name;
            }

            private ObjectFlags DeserializeObjectFlags()
            {
                byte[] objectFlags = new byte[1];
                _stream.Position = Adress + OBJECT_FLAGS_INDEX;
                _stream.Read(objectFlags, 0, 1);
                _objectFlags = (ObjectFlags)objectFlags[0];
                return _objectFlags;
            }

            public SetNameResult TrySetName(string name)
            {
                if (name is null) return SetNameResult.NameWasNull;
                if (name == "") return SetNameResult.NameWasEmpty;
                if (name.Length > NAME_MAX_LENGTH) return SetNameResult.NameExceededMaxLength;

                _name = name;
                return SetNameResult.Success;
            }

            public void Dispose()
            {
                throw new NotImplementedException();
            }

            public enum SetNameResult : byte
            {
                None,
                Success,
                NameWasNull,
                NameWasEmpty,
                NameExceededMaxLength
            }
        }
    }    
}
