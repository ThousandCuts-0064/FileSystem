namespace Core
{
    public static class Constants
    {
        public const int BYTE_BITS    = 8;
        public const int USHORT_BITS  = 16;
        public const int UINT_BITS    = 32;
        public const int ULONG_BITS   = 64;
                                      
        public const int SBYTE_BITS   = BYTE_BITS;
        public const int SHORT_BITS   = USHORT_BITS;
        public const int INT_BITS     = UINT_BITS;
        public const int LONG_BITS    = ULONG_BITS;

        public const int USHORT_BYTES = USHORT_BITS / BYTE_BITS;
        public const int UINT_BYTES   = UINT_BITS   / BYTE_BITS;
        public const int ULONG_BYTES  = ULONG_BITS  / BYTE_BITS;

        public const int SHORT_BYTES  = USHORT_BYTES;
        public const int INT_BYTES    = UINT_BYTES;
        public const int LONG_BYTES   = ULONG_BYTES;

        public const int UNICODE_SYMBOL_BYTES = 2;
    }
}
