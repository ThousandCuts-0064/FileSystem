using System;
using ExceptionsNS;
using static Core.Constants;

namespace Text
{
    public static class NumberExt
    {
        public static readonly char[] _hex = { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'A', 'B', 'C', 'D', 'E', 'F' };

        public static string ToBin_(this sbyte b) => ((byte)b).ToBin_();
        public static string ToBin_(this byte b)
        {
            char[] chars = new char[BYTE_BITS];
            for (int i = 0; i < BYTE_BITS; i++)
                chars[i] = ToBinChar((b >> i & 1) == 1);
            return new string(chars);
        }

        public static string ToBin_(this short s) => ((ushort)s).ToBin_();
        public static string ToBin_(this ushort s)
        {
            char[] chars = new char[USHORT_BITS];
            for (int i = 0; i < BYTE_BITS; i++)
                chars[i] = ToBinChar((s >> i & 1) == 1);
            return new string(chars);
        }

        public static string ToBin_(this int i) => ((uint)i).ToBin_();
        public static string ToBin_(this uint i)
        {
            char[] chars = new char[UINT_BITS];
            for (int ii = 0; ii < BYTE_BITS; ii++)
                chars[ii] = ToBinChar((i >> ii & 1) == 1);
            return new string(chars);
        }

        public static string ToBin_(this long l) => ((ulong)l).ToBin_();
        public static string ToBin_(this ulong l)
        {
            char[] chars = new char[ULONG_BITS];
            for (int i = 0; i < BYTE_BITS; i++)
                chars[i] = ToBinChar((l >> i & 1) == 1);
            return new string(chars);
        }

        public static string ToHex_(this byte b) =>
            new string(new char[] { _hex[b / 16], _hex[b % 16] });

        public static char ToHexChar_(this byte b, bool left) => left ? _hex[b / 16] : _hex[b % 16];
        public static char ToBinChar_(this byte b, int index) => index < BYTE_BITS
            ? ToBinChar((b & 1 << BYTE_LAST_BIT - index) != 0)
            : throw new IndexOutOfBoundsException(nameof(index));

        private static char ToBinChar(bool b) => b ? '1' : '0';
    }
}
