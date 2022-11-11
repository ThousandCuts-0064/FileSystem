using System;
using System.Collections.Generic;
using ExceptionsNS;
using CustomQuery;
using CustomCollections;
using Text;
using System.Text;
using static FileSystem.Constants;

namespace FileSystem
{
    internal class Directory
    {
        public const int SIZE_IN_BYTES = 1024;
        public const int NAME_MAX_CHARS = 32;
        public const int NAME_MAX_BYTES = NAME_MAX_CHARS * UNICODE_SYMBOL_BYTES;
        public const int SUBDIRECTORY_MAX_COUNT = 256;
        private readonly IList<Directory> _subDirectories;
        public string Name { get; private set; }
        public Directory Root { get; private set; }
        public IReadOnlyList<Directory> SubDirectories { get; }

        private Directory(string name, Directory root, IList<Directory> subDirectories)
        {
            Name = name;
            Root = root;
            _subDirectories = subDirectories;
            SubDirectories = subDirectories.ToReadOnly();
        }

        public static DeserializeResult TryDeserialize(byte[] bytes, out Directory directory)
        {
            directory = null;
            if (bytes is null) return DeserializeResult.ByteArrayWasNull;
            if (bytes.Length != SIZE_IN_BYTES) return DeserializeResult.ByteArrayLengthMismatch;

            directory = new Directory(Encoding.Unicode.GetString(bytes, 1, NAME_MAX_BYTES), null, null);
            return DeserializeResult.Success;
        }

        public SetNameResult TrySetName(string name)
        {
            if (name is null) return SetNameResult.NameWasNull;
            if (name == "") return SetNameResult.NameWasEmpty;
            if (name.Length > NAME_MAX_CHARS) return SetNameResult.NameExceededMaxLength;

            Name = name;
            return SetNameResult.Success;
        }

        public SetRootResult TrySetRoot(Directory directory)
        {
            return SetRootResult.Success;
        }

        public byte[] ToBytes()
        {
            return null;
        }

        public enum DeserializeResult : byte
        {
            None,
            Success,
            ByteArrayWasNull,
            ByteArrayLengthMismatch,
        }

        public enum SetNameResult : byte
        {
            None,
            Success,
            NameWasNull,
            NameWasEmpty,
            NameExceededMaxLength
        }

        public enum SetRootResult : byte
        {
            None,
            Success,
        }
    }
}
