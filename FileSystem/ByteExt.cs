using System;
using ExceptionsNS;
using static Core.Constants;

namespace FileSystemNS
{
    public static class ByteExt
    {
        private const int USHORT_LAST_BYTE = USHORT_BYTES - 1;
        private const int UINT_LAST_BYTE = UINT_BYTES - 1;
        private const int ULONG_LAST_BYTE = ULONG_BYTES - 1;

        public static ushort ToUShort(this byte[] bytes, int index)
        {
            if (bytes is null) throw new ArgumentNullException(nameof(bytes), Exceptions.CANNOT_BE_NULL);

            ushort value = 0;
            value |= bytes[index];
            for (int i = 1; i < USHORT_BYTES; i++)
            {
                value <<= BYTE_BITS;
                value |= bytes[index + i];
            }
            return value;
        }

        public static uint ToUInt(this byte[] bytes, int index)
        {
            if (bytes is null) throw new ArgumentNullException(nameof(bytes), Exceptions.CANNOT_BE_NULL);

            uint value = 0;
            value |= bytes[index];
            for (int i = 1; i < UINT_BYTES; i++)
            {
                value <<= BYTE_BITS;
                value |= bytes[index + i];
            }
            return value;
        }

        public static ulong ToULong(this byte[] bytes, int index)
        {
            if (bytes is null) throw new ArgumentNullException(nameof(bytes), Exceptions.CANNOT_BE_NULL);

            ulong value = 0;
            value |= bytes[index];
            for (int i = 1; i < ULONG_BYTES; i++)
            {
                value <<= BYTE_BITS;
                value |= bytes[index + i];
            }
            return value;
        }

        public static byte GetByte(this short value, int index) => ((ushort)value).GetByte(index);
        public static byte GetByte(this ushort value, int index) => (uint)index < USHORT_BYTES
                ? (byte)(value >> ((USHORT_LAST_BYTE - index) * BYTE_BITS))
                : throw new ArgumentOutOfRangeException(nameof(value), Exceptions.INDEX_OUTSIDE);

        public static byte GetByte(this int value, int index) => ((uint)value).GetByte(index);
        public static byte GetByte(this uint value, int index) => (uint)index < UINT_BYTES
                ? (byte)(value >> ((UINT_LAST_BYTE - index) * UINT_BYTES))
                : throw new ArgumentOutOfRangeException(nameof(value), Exceptions.INDEX_OUTSIDE);

        public static byte GetByte(this long value, int index) => ((ulong)value).GetByte(index);
        public static byte GetByte(this ulong value, int index) => (uint)index < ULONG_BYTES
                ? (byte)(value >> ((ULONG_LAST_BYTE - index) * ULONG_BYTES))
                : throw new ArgumentOutOfRangeException(nameof(value), Exceptions.INDEX_OUTSIDE);
    }
}
