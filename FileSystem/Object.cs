using System;
using System.IO;
using System.Text;
using CustomCollections;
using ExceptionsNS;
using static Core.Constants;
using static FileSystemNS.Constants;

namespace FileSystemNS
{
    public abstract class Object
    {
        private string _fullName;

        private protected FileSystem FileSystem { get; }

        internal long Address { get; }
        internal long ByteCount { get; private set; }
        internal ObjectFlags ObjectFlags { get; private set; }

        public Directory Root { get; private set; }
        public string Name { get; private set; }
        public string FullName => _fullName ?? GetFullName();

        private protected Object(FileSystem fileSystem, long address, ObjectFlags objectFlags, string name, long byteCount) // Only for root directory
        {
            FileSystem = fileSystem;
            Address = address;
            ObjectFlags = objectFlags;
            ByteCount = byteCount;
            Name = name;
        }

        private protected Object(FileSystem fileSystem, Directory root, long address, string name, ObjectFlags objectFlags)
        {
            FileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem), Exceptions.CANNOT_BE_NULL);
            Root = root ?? throw new ArgumentNullException(nameof(root), Exceptions.CANNOT_BE_NULL);
            Address = address >= 0 ? address : throw new ArgumentOutOfRangeException(nameof(address), Exceptions.CANNOT_BE_NEGATIVE);
            Name = name ?? throw new ArgumentNullException(nameof(name), Exceptions.CANNOT_BE_NULL);
            Name = name.Length <= NAME_MAX_LENGTH ? name : throw new ArgumentOutOfRangeException($"{nameof(name)}.{nameof(name.Length)}", Exceptions.ARR_MAX_CAPACITY_EXCEEDED);
            ObjectFlags = objectFlags;
        }

        internal abstract void DeserializeBytes(byte[] bytes);
        internal abstract byte[] SerializeBytes();

        private string GetFullName()
        {
            StringBuilder_ sb = new StringBuilder_();
            Object curr = this;
            sb.Prepend(curr.Name);
            curr = curr.Root;
            while (!(curr is null))
            {
                sb.Prepend("\\").Prepend(curr.Name);
                curr = curr.Root;
            }
            _fullName = sb.ToString();
            return _fullName;
        }

        public SetNameResult TrySetName(string name)
        {
            if (name is null) return SetNameResult.NameWasNull;
            if (name == "") return SetNameResult.NameWasEmpty;
            if (name.Length > NAME_MAX_LENGTH) return SetNameResult.NameExceededMaxLength;

            Name = name;
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
