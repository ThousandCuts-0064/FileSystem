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
    public class Directory : FileSystem.Object
    {
        private readonly IList<Directory> _subDirectories;
        public Directory Root { get; private set; }
        public IReadOnlyList<Directory> SubDirectories { get; }

        public Directory(long address) : base(address) { }

        private Directory(long address, string name, ObjectFlags objectFlags, Directory root, IList<Directory> subDirectories) : base(address, name, objectFlags)
        {
            Root = root;
            _subDirectories = subDirectories;
            SubDirectories = subDirectories.ToReadOnly();
        }
       

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
