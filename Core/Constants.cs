namespace Core
{
    public static class Constants
    {
        public const int    BYTE_BITS  = 8;
        public const int  USHORT_BITS  = 16;
        public const int    UINT_BITS  = 32;
        public const int   ULONG_BITS  = 64;
                                       
        public const int   SBYTE_BITS  =   BYTE_BITS;
        public const int   SHORT_BITS  = USHORT_BITS;
        public const int     INT_BITS  =   UINT_BITS;
        public const int    LONG_BITS  =  ULONG_BITS;
                                       
        public const int  USHORT_BYTES = USHORT_BITS / BYTE_BITS;
        public const int    UINT_BYTES =   UINT_BITS / BYTE_BITS;
        public const int   ULONG_BYTES =  ULONG_BITS / BYTE_BITS;
                                      
        public const int   SHORT_BYTES = USHORT_BYTES;
        public const int     INT_BYTES =   UINT_BYTES;
        public const int    LONG_BYTES =  ULONG_BYTES;

        public const int UNICODE_BYTES = 2;

        public const int   BYTE_LAST_BIT  =   BYTE_BITS  - 1;

        public const int USHORT_LAST_BYTE = USHORT_BYTES - 1;
        public const int   UINT_LAST_BYTE =   UINT_BYTES - 1;
        public const int  ULONG_LAST_BYTE =  ULONG_BYTES - 1;
    }
}
