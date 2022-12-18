using System.Collections.Generic;
using CustomCollections;
using static Core.Constants;

namespace FileSystemNS
{
    public static class Constants
    {
        internal const int ADDRESS_BYTES = LONG_BYTES;
        public const string NAME_FORBIDDEN_CHARS = @"\/";
        public const string CUR_DIR = ".";
        public const string PAR_DIR = "..";
        public static IReadOnlyList<string> ReservedNames { get; } = new string[] { CUR_DIR, PAR_DIR }.ToReadOnly();

        internal const int TOTAL_SIZE_INDEX = 1;
        internal const int SECTOR_SIZE_INDEX = TOTAL_SIZE_INDEX + LONG_BYTES;
        internal const int SECTOR_COUNT_INDEX = SECTOR_SIZE_INDEX + USHORT_BYTES;
        internal const int BOOT_SECTOR_SIZE = SECTOR_COUNT_INDEX + ULONG_BYTES;
        internal const int BITMAP_INDEX = BOOT_SECTOR_SIZE;

        internal const int NAME_MAX_LENGTH = 31;
        internal const int NAME_BYTES = NAME_MAX_LENGTH * UNICODE_BYTES;
        internal const int OBJECT_FLAGS_INDEX = 0;
        internal const int NAME_INDEX = 2;
        internal const int BYTE_COUNT_INDEX = NAME_INDEX + NAME_BYTES;
        internal const int INFO_BYTES_INDEX = BYTE_COUNT_INDEX + LONG_BYTES;
    }
}
