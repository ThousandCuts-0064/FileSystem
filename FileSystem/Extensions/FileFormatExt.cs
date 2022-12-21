using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using Core;
using CustomCollections;
using CustomQuery;
using Text;

namespace FileSystemNS
{
    public static class FileFormatExt
    {
        public static IReadOnlyList<string> FormatsAsLower { get; } =
            Enum.GetNames(typeof(FileFormat)).Get(out string[] names)
            .Select_(str => str.ToLowerASCII_())
            .ToArrayFixed_(names.Length)
            .ToReadOnly();

        public static string ResolveAliases(string format)
        {
            switch (format)
            {
                case "jpg": return "jpeg";
                case "tif": return "tiff";
                case "ico": return "icon";
                default: return format;
            }
        }

        public static string ToLower(this FileFormat format) => FormatsAsLower[(int)format];

        public static ImageFormat ToImageFormat(this FileFormat format)
        {
            switch (format)
            {
                case FileFormat.Bmp: return ImageFormat.Bmp;
                case FileFormat.Emf: return ImageFormat.Emf;
                case FileFormat.Wmf: return ImageFormat.Wmf;
                case FileFormat.Gif: return ImageFormat.Gif;
                case FileFormat.Jpeg: return ImageFormat.Jpeg;
                case FileFormat.Png: return ImageFormat.Png;
                case FileFormat.Tiff: return ImageFormat.Tiff;
                case FileFormat.Exif: return ImageFormat.Exif;
                case FileFormat.Icon: return ImageFormat.Icon;

                default: throw new InvalidOperationException();
            }
        }
    }
}
