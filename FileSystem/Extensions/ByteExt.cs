using System;
using ExceptionsNS;
using Text;
using static Core.Constants;
using static FileSystemNS.Constants;

namespace FileSystemNS
{
    public static class ByteExt
    {
        public static short GetShort(this byte[] bytes, int index) => (short)GetUShort(bytes, index);
        public static ushort GetUShort(this byte[] bytes, int index)
        {
            if (bytes is null) throw new ArgumentNullException(nameof(bytes));
            if (bytes.Length < USHORT_BYTES) throw new ArrayTooShortExcpetion(nameof(bytes));
            if ((uint)index > (uint)(bytes.Length - USHORT_BYTES)) throw new IndexOutOfBoundsException(nameof(index));

            ushort value = 0;
            value |= bytes[index];
            for (int i = 1; i < USHORT_BYTES; i++)
            {
                value <<= BYTE_BITS;
                value |= bytes[index + i];
            }
            return value;
        }

        public static int GetInt(this byte[] bytes, int index) => (int)GetUInt(bytes, index);
        public static uint GetUInt(this byte[] bytes, int index)
        {
            if (bytes is null) throw new ArgumentNullException(nameof(bytes));
            if (bytes.Length < UINT_BYTES) throw new ArrayTooShortExcpetion(nameof(bytes));
            if ((uint)index > (uint)(bytes.Length - UINT_BYTES)) throw new IndexOutOfBoundsException(nameof(index));

            uint value = 0;
            value |= bytes[index];
            for (int i = 1; i < UINT_BYTES; i++)
            {
                value <<= BYTE_BITS;
                value |= bytes[index + i];
            }
            return value;
        }

        public static long GetLong(this byte[] bytes, int index) => (long)GetULong(bytes, index);
        public static ulong GetULong(this byte[] bytes, int index)
        {
            if (bytes is null) throw new ArgumentNullException(nameof(bytes));
            if (bytes.Length < ULONG_BYTES) throw new ArrayTooShortExcpetion(nameof(bytes));
            if ((uint)index > (uint)(bytes.Length - ULONG_BYTES)) throw new IndexOutOfBoundsException(nameof(index));

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
                : throw new IndexOutOfBoundsException(nameof(index));

        public static byte GetByte(this int value, int index) => ((uint)value).GetByte(index);
        public static byte GetByte(this uint value, int index) => (uint)index < UINT_BYTES
                ? (byte)(value >> ((UINT_LAST_BYTE - index) * UINT_BYTES))
                : throw new IndexOutOfBoundsException(nameof(index));

        public static byte GetByte(this long value, int index) => ((ulong)value).GetByte(index);
        public static byte GetByte(this ulong value, int index) => (uint)index < ULONG_BYTES
                ? (byte)(value >> ((ULONG_LAST_BYTE - index) * ULONG_BYTES))
                : throw new IndexOutOfBoundsException(nameof(index));

        public static void GetBytes(this short value, byte[] bytes, int index) => GetBytes((ushort)value, bytes, index);
        public static void GetBytes(this ushort value, byte[] bytes, int index)
        {
            if (bytes is null) throw new ArgumentNullException(nameof(bytes));
            if (bytes.Length < USHORT_BYTES) throw new ArrayTooShortExcpetion(nameof(bytes));
            if ((uint)index > (uint)(bytes.Length - USHORT_BYTES)) throw new IndexOutOfBoundsException(nameof(index));

            for (int i = 0; i < USHORT_BYTES; i++)
                bytes[index + i] = value.GetByte(i);
        }

        public static void GetBytes(this int value, byte[] bytes, int index) => GetBytes((uint)value, bytes, index);
        public static void GetBytes(this uint value, byte[] bytes, int index)
        {
            if (bytes is null) throw new ArgumentNullException(nameof(bytes));
            if (bytes.Length < UINT_BYTES) throw new ArrayTooShortExcpetion(nameof(bytes));
            if ((uint)index > (uint)(bytes.Length - UINT_BYTES)) throw new IndexOutOfBoundsException(nameof(index));

            for (int i = 0; i < UINT_BYTES; i++)
                bytes[index + i] = value.GetByte(i);
        }

        public static void GetBytes(this long value, byte[] bytes, int index) => GetBytes((ulong)value, bytes, index);
        public static void GetBytes(this ulong value, byte[] bytes, int index)
        {
            if (bytes is null) throw new ArgumentNullException(nameof(bytes));
            if (bytes.Length < USHORT_BYTES) throw new ArrayTooShortExcpetion(nameof(bytes));
            if ((uint)index > (uint)(bytes.Length - ULONG_BYTES)) throw new IndexOutOfBoundsException(nameof(index));

            for (int i = 0; i < ULONG_BYTES; i++)
                bytes[index + i] = value.GetByte(i);
        }

        public static string ToHex_(this byte[] bytes)
        {
            char[] chars = new char[bytes.Length * 3 - 1];

            chars[0] = bytes[0].ToHexChar_(true);
            chars[1] = bytes[0].ToHexChar_(false);
            for (int i = 1; i < bytes.Length; i++)
            {
                chars[i * 3 - 1] = ' ';
                chars[i * 3 + 0] = bytes[i].ToHexChar_(true);
                chars[i * 3 + 1] = bytes[i].ToHexChar_(false);
            }

            return new string(chars);
        }

        public static string ToBin_(this byte[] bytes)
        {
            int binWithSpace = BYTE_BITS + 1;
            char[] chars = new char[bytes.Length * binWithSpace - 1];

            for (int i = 0; i < BYTE_BITS; i++)
                chars[i] = bytes[0].ToBinChar_(i);
            for (int i = 1; i < bytes.Length; i++)
            {
                chars[i * binWithSpace - 1] = ' ';
                for (int y = 0; y < BYTE_BITS; y++)
                    chars[i * binWithSpace + y] = bytes[i].ToBinChar_(y);
            }

            return new string(chars);
        }
    }
}
