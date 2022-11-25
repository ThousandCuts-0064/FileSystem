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
        private readonly ICollection<Directory> _subDirectories;
        public IReadOnlyCollection<Directory> SubDirectories { get; }

        private Directory(FileSystem fileSystem, long address, ObjectFlags objectFlags, string name, long byteCount) 
            : base(fileSystem, address, objectFlags, name, byteCount) 
        {
            _subDirectories = new LinkedList_<Directory>();
            SubDirectories = _subDirectories.ToReadOnly();
        } // Only for root directory

        private Directory(FileSystem fileSystem, Directory root, long address, string name, ObjectFlags objectFlags) 
            : base(fileSystem, root, address, name, objectFlags) { }

        internal static Directory CreateRoot(FileSystem fileSystem)
        {
            string name = nameof(Root);
            ObjectFlags objectFlags = ObjectFlags.System | ObjectFlags.Folder;

            Directory directory = new Directory(fileSystem, fileSystem.RootAddress, objectFlags, name, 0);

            fileSystem.AllocateSectorAt(fileSystem.RootAddress);
            fileSystem.SerializeProperties(directory);

            return directory;
        }

        internal static Directory LoadRoot(FileSystem fileSystem)
        {
            string name = nameof(Root);
            ObjectFlags objectFlags = ObjectFlags.System | ObjectFlags.Folder;

            long byteCount = fileSystem.GetByteCountAt(fileSystem.RootAddress);
            Directory directory = new Directory(fileSystem, fileSystem.RootAddress, objectFlags, name, byteCount);
            fileSystem.DeserializeAllInfoBytes(directory);

            return directory;
        }

        internal override void DeserializeBytes(byte[] bytes)
        {
            
        }

        internal override byte[] SerializeBytes()
        {
            byte[] bytes = new byte[_subDirectories.Count * ADDRESS_BYTES];
            int i = 0;
            foreach (var dir in _subDirectories)
            {
                for (int y = 0; y < ADDRESS_BYTES; y++)
                    bytes[i * ADDRESS_BYTES + y] = dir.Address.GetByte(y);
                i++;
            }
            return bytes;
        }

        public Directory CreateSubdirectory(string name)
        {
            long address = FileSystem.FindFreeSector();
            var directory = new Directory(FileSystem, this, address, name, ObjectFlags.Folder);
            _subDirectories.Add(directory);

            FileSystem.AllocateSectorAt(address);
            FileSystem.SerializeProperties(directory);

            return directory;
        }

        public void RemoveSubdirectory(string nane)
        {
            
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
