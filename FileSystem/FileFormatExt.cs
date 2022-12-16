using System;
using System.Collections.Generic;
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
    }
}
