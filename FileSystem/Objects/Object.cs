using System;
using System.Collections.Generic;
using System.Diagnostics;
using CustomCollections;
using CustomQuery;
using ExceptionsNS;
using static FileSystemNS.Constants;

namespace FileSystemNS
{
    [DebuggerDisplay("{" + nameof(FullName) + "}")]
    public abstract class Object
    {
        private string _fullName;

        internal long Address { get; private protected set; }
        internal ObjectFlags ObjectFlags { get; private set; }

        public long ByteCount { get; private protected set; }
        public string Name { get; private set; }
        public Directory Parent { get; private set; }
        public FileSystem FileSystem { get; }
        public string FullName => _fullName ?? EvaluateFullName();

        private protected Object(FileSystem fileSystem, Directory parent, long address, ObjectFlags objectFlags, string name, long byteCount)
        {
            FileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
            if (address <= 0) throw new NumberNotPositiveException(nameof(address));
            Parent = parent;
            Address = Parent is null
                ? address == fileSystem.RootAddress
                    ? address
                    : throw new ArgumentException($"{nameof(parent)} = null is only possible for {fileSystem.RootAddress}", nameof(address))
                : address != fileSystem.RootAddress
                    ? address
                    : throw new ArgumentException($"{nameof(address)} = {nameof(fileSystem.RootAddress)} is only possible for {nameof(fileSystem.RootDirectory)} ({nameof(parent)} = null)", nameof(address));
            ObjectFlags = objectFlags;
            Name = ValidatedName(Parent, name);
            ByteCount = byteCount;
        }

        internal static string ValidatedName(Directory parent, string name) =>
            name is null
                ? throw new ArgumentNullException(nameof(name))
                : name == ""
                    ? throw new CollectionEmptyException(nameof(name))
                    : name.Length > NAME_MAX_LENGTH
                        ? throw new ArgumentOutOfRangeException(nameof(name), $"{nameof(name)} cannot exceed {NAME_MAX_LENGTH}")
                        : name.ContainsAny_(NAME_FORBIDDEN_CHARS)
                            ? throw new ArgumentException($"{nameof(name)} contained a forbidden symbol", nameof(name))
                            : ReservedNames.Contains_(name)
                                ? throw new InvalidOperationException($"The {nameof(name)} \"{name}\" is reserved by the file system.")
                                : parent is null
                                    ? name
                                    : parent.EnumerateObjects().Contains_(obj => obj.Name == name)
                                        ? throw new ArgumentException($"{nameof(Object)} with this {nameof(name)} already exists.", nameof(name))
                                        : name;

        internal static FSResult ValidateName(Directory parent, string name, bool allowReserved = false) =>
            name is null
                ? FSResult.NameWasNull
                : name == ""
                    ? FSResult.NameWasEmpty
                    : name.Length > NAME_MAX_LENGTH
                        ? FSResult.NameExceededMaxLength
                        : name.ContainsAny_(NAME_FORBIDDEN_CHARS)
                            ? FSResult.NameHadForbiddenChar
                            : allowReserved
                                ? FSResult.Success
                                : ReservedNames.Contains_(name)
                                    ? FSResult.NameIsReserved
                                    : parent is null
                                        ? FSResult.Success
                                        : parent.EnumerateObjects().Contains_(obj => obj.Name == name)
                                            ? FSResult.NameWasTaken
                                            : FSResult.Success;

        public virtual FSResult TrySetName(string name)
        {
            if (FileSystem.IsRootCorrupted)
                return FSResult.RootCorrupted;

            var result = ValidateName(Parent, name);
            if (result != FSResult.Success)
                return result;

            Name = name;
            _fullName = null;

            if (!TryGetSector(out var sector))
                return FSResult.BadSectorFound;

            sector.Name = Name;
            sector.UpdateResiliancy();
            return FSResult.Success;
        }

        public virtual FSResult Clear()
        {
            if (FileSystem.IsRootCorrupted)
                return FSResult.RootCorrupted;

            ByteCount = 0;
            if (!TryGetSector(out var sector))
                return FSResult.BadSectorFound;

            FileSystem.FreeSectorsOf(this, false);
            sector.ByteCount = ByteCount;
            sector.UpdateResiliancy();
            return FSResult.Success;
        }

        internal bool TryGetSector(out FileSystem.Sector sector)
        {
            if (FileSystem.TryGetSector(Address, out sector))
                return true;

            Object obj = this;
            while (!(obj.Parent is null) && !obj.TryRemoveFromParent())
                obj = obj.Parent;

            return false;
        }

        internal bool TryUpdateAddress(Object obj, long oldByteCount, FileSystem.Sector newSector)
        {
            if (!FileSystem.TryUpdateSector(this, oldByteCount, newSector))
                return false;

            obj.Address = newSector.Address;
            return true;
        }

        internal abstract bool TryDeserializeBytes(byte[] bytes);

        internal byte[] SerializeBytes()
        {
            byte[] bytes = GetSerializedBytes();
            ByteCount = bytes.Length;
            return bytes;
        }

        internal abstract int GetIndexInParent();

        private protected abstract byte[] GetSerializedBytes();

        private protected abstract bool TryRemoveFromParent();

        private protected void RecursiveResetFullName(Directory currDir)
        {
            currDir._fullName = null;
            foreach (var child in currDir.Directories)
                RecursiveResetFullName(child);

            foreach (var file in currDir.Files)
                file._fullName = null;
        }

        private string EvaluateFullName()
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
    }
}
