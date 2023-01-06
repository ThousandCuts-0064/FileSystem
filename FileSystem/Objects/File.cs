using System;
using System.Drawing;
using System.IO;
using System.Media;
using System.Text;
using System.Windows.Forms;
using Core;
using CustomCollections;
using CustomQuery;
using ExceptionsNS;
using Text;

namespace FileSystemNS
{
    public sealed class File : Object
    {
        public object Object { get; private set; }
        public FileFormat Format { get; }

        internal File(FileSystem fileSystem, Directory parent, long address, ObjectFlags objectFlags, string name, long byteCount)
            : base(fileSystem, parent, address, objectFlags, name, byteCount)
        {
            if (objectFlags.HasFlag(ObjectFlags.Directory)) throw new InvalidOperationException($"{nameof(ObjectFlags.Directory)} flag was present.");
            Format = Enum.TryParse(GetExtension(name), true, out FileFormat fileFormat)
                ? fileFormat
                : throw new InvalidCastException($"{nameof(Format)} was invalid.");
        }

        public static string GetExtension(string name) => name.Substring_(name.LastIndexOf_('.') + 1);
        public static string GetPureName(string name) => name.Substring_(0, name.LastIndexOf_('.'));
        public static string AttachFormat(string name, FileFormat format) => $"{name}.{format.ToLower()}";
        public static string AttachExtension(string name, string ext) => $"{name}.{ext}";

        internal static FSResult ValidateFormat(string name) =>
            name.LastIndexOf_('.').Get(out int index) == -1 || index == name.Length - 1
                ? FSResult.FormatNotSpecified
                : FileFormatExt.FormatsAsLower.Contains_(name.Substring_(index + 1))
                    ? FSResult.Success
                    : FSResult.FormatNotSupported;

        public bool TrySave() =>
            TryGetSector(out var sector) && 
            sector.TryGetLast(checked((int)ByteCount), out _) &&
            sector.TryFindFree(out sector) && 
            sector.TrySerializeChainFrom(this) &&
            TryUpdateAddress(this, sector);

        public bool TryLoad() => 
            TryGetSector(out var sector) && 
            sector.TryDeserializeChainTo(this);

        public override FSResult TrySetName(string name) =>
            base.TrySetName(AttachFormat(name, Format));

        public FSResult TrySetObject(object obj)
        {
            if (!(obj is null))
                switch (Format)
                {
                    case FileFormat.None: throw new InvalidOperationException($"{nameof(Format)} must be set.");

                    case FileFormat.Txt:
                        if (obj is string) break;
                        return FSResult.FormatMismatch;

                    case FileFormat.Rtf:
                        if (obj is RichTextBox) break;
                        return FSResult.FormatMismatch;

                    case FileFormat.Bmp:
                    case FileFormat.Emf:
                    case FileFormat.Wmf:
                    case FileFormat.Gif:
                    case FileFormat.Jpeg:
                    case FileFormat.Png:
                    case FileFormat.Tiff:
                    case FileFormat.Exif:
                    case FileFormat.Icon:
                        if (obj is Image img && img.RawFormat == Format.ToImageFormat()) break;
                        return FSResult.FormatMismatch;

                    case FileFormat.Wav:
                        if (obj is SoundPlayer) break;
                        return FSResult.FormatMismatch;

                    default: throw new UnreachableException($"{nameof(Format)} should have been set correctly by the constructor.");
                }

            Object = obj;
            return FSResult.Success;
        }

        internal object GetObjectDeepCopy()
        {
            if (Object is null)
                return null;

            var stream = new MemoryStream();
            switch (Object)
            {
                case string str:
                    return str;

                case RichTextBox rtb:
                    rtb.SaveFile(stream, RichTextBoxStreamType.RichText);
                    stream.Position = 0;
                    RichTextBox rtbNew = new RichTextBox();
                    rtbNew.LoadFile(stream, RichTextBoxStreamType.RichText);
                    return rtbNew;

                case Image img:
                    img.Save(stream, Format.ToImageFormat());
                    stream.Position = 0;
                    return Image.FromStream(stream);

                case SoundPlayer sp:
                    sp.Stream.CopyTo(stream);
                    stream.Position = 0;
                    return new SoundPlayer(stream);

                default: throw new UnreachableException($"{nameof(Object)} should have been set correctly by the {nameof(TrySetObject)} method.");
            }
        }

        internal override bool TryDeserializeBytes(byte[] bytes)
        {
            if (bytes.Length == 0)
            {
                Object = null;
                return true;
            }

            if (Format == FileFormat.Txt)
            {
                Object = Encoding.Unicode.GetString(bytes);
                return true;
            }

            var stream = new MemoryStream();
            stream.Write(bytes, 0, bytes.Length);
            stream.Position = 0;

            switch (Format)
            {
                case FileFormat.Rtf:
                    RichTextBox rtb = new RichTextBox();
                    rtb.LoadFile(stream, RichTextBoxStreamType.RichText);
                    Object = rtb;
                    return true;

                case FileFormat.Bmp:
                case FileFormat.Emf:
                case FileFormat.Wmf:
                case FileFormat.Gif:
                case FileFormat.Jpeg:
                case FileFormat.Png:
                case FileFormat.Tiff:
                case FileFormat.Exif:
                case FileFormat.Icon:
                    Object = Image.FromStream(stream);
                    return true;

                case FileFormat.Wav:
                    Object = new SoundPlayer(stream);
                    return true;

                default: throw new UnreachableException($"{nameof(Format)} should have been set correctly by the constructor.");
            }
        }

        internal override int GetIndexInParent() => Parent.Directories.Count + Parent.Files.IndexOf_(this);

        private protected override byte[] GetSerializedBytes()
        {
            if (Object is null)
                return Array.Empty<byte>();

            if (Object is string str)
                return Encoding.Unicode.GetBytes(str);

            using (var stream = new MemoryStream())
            {
                switch (Object)
                {
                    case RichTextBox rtb:
                        rtb.SaveFile(stream, RichTextBoxStreamType.RichText);
                        break;

                    case Image img:
                        img.Save(stream, Format.ToImageFormat());
                        break;

                    case SoundPlayer sp:
                        sp.Stream.CopyTo(stream);
                        break;

                    default: throw new UnreachableException($"{nameof(Object)} should have been set correctly by the {nameof(TrySetObject)} method.");
                }

                return stream.ToArray();
            }
        }

        private protected override bool TryRemoveFromParent() => Parent.TryRemoveFile(Name) == FSResult.Success;
    }
}
