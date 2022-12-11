using System;
using System.Collections.Generic;
using CustomQuery;
using CustomCollections;
using static FileSystemNS.Constants;

namespace FileSystemNS
{
    public sealed class Directory : Object
    {
        private readonly IList<Directory> _subDirectories;
        private readonly IList<File> _files;
        public IReadOnlyList<Directory> SubDirectories { get; }
        public IReadOnlyList<File> Files { get; }

        private Directory(FileSystem fileSystem, Directory parent, long address, ObjectFlags objectFlags, string name, long byteCount)
            : base(fileSystem, parent, address, objectFlags, name, byteCount)
        {
            _subDirectories = new UnorderedList_<Directory>();
            SubDirectories = _subDirectories.ToReadOnly();
            _files = new UnorderedList_<File>();
            Files = _files.ToReadOnly();
        }

        internal static Directory CreateRoot(FileSystem fileSystem, string name)
        {
            Directory directory = new Directory(
                fileSystem ?? throw new ArgumentNullException(nameof(fileSystem)),
                null,
                fileSystem.RootAddress,
                ObjectFlags.SysFolder,
                ValidatedName(null, name),
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

        public IEnumerable<Object> EnumerateObjects() => _subDirectories.Concat_<Object>(_files);

        public FSResult TryCreateSubdirectory(string name, out Directory directory)
        {
            directory = null;

            var result = ValidateName(this, name);
            if (result != FSResult.Success) return result;

            if (FileSystem.FreeSectors == 0) return FSResult.FSIsFull;

            long address = FileSystem.FindFreeSector();
            directory = new Directory(FileSystem, this, address, ObjectFlags.Folder, name, 0);
            _subDirectories.Add(directory);

            byte[] bytes = new byte[ADDRESS_BYTES];
            address.GetBytes(bytes, 0);
            FileSystem.AppendInfoBytes(this, bytes);
            ByteCount += bytes.Length;

            FileSystem.SerializeProperties(directory);
            FileSystem.AllocateSectorAt(address);
            return FSResult.Success;
        }

        public FSResult TryCreateFile(string name, out File file)
        {
            file = null;
            var result = ValidateName(this, name);
            if (result != FSResult.Success) return result;

            if (FileSystem.FreeSectors == 0) return FSResult.FSIsFull;

            long address = FileSystem.FindFreeSector();
            file = new File(FileSystem, this, address, ObjectFlags.Folder, name, 0);
            _files.Add(file);

            byte[] bytes = new byte[ADDRESS_BYTES];
            address.GetBytes(bytes, 0);
            FileSystem.AppendInfoBytes(this, bytes);
            ByteCount += bytes.Length;

            FileSystem.SerializeProperties(file);
            FileSystem.AllocateSectorAt(address);
            return FSResult.Success;
        }

        public FSResult TryRemoveSubdirectory(string name)
        {
            var result = ValidateName(null, name);
            if (result != FSResult.Success) return result;

            int index = _subDirectories.IndexOf_(dir => dir.Name == name);
            if (index == -1) return FSResult.NameWasNotFound;

            FileSystem.FreeSectorsOf(_subDirectories[index]);
            FileSystem.RemoveObjectFromDirectory(this, index);
            _subDirectories.RemoveAt(index);
            ByteCount -= ADDRESS_BYTES;
            FileSystem.SerializeByteCount(this);

            return FSResult.Success;
        }

        internal override void DeserializeBytes(byte[] bytes)
        {
            int addressCount = bytes.Length / ADDRESS_BYTES;
            ObjectFlags flags;
            long address;
            int i = 0;
            while (i < addressCount)
            {
                address = bytes.GetLong(i++ * ADDRESS_BYTES);
                flags = FileSystem.GetObjectFlagsAt(address);
                if (!flags.HasFlag(ObjectFlags.Folder))
                {
                    AddFile();
                    break;
                }

                _subDirectories.Add(new Directory(
                    FileSystem,
                    this,
                    address,
                    flags,
                    FileSystem.GetNameAt(address),
                    FileSystem.GetByteCountAt(address)));
            }

            while (i < addressCount)
            {
                address = bytes.GetLong(i++ * ADDRESS_BYTES);
                flags = FileSystem.GetObjectFlagsAt(address);
                AddFile();
            }

            void AddFile() => 
                _files.Add(new File(
                    FileSystem,
                    this,
                    address,
                    flags,
                    FileSystem.GetNameAt(address),
                    FileSystem.GetByteCountAt(address)));
        }

        private protected override byte[] OnSerializeBytes()
        {
            byte[] bytes = new byte[(_subDirectories.Count + _files.Count) * ADDRESS_BYTES];
            for (int i = 0; i < _subDirectories.Count; i++)
                _subDirectories[i].Address.GetBytes(bytes, i * ADDRESS_BYTES);
            return bytes;
        }
    }
}
