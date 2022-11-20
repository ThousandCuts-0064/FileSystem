using System;
using System.Collections.Generic;
using ExceptionsNS;
using CustomQuery;
using CustomCollections;
using Text;
using System.Text;
using static Core.Constants;
using static FileSystemNS.Constants;

namespace FileSystemNS
{
    public sealed class Directory : Object
    {
        private readonly IList<Directory> _subDirectories;
        public IReadOnlyList<Directory> SubDirectories { get; }

        private Directory(FileSystem fileSystem, bool isNew) : base(fileSystem, isNew) { } // Only for root directory

        private Directory(FileSystem fileSystem, Directory root, long address) : base(fileSystem, root, address) { }

        private Directory(FileSystem fileSystem, Directory root, long address, string name, ObjectFlags objectFlags, IList<Directory> subDirectories) : base(fileSystem, root, address, name, objectFlags)
        {
            _subDirectories = subDirectories;
            SubDirectories = subDirectories.ToReadOnly();
        }

        internal static Directory CreateRoot(FileSystem fileSystem) => new Directory(fileSystem, true);
        internal static Directory LoadRoot(FileSystem fileSystem) => new Directory(fileSystem, false);

        public Directory CreateSubdirectory(string name) =>
            new Directory(FileSystem, this, FileSystem.FindFreeSector());

        public SetRootResult TrySetRoot(Directory directory)
        {
            return SetRootResult.Success;
        }

        public byte[] ToBytes()
        {
            return null;
        }

        public enum SetRootResult : byte
        {
            None,
            Success,
        }
    }
}
