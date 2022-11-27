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

        private Directory(FileSystem fileSystem, Directory parent, long address, ObjectFlags objectFlags, string name, long byteCount)
            : base(fileSystem, parent, address, objectFlags, name, byteCount)
        {
            _subDirectories = new UnorderedList_<Directory>();
            SubDirectories = _subDirectories.ToReadOnly();
        }

        internal static Directory CreateRoot(FileSystem fileSystem, string name)
        {
            Directory directory = new Directory(
                fileSystem ?? throw new ArgumentNullException(nameof(fileSystem)),
                null,
                fileSystem.RootAddress,
                ObjectFlags.SysFolder,
                ValidatedName(name),
                0);

            fileSystem.SerializeProperties(directory);
            fileSystem.AllocateSectorAt(fileSystem.RootAddress);

            return directory;
        }

        internal static Directory LoadRoot(FileSystem fileSystem)
        {
            Directory directory = new Directory(
                fileSystem ?? throw new ArgumentNullException(nameof(fileSystem)),
                null,
                fileSystem.RootAddress,
                ObjectFlags.SysFolder,
                fileSystem.GetNameAt(fileSystem.RootAddress),
                fileSystem.GetByteCountAt(fileSystem.RootAddress));

            fileSystem.DeserializeAllInfoBytes(directory);

            return directory;
        }

        public Directory CreateSubdirectory(string name)
        {
            long address = FileSystem.FindFreeSector();
            var directory = new Directory(FileSystem, this, address, ObjectFlags.Folder, ValidatedName(name), 0);
            _subDirectories.Add(directory);

            FileSystem.SerializeProperties(directory);
            FileSystem.AllocateSectorAt(address);

            return directory;
        }

        public bool TryRemoveSubdirectory(string name)
        {
            int index = _subDirectories.IndexOf_(dir => dir.Name == name);
            if (index == -1) return false;

            FileSystem.FreeSectors(_subDirectories[index]);
            FileSystem.OverrideInfoBytes(this, index * ADDRESS_BYTES, null);
            _subDirectories.RemoveAt(index);
            return true;
        }

        internal override void DeserializeBytes(byte[] bytes)
        {
            int addressCount = bytes.Length / ADDRESS_BYTES;
            for (int i = 0; i < addressCount; i++)
            {
                long address = bytes.GetLong(i * ADDRESS_BYTES);
                _subDirectories.Add(new Directory(
                    FileSystem,
                    this,
                    address,
                    FileSystem.GetObjectFlagsAt(address),
                    FileSystem.GetNameAt(address),
                    FileSystem.GetByteCountAt(address)));
            }
        }

        private protected override byte[] OnSerializeBytes()
        {
            byte[] bytes = new byte[_subDirectories.Count * ADDRESS_BYTES];
            for (int i = 0; i < _subDirectories.Count; i++)
                _subDirectories[i].Address.GetBytes(bytes, i * ADDRESS_BYTES);
            return bytes;
        }
    }
}
