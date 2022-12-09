using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace FileSystemNS
{
    public sealed class File : Object
    {
        private static readonly IFormatter _formatter = new BinaryFormatter();
        public object Object { get; set; }

        internal File(FileSystem fileSystem, Directory parent, long address, ObjectFlags objectFlags, string name, long byteCount)
            : base(fileSystem, parent, address, objectFlags, name, byteCount)
        {
        }

        public void Save()
        {

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
    }
}
