using System;
using System.Collections.Generic;
using CustomCollections;
using CustomQuery;
using ExceptionsNS;
using static Core.Constants;
using static FileSystemNS.Constants;

namespace FileSystemNS
{
    public abstract class Object
    {
        private string _fullName;

        internal FileSystem FileSystem { get; }
        internal long Address { get; private set; }
        internal long ByteCount { get; private protected set; }
        internal ObjectFlags ObjectFlags { get; private set; }

        public Directory Parent { get; private set; }
        public string Name { get; private set; }
        public string FullName => _fullName ?? GetFullName();

        private protected Object(FileSystem fileSystem, Directory parent, long address, ObjectFlags objectFlags, string name, long byteCount)
        {
            FileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
            if (address <= 0) throw new ArgumentOutOfRangeException(nameof(address), "Number must be positive");
            Parent = parent;
            Address = Parent is null
                ? address == fileSystem.RootAddress
                    ? address
                    : throw new ArgumentException($"{nameof(parent)} = null is only possible for {fileSystem.RootAddress}", nameof(address))
                : address != fileSystem.RootAddress
                    ? address
                    : throw new ArgumentException($"{nameof(address)} = {nameof(fileSystem.RootAddress)} is only possible for {nameof(fileSystem.RootDirectory)} ({nameof(parent)} = null)", nameof(address));
            ObjectFlags = objectFlags;
            Name = ValidatedName(name);
            ByteCount = byteCount;
        }

        internal abstract void DeserializeBytes(byte[] bytes);

        internal byte[] SerializeBytes()
        {
            byte[] bytes = OnSerializeBytes();
            ByteCount = bytes.Length;
            return bytes;
        }

        private protected static string ValidatedName(string name) => name is null
                ? throw new ArgumentNullException(nameof(name))
                : name == ""
                    ? throw new CollectionEmptyException(nameof(name))
                    : name.Length > NAME_MAX_LENGTH
                        ? throw new ArgumentOutOfRangeException(nameof(name), $"{nameof(name)} cannot exceed {NAME_MAX_LENGTH}")
                        : name.ContainsAny_(NAME_FORBIDDEN_CHARS)
                            ? throw new ArgumentException($"{nameof(name)} contained a forbidden symbol", nameof(name))
                            : name;

        private protected abstract byte[] OnSerializeBytes();

        private string GetFullName()
        {
            StringBuilder_ sb = new StringBuilder_();
            Object curr = this;
            sb.Prepend(curr.Name);
            curr = curr.Parent;
            while (!(curr is null))
            {
                sb.Prepend("\\").Prepend(curr.Name);
                curr = curr.Parent;
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
