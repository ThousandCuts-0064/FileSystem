﻿using System;
using System.Collections.Generic;
using CustomQuery;
using CustomCollections;
using static FileSystemNS.Constants;
using Core;

namespace FileSystemNS
{
    public sealed class Directory : Object
    {
        private readonly UnorderedList_<Directory> _subDirectories;
        private readonly UnorderedList_<File> _files;
        public IReadOnlyList<Directory> SubDirectories { get; }
        public IReadOnlyList<File> Files { get; }
        public int ObjectCount => _subDirectories.Count + _files.Count;

        private Directory(FileSystem fileSystem, Directory parent, long address, ObjectFlags objectFlags, string name, long byteCount)
            : base(fileSystem, parent, address, objectFlags, name, byteCount)
        {
            if (!objectFlags.HasFlag(ObjectFlags.Directory)) throw new InvalidOperationException($"{nameof(ObjectFlags.Directory)} flag was missing.");

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
                ObjectFlags.SysDir,
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
                ObjectFlags.SysDir,
                fileSystem.GetNameAt(fileSystem.RootAddress),
                fileSystem.GetByteCountAt(fileSystem.RootAddress));

            fileSystem.DeserializeAllInfoBytes(directory);

            return directory;
        }

        public override FSResult TrySetName(string name)
        {
            var result = base.TrySetName(name);
            if (result == FSResult.Success) 
                RecursiveResetFullName(this);
            return result;
        }

        public IEnumerable<Object> EnumerateObjects() => _subDirectories.Concat_<Object>(_files);

        public FSResult TryFindDirectory(string name, out Directory directory)
        {
            directory = null;

            var result = ValidateName(null, name, true);
            if (result != FSResult.Success)
                return result;

            if (name == FileSystem.RootDirectory.Name)
            {
                directory = FileSystem.RootDirectory;
                return FSResult.Success;
            }

            if (name == CUR_DIR)
            {
                directory = this;
                return FSResult.Success;
            }

            if (name == PAR_DIR)
            {
                if (Parent is null)
                    return FSResult.RootHasNoParent;

                directory = Parent;
                return FSResult.Success;
            }

            directory = _subDirectories.FirstOrDefault_(dir => dir.Name == name);
            return directory is null ? FSResult.NameWasNotFound : FSResult.Success;
        }

        public FSResult TryFindDirectory(string path, out Directory directory, out string faultedName)
        {
            var result = TryFollowPath(path, out directory, out string lastName, out faultedName);
            return result == FSResult.Success 
                ? directory.TryFindDirectory(lastName, out directory) 
                : result;
        }

        public FSResult TryFindFile(string name, out File file)
        {
            file = null;

            var result = ValidateName(null, name);
            if (result != FSResult.Success)
                return result;

            file = _files.FirstOrDefault_(f => f.Name == name);
            return file is null ? FSResult.NameWasNotFound : FSResult.Success;
        }

        public FSResult TryFindFile(string path, out File file, out string faultedName)
        {
            file = null;
            var result = TryFollowPath(path, out Directory directory, out string lastName, out faultedName);
            return result == FSResult.Success
                ? directory.TryFindFile(lastName, out file)
                : result;
        }

        public FSResult TryFindObject(string name, out Object obj)
        {
            obj = null;

            var result = ValidateName(null, name, true);
            if (result != FSResult.Success)
                return result;

            if (name == CUR_DIR)
            {
                obj = this;
                return FSResult.Success;
            }

            if (name == PAR_DIR)
            {
                if (Parent is null)
                    return FSResult.RootHasNoParent;

                obj = Parent;
                return FSResult.Success;
            }

            obj = EnumerateObjects().FirstOrDefault_(f => f.Name == name);
            return obj is null ? FSResult.NameWasNotFound : FSResult.Success;
        }

        public FSResult TryFindObject(string path, out Object obj, out string faultedName)
        {
            obj = null;
            var result = TryFollowPath(path, out Directory directory, out string lastName, out faultedName);
            return result == FSResult.Success
                ? directory.TryFindObject(lastName, out obj)
                : result;
        }

        public FSResult TryFollowPath(string path, out Directory preLastDir, out string lastName, out string faultedName, bool create = false)
        {
            preLastDir = this;
            lastName = null;
            faultedName = null;

            string[] names = path.Split('\\');
            int i = -1;
            if (names[0] == FileSystem.RootDirectory.Name)
            {
                preLastDir = FileSystem.RootDirectory;
                i++;
            }

            if (names.Length == 1)
            {
                lastName = names[0];
                return FSResult.Success;
            }

            while (++i < names.Length - 1)
            {
                if (names[i].Length > NAME_MAX_LENGTH)
                {
                    faultedName = names[i];
                    return FSResult.NameExceededMaxLength;
                }

                if (names[i] == CUR_DIR)
                {
                    preLastDir = this;
                    continue;
                }

                if (names[i] == PAR_DIR)
                {
                    if (preLastDir.Parent is null)
                    {
                        faultedName = names[i];
                        return FSResult.RootHasNoParent;
                    }

                    preLastDir = preLastDir.Parent;
                    continue;
                }

                int index = preLastDir.SubDirectories.IndexOf_(dir => dir.Name == names[i]);
                if (index != -1)
                {
                    preLastDir = preLastDir.SubDirectories[index];
                    continue;
                }

                if (!create)
                {
                    faultedName = names[i];
                    return FSResult.NameWasNotFound;
                }

                var result = preLastDir.TryCreateDirectory(names[i], out preLastDir);
                if (result != FSResult.Success)
                {
                    faultedName = names[i];
                    return result;
                }
            }

            lastName = names.Last_();
            return FSResult.Success;
        }

        public FSResult TryCreateDirectory(string name, out Directory directory)
        {
            directory = null;

            var result = ValidateName(this, name);
            if (result != FSResult.Success) return result;

            if (FileSystem.FreeSectors == 0) return FSResult.NotEnoughSpace;

            long address = FileSystem.FindFreeSector();
            FileSystem.CreateSubdirectory(this, address);

            directory = new Directory(FileSystem, this, address, ObjectFlags.Directory, name, 0);
            _subDirectories.Add(directory);
            ByteCount += ADDRESS_BYTES;
            _files.CycleLeft();

            FileSystem.SerializeProperties(directory);
            FileSystem.AllocateSectorAt(address);
            return FSResult.Success;
        }

        public FSResult TryCreateDirectory(string path, out Directory directory, out string faultedName)
        {
            var result = TryFollowPath(path, out directory, out string lastName, out faultedName, true);
            return result == FSResult.Success
                ? directory.TryCreateDirectory(lastName, out directory)
                : result;
        }

        public FSResult TryCreateFile(string name, out File file)
        {
            file = null;

            var result = ValidateName(this, name);
            if (result != FSResult.Success) return result;

            result = File.ValidateFormat(name);
            if (result != FSResult.Success) return result;

            if (FileSystem.FreeSectors == 0) return FSResult.NotEnoughSpace;

            long address = FileSystem.FindFreeSector();
            FileSystem.CreatFile(this, address);

            file = new File(FileSystem, this, address, ObjectFlags.None, name, 0);
            _files.Add(file);
            ByteCount += ADDRESS_BYTES;

            FileSystem.SerializeProperties(file);
            FileSystem.AllocateSectorAt(address);
            return FSResult.Success;
        }

        public FSResult TryCreateFile(string path, out File file, out string faultedName)
        {
            file = null;
            var result = TryFollowPath(path, out Directory directory, out string lastName, out faultedName, true);
            return result == FSResult.Success
                ? directory.TryCreateFile(lastName, out file)
                : result;
        }

        public FSResult TryCopyFile(File file)
        {
            file.Load();
            if (FileSystem.FreeSectors < 
                1 + Math_.DivCeiling(file.ByteCount - FileSystem.FirstSectorInfoSize, FileSystem.SectorInfoSize)) 
                return FSResult.NotEnoughSpace;

            var result = TryCreateFile(file.Name, out File newFile);
            if (result != FSResult.Success)
                return result;

            result = newFile.TrySetObject(file.GetObjectByByteCopy());
            if (result != FSResult.Success)
                return result;

            newFile.Save();

            return FSResult.Success;
        }

        public FSResult TryRemoveDirectory(string name)
        {
            var result = ValidateName(null, name);
            if (result != FSResult.Success) return result;

            int index = _subDirectories.IndexOf_(dir => dir.Name == name);
            if (index == -1) return FSResult.NameWasNotFound;

            FileSystem.FreeSectorsOf(_subDirectories[index]);
            FileSystem.RemoveSubdirectory(this, index);
            _subDirectories.RemoveAt(index);
            ByteCount -= ADDRESS_BYTES;
            _files.CycleRight();

            FileSystem.SerializeByteCount(this);
            return FSResult.Success;
        }

        public FSResult TryRemoveDirectory(string path, out string faultedName)
        {
            var result = TryFollowPath(path, out Directory preLastDir, out string lastName, out faultedName);
            return result == FSResult.Success
                ? preLastDir.TryRemoveDirectory(lastName)
                : result;
        }

        public FSResult TryRemoveFile(string name)
        {
            var result = ValidateName(null, name);
            if (result != FSResult.Success) return result;

            int index = _files.IndexOf_(dir => dir.Name == name);
            if (index == -1) return FSResult.NameWasNotFound;

            FileSystem.FreeSectorsOf(_files[index]);
            FileSystem.RemoveFile(this, index);
            _files.RemoveAt(index);
            ByteCount -= ADDRESS_BYTES;

            FileSystem.SerializeByteCount(this);
            return FSResult.Success;
        }

        public FSResult TryRemoveFile(string path, out string faultedName)
        {
            var result = TryFollowPath(path, out Directory preLastDir, out string lastName, out faultedName);
            return result == FSResult.Success
                ? preLastDir.TryRemoveFile(lastName)
                : result;
        }

        public override void Clear()
        {
            foreach (var dir in _subDirectories)
                FileSystem.FreeSectorsOf(dir);
            _subDirectories.Clear();
            base.Clear();
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
                if (!flags.HasFlag(ObjectFlags.Directory))
                {
                    AddFile();
                    break;
                }

                var dir = new Directory(
                    FileSystem,
                    this,
                    address,
                    flags,
                    FileSystem.GetNameAt(address),
                    FileSystem.GetByteCountAt(address));
                _subDirectories.Add(dir);
                FileSystem.DeserializeAllInfoBytes(dir);
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
