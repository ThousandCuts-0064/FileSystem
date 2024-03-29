﻿using System;
using System.Collections.Generic;
using CustomQuery;
using CustomCollections;
using Core;
using Text;
using static FileSystemNS.Constants;

namespace FileSystemNS
{
    public sealed class Directory : Object
    {
        private readonly UnorderedList_<Directory> _directories;
        private readonly UnorderedList_<File> _files;
        public IReadOnlyList<Directory> Directories { get; }
        public IReadOnlyList<File> Files { get; }
        public int ObjectCount => _directories.Count + _files.Count;

        private Directory(FileSystem fileSystem, Directory parent, long address, ObjectFlags objectFlags, string name, long byteCount)
            : base(fileSystem, parent, address, objectFlags, name, byteCount)
        {
            if (!objectFlags.HasFlag(ObjectFlags.Directory)) throw new InvalidOperationException($"{nameof(ObjectFlags.Directory)} flag was missing.");

            _directories = new UnorderedList_<Directory>();
            Directories = _directories.ToReadOnly();
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

            directory.TryGetSector(out var sector);
            sector.SetProperties(directory);
            sector.UpdateResiliancy();
            sector.Allocate();
            return directory;
        }

        internal static Directory LoadRoot(FileSystem fileSystem)
        {
            fileSystem.TryGetSector(fileSystem.RootAddress, out var sector);

            Directory directory = new Directory(
                fileSystem ?? throw new ArgumentNullException(nameof(fileSystem)),
                null,
                fileSystem.RootAddress,
                ObjectFlags.SysDir,
                sector.Name,
                sector.ByteCount);

            sector.TryDeserializeChainTo(directory);
            return directory;
        }

        public override FSResult TrySetName(string name)
        {
            var result = base.TrySetName(name);
            if (result == FSResult.Success)
                RecursiveResetFullName(this);
            return result;
        }

        public IEnumerable<Object> EnumerateObjects() => Directories.Concat_<Object>(Files);

        public FSResult TryFindDirectory(string name, out Directory directory)
        {
            directory = null;

            if (FileSystem.IsRootCorrupted)
                return FSResult.RootCorrupted;

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

            directory = _directories.FirstOrDefault_(dir => dir.Name == name);
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

            if (FileSystem.IsRootCorrupted)
                return FSResult.RootCorrupted;

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

            if (FileSystem.IsRootCorrupted)
                return FSResult.RootCorrupted;

            var result = ValidateName(null, name, true);
            if (result != FSResult.Success)
                return result;

            if (name == FileSystem.RootDirectory.Name)
            {
                obj = FileSystem.RootDirectory;
                return FSResult.Success;
            }

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

        public FSResult TryCreateDirectory(string name, out Directory directory)
        {
            directory = null;

            if (FileSystem.IsRootCorrupted)
                return FSResult.RootCorrupted;

            var result = ValidateName(this, name);
            if (result != FSResult.Success) return result;

            if (FileSystem.FreeSectorCount < 2) // When current directory's last sector gets full, one is needed for the new directory and one for expansion. 
                return FSResult.NotEnoughSpace;

            result = FileSystem.TryCreateDirectory(this, out var sector);
            if (result != FSResult.Success) return result;

            directory = new Directory(FileSystem, this, sector.Address, ObjectFlags.Directory, name, 0);
            _directories.Add(directory);
            ByteCount += ADDRESS_BYTES;
            _files.CycleLeft();

            sector.SetProperties(directory);
            sector.UpdateResiliancy();
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

            if (FileSystem.IsRootCorrupted)
                return FSResult.RootCorrupted;

            var result = ValidateName(this, name);
            if (result != FSResult.Success) return result;

            result = File.ValidateFormat(name);
            if (result != FSResult.Success) return result;

            if (FileSystem.FreeSectorCount < 2) // When current directory's last sector gets full, one is needed for the new file and one for expansion. 
                return FSResult.NotEnoughSpace;

            result = FileSystem.TryCreateFile(this, out var sector);
            if (result != FSResult.Success) return result;

            file = new File(FileSystem, this, sector.Address, ObjectFlags.None, name, 0);
            _files.Add(file);
            ByteCount += ADDRESS_BYTES;

            sector.SetProperties(file);
            sector.UpdateResiliancy();
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

        public FSResult TryCopyFile(File file, string newName = null)
        {
            if (FileSystem.IsRootCorrupted)
                return FSResult.RootCorrupted;

            newName = newName is null
                ? GetFileNameWithRepeatCount(file.Name)
                : newName + '.' + file.Format.ToLower();
            var result = ValidateName(this, newName);
            if (result != FSResult.Success)
                return result;

            file.TryLoad();
            if (FileSystem.FreeSectorCount <
                    1 + Math_.DivCeiling(file.ByteCount - FileSystem.FirstSectorInfoSize, FileSystem.SectorInfoSize))
                return FSResult.NotEnoughSpace;

            result = TryCreateFile(newName, out File newFile);
            if (result != FSResult.Success)
                return result;

            result = newFile.TrySetObject(file.GetObjectDeepCopy());
            if (result != FSResult.Success)
            {
                TryRemoveFile(newFile.Name);
                return result;
            }

            newFile.TrySave();

            return FSResult.Success;
        }

        public FSResult TryRemoveDirectory(string name, out Directory parent)
        {
            parent = this;

            if (FileSystem.IsRootCorrupted)
                return FSResult.RootCorrupted;

            var result = ValidateName(null, name, true);
            if (result != FSResult.Success) return result;

            if (name == FileSystem.RootDirectory.Name)
                return FSResult.RootHasNoParent;

            if (name == CUR_DIR)
            {
                if (Parent is null)
                    return FSResult.RootHasNoParent;

                parent = Parent;
                name = Name;
            }
            else if (name == PAR_DIR)
            {
                if (Parent?.Parent is null)
                    return FSResult.RootHasNoParent;

                parent = Parent.Parent;
                name = Parent.Name;
            }

            int index = parent._directories.IndexOf_(dir => dir.Name == name);
            if (index == -1) return FSResult.NameWasNotFound;

            result = FileSystem.FreeSectorsOf(parent._directories[index]);
            if (result != FSResult.Success) return result;

            result = FileSystem.TryRemoveDirectory(parent, index);
            if (result != FSResult.Success) return result;

            if (index == -1) return result;

            parent._directories.RemoveAt(index);
            parent._files.CycleRight();
            parent.ByteCount -= ADDRESS_BYTES;
            return FSResult.Success;
        }

        public FSResult TryRemoveDirectory(string path, out string faultedName, out Directory parent)
        {
            parent = null;
            var result = TryFollowPath(path, out Directory preLastDir, out string lastName, out faultedName);
            return result == FSResult.Success
                ? preLastDir.TryRemoveDirectory(lastName, out parent)
                : result;
        }

        public FSResult TryRemoveFile(string name)
        {
            if (FileSystem.IsRootCorrupted)
                return FSResult.RootCorrupted;

            var result = ValidateName(null, name);
            if (result != FSResult.Success) return result;

            int index = _files.IndexOf_(dir => dir.Name == name);
            if (index == -1) return FSResult.NameWasNotFound;

            result = FileSystem.FreeSectorsOf(_files[index]);
            if (result != FSResult.Success) return result;

            result = FileSystem.TryRemoveFile(this, index);
            if (result != FSResult.Success) return result;

            _files.RemoveAt(index);
            ByteCount -= ADDRESS_BYTES;
            return FSResult.Success;
        }

        public FSResult TryRemoveFile(string path, out string faultedName)
        {
            var result = TryFollowPath(path, out Directory preLastDir, out string lastName, out faultedName);
            return result == FSResult.Success
                ? preLastDir.TryRemoveFile(lastName)
                : result;
        }

        public string GetDirectoryNameWithRepeatCount(string name)
        {
            if (!_directories.Contains_(d => d.Name == name))
                return name;

            int openBracket = name.LastIndexOf('(');
            ulong count = 0;

            if (openBracket == -1)
                openBracket = name.Length;
            else if (ulong.TryParse(name.SubstringAt_(openBracket + 1, name.Length - 1), out ulong currCount))
                count = currCount;

            for (int i = 0; i < _directories.Count; i++)
            {
                string dName = _directories[i].Name;
                if (dName.Length <= openBracket)
                    continue;

                if (dName[openBracket] == '(' &&
                    ulong.TryParse(dName.SubstringAt_(openBracket + 1, dName.Length - 2), out ulong currCount) &&
                    currCount > count)
                    count = currCount;
            }

            return name + $"({count + 1})";
        }

        public string GetFileNameWithRepeatCount(string name)
        {
            if (!_files.Contains_(f => f.Name == name))
                return name;

            ulong count = 0;
            int dotIndex = name.LastIndexOf_('.');
            string format = name.Substring(dotIndex + 1);

            for (int i = 0; i < _files.Count; i++)
            {
                if (_files[i].Format.ToLower() != format)
                    continue;

                string fName = _files[i].Name;
                int closeBracket = fName.Length - format.Length - 2;
                if (dotIndex < fName.Length &&
                    fName[dotIndex] == '(' &&
                    fName[closeBracket] == ')' &&
                    ulong.TryParse(fName.SubstringAt_(dotIndex + 1, closeBracket - 1), out ulong currCount) &&
                    currCount > count)
                    count = currCount;
            }

            return name.Substring_(0, dotIndex) + $"({count + 1})" + '.' + format;
        }

        public bool IsChildOf(Directory directory)
        {
            Directory parent = Parent;
            while (!(parent is null))
            {
                if (parent == directory)
                    return true;

                parent = parent.Parent;
            }
            return false;
        }

        public override FSResult Clear()
        {
            var result = base.Clear();
            if (result != FSResult.Success)
                return result;

            _directories.Clear();
            _files.Clear();
            return result;
        }

        internal override bool TryDeserializeBytes(byte[] bytes)
        {
            int addressCount = bytes.Length / ADDRESS_BYTES;
            FileSystem.Sector sector;
            ObjectFlags flags;
            int i = 0;
            while (i < addressCount)
            {
                if (!FileSystem.TryGetSector(bytes.GetLong(i++ * ADDRESS_BYTES), out sector))
                    return false;

                flags = sector.ObjectFlags;
                if (!flags.HasFlag(ObjectFlags.Directory))
                {
                    _files.Add(
                        new File(FileSystem, this, sector.Address, flags, sector.Name, sector.ByteCount));
                    break;
                }

                var dir = new Directory(FileSystem, this, sector.Address, flags, sector.Name, sector.ByteCount);
                _directories.Add(dir);
                sector.TryDeserializeChainTo(dir);
            }

            while (i < addressCount)
            {
                if (!FileSystem.TryGetSector(bytes.GetLong(i++ * ADDRESS_BYTES), out sector))
                    return false;

                flags = sector.ObjectFlags;
                _files.Add(new File(
                    FileSystem, this, sector.Address, flags, sector.Name, sector.ByteCount));
            }
            return true;
        }

        internal override int GetIndexInParent() => Parent.Directories.IndexOf_(this);

        private protected override byte[] GetSerializedBytes()
        {
            byte[] bytes = new byte[(_directories.Count + _files.Count) * ADDRESS_BYTES];
            for (int i = 0; i < _directories.Count; i++)
                _directories[i].Address.GetBytes(bytes, i * ADDRESS_BYTES);
            return bytes;
        }

        private protected override bool TryRemoveFromParent() => Parent.TryRemoveDirectory(Name, out _) == FSResult.Success;

        private FSResult TryFollowPath(string path, out Directory preLastDir, out string lastName, out string faultedName, bool create = false)
        {
            preLastDir = this;
            lastName = null;
            faultedName = null;

            if (FileSystem.IsRootCorrupted)
                return FSResult.RootCorrupted;

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

                int index = preLastDir.Directories.IndexOf_(dir => dir.Name == names[i]);
                if (index != -1)
                {
                    preLastDir = preLastDir.Directories[index];
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
    }
}
