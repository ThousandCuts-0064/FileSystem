using System;
using System.Drawing;
using System.IO;
using System.Media;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using Core;
using CustomCollections;
using CustomQuery;
using Text;

namespace FileSystemNS
{
    public sealed class File : Object
    {
        private static readonly IFormatter _formatter = new BinaryFormatter();
        public object Object { get; private set; }
        public FileFormat Format { get; }

        internal File(FileSystem fileSystem, Directory parent, long address, ObjectFlags objectFlags, string name, long byteCount)
            : base(fileSystem, parent, address, objectFlags, name, byteCount)
        {
            if (objectFlags.HasFlag(ObjectFlags.Directory)) throw new InvalidOperationException($"{nameof(ObjectFlags.Directory)} flag was present.");
            Format = Enum.TryParse(name.Substring(name.LastIndexOf_('.') + 1), true, out FileFormat fileFormat)
                ? fileFormat
                : throw new InvalidCastException("Format was invalid.");
        }

        internal static FSResult ValidateFormat(string name) => 
            name.LastIndexOf_('.').Get(out int index) == -1 || index == name.Length - 1
                ? FSResult.FormatNotSpecified
                : FileFormatExt.FormatsAsLower.Contains_(name.Substring_(index + 1))
                    ? FSResult.Success
                    : FSResult.FormatNotSupported;

        public void Save() => FileSystem.SerializeAllInfoBytes(this);
        public void Load() => FileSystem.DeserializeAllInfoBytes(this);

        public override FSResult TrySetName(string name) => 
            base.TrySetName(AttachFormat(name));

        public FSResult TrySetObject(object obj)
        {
            if (obj is null) throw new ArgumentNullException(nameof(obj));

            switch (Format)
            {
                case FileFormat.None: throw new InvalidOperationException($"{nameof(Format)} must be set.");

                case FileFormat.Txt:
                    if (!(obj is string)) return FSResult.FormatMismatch;
                    Object = obj;
                    return FSResult.Success;

                case FileFormat.Bmp:
                    if (!(obj is Bitmap)) return FSResult.FormatMismatch;
                    Object = obj;
                    return FSResult.Success;

                case FileFormat.Wav:
                    if (!(obj is SoundPlayer)) return FSResult.FormatMismatch;
                    Object = obj;
                    return FSResult.Success;

                default: throw new NotSupportedException();
            }
        }

        internal object GetObjectByByteCopy()
        {
            using (var stream = new MemoryStream())
            {
                _formatter.Serialize(stream, Object);
                stream.Position = 0;
                return _formatter.Deserialize(stream);
            }
        }

        internal override void DeserializeBytes(byte[] bytes)
        {
            using (var stream = new MemoryStream())
                Object = _formatter.Deserialize(stream);
        }

        private protected override byte[] OnSerializeBytes()
        {
            using (var stream = new MemoryStream())
            {
                _formatter.Serialize(stream, Object);
                return stream.ToArray();
            }
        }

        private string AttachFormat(string name) => $"{name}.{Format.ToString().ToLowerASCII_()}";
    }
}
