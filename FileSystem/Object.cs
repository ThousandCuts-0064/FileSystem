using System;
using System.IO;
using System.Text;
using CustomCollections;
using ExceptionsNS;
using static Core.Constants;
using static FileSystemNS.Constants;

namespace FileSystemNS
{
    public class Object
    {
        internal const int NAME_INDEX = 1;
        internal const int OBJECT_FLAGS_INDEX = 0;

        private string _name;
        private string _fullName;
        private ObjectFlags _objectFlags;
        private protected readonly FileSystem FileSystem;
        private protected FileStream Stream => FileSystem.Stream;
        public Directory Root { get; private set; }
        public string Name => _name ?? DeserializeName();
        public string FullName => _fullName ?? GetFullName();
        public long Adress { get; }
        public ObjectFlags ObjectFlags => ObjectFlags == default ? DeserializeObjectFlags() : _objectFlags;

        private protected Object(FileSystem fileSystem, bool isNew) // Only for root directory
        {
            FileSystem = fileSystem;
            Adress = BOOT_SECTOR_SIZE + (long)FileSystem.BitmapBytes;
            _name = nameof(Root);
            _objectFlags = ObjectFlags.System | ObjectFlags.Folder;

            if (isNew)
            {
                fileSystem.AllocateSector(Adress);
                Stream.Position = Adress;
                Stream.WriteByte((byte)_objectFlags);
                byte[] name = Encoding.Unicode.GetBytes(_name);
                Stream.Write(name, 0, name.Length);
            }
        }

        private protected Object(FileSystem fileSystem, Directory root, long adress)
        {
            FileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem), Exceptions.CANNOT_BE_NULL);
            Root = root ?? throw new ArgumentNullException(nameof(root), Exceptions.CANNOT_BE_NULL);
            Adress = adress >= 0 ? adress : throw new ArgumentOutOfRangeException(nameof(adress), Exceptions.CANNOT_BE_NEGATIVE);
        }

        private protected Object(FileSystem fileSystem, Directory root, long adress, string name, ObjectFlags objectFlags) : this(fileSystem, root, adress)
        {
            _name = name.Length <= NAME_MAX_LENGTH ? name : throw new ArgumentOutOfRangeException($"{nameof(name)}.{nameof(name.Length)}", Exceptions.ARR_MAX_CAPACITY_EXCEEDED);
            _objectFlags = objectFlags;
        }

        private string DeserializeName()
        {
            byte[] name = new byte[NAME_BYTES];
            Stream.Position = Adress + NAME_INDEX;
            Stream.Read(name, 0, NAME_BYTES);
            _name = Encoding.Unicode.GetString(name);
            return _name;
        }

        private string GetFullName()
        {
            StringBuilder_ sb = new StringBuilder_();
            Object curr = this;
            do
            {
                sb.Prepend(curr.Name).Prepend("\\");
                curr = curr.Root;
            }
            while (!(curr is null));
            _fullName = sb.ToString();
            return _fullName;
        }

        private ObjectFlags DeserializeObjectFlags()
        {
            byte[] objectFlags = new byte[1];
            Stream.Position = Adress + OBJECT_FLAGS_INDEX;
            Stream.Read(objectFlags, 0, 1);
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
