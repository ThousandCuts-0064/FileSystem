using static Core.Constants;

namespace FileSystemNS
{
    public static class Constants
    {
        public const string FORBIDDEN_CHARS = @"\/";
        public const int NAME_BYTES = byte.MaxValue;
        public const int NAME_MAX_LENGTH = NAME_BYTES / UNICODE_SYMBOL_BYTES;

        public const int SECTOR_SIZE_INDEX = 1;
        public const int SECTOR_COUNT_INDEX = SECTOR_SIZE_INDEX + USHORT_BYTES;
        public const int TOTAL_SIZE_INDEX = SECTOR_COUNT_INDEX + ULONG_BYTES;
        public const int BOOT_SECTOR_SIZE = TOTAL_SIZE_INDEX + ULONG_BYTES;
        public const int BITMAP_INDEX = BOOT_SECTOR_SIZE;
    }
}
