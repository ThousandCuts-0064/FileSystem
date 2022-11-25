using System;
using ExceptionsNS;
using CustomQuery;
using static Text.Constants;

namespace Text
{
    public static class StringExt
    {
        public static string ToUpperASCII_(this string str)
        {
            if (str is null) throw new ArgumentNullException(nameof(str), Exceptions.CANNOT_BE_NULL);

            char[] chars = new char[str.Length];
            for (int i = 0; i < str.Length; i++)
                chars[i] = str[i].ToUpperASCII_();
            return new string(chars);
        }

        public static string ToLowerASCII_(this string str)
        {
            if (str is null) throw new ArgumentNullException(nameof(str), Exceptions.CANNOT_BE_NULL);

            char[] chars = new char[str.Length];
            for (int i = 0; i < str.Length; i++)
                chars[i] = str[i].ToLowerASCII_();
            return new string(chars);
        }

        public static string TrimStart_(this string str, char trim)
        {
            if (str is null) throw new ArgumentNullException(nameof(str), Exceptions.CANNOT_BE_NULL);

            if (str is "") return str;

            int i = 0;
            while (str[i] == trim && i < str.Length) i++;
            return str.SubstringAt_(i, str.Length - 1);
        }

        public static string TrimEnd_(this string str, char trim)
        {
            if (str is null) throw new ArgumentNullException(nameof(str), Exceptions.CANNOT_BE_NULL);

            if (str is "") return str;

            int i = str.Length - 1;
            while (str[i] == trim && i >= 0) i--;
            return str.SubstringAt_(0, i);
        }

        public static string PadLeft_(this string str, int widthTotal) => str.PadLeft_(widthTotal, ' ');
        public static string PadLeft_(this string str, int widthTotal, char pad)
        {
            if (str is null) throw new ArgumentNullException(nameof(str), Exceptions.CANNOT_BE_NULL);
            if (widthTotal < 0) throw new ArgumentOutOfRangeException(nameof(widthTotal), Exceptions.CANNOT_BE_NEGATIVE);

            char[] chars = new char[widthTotal];
            int totalPads = widthTotal - str.Length;
            int i = str.Length;
            while (i > totalPads) chars[--i] = str[i];
            while (i > 0) chars[--i] = pad;
            return new string(chars);
        }

        public static string PadRight_(this string str, int widthTotal) => str.PadRight_(widthTotal, ' ');
        public static string PadRight_(this string str, int widthTotal, char pad)
        {
            if (str is null) throw new ArgumentNullException(nameof(str), Exceptions.CANNOT_BE_NULL);
            if (widthTotal < 0) throw new ArgumentOutOfRangeException(nameof(widthTotal), Exceptions.CANNOT_BE_NEGATIVE);

            if (widthTotal <= str.Length) return str;

            char[] chars = new char[widthTotal];
            int i = 0;
            while (i < str.Length) chars[i] = str[i++];
            while (i < widthTotal) chars[i++] = pad;
            return new string(chars);
        }

        public static string Substring_(this string str, int index, int length)
        {
            if (str is null) throw new ArgumentNullException(nameof(str), Exceptions.CANNOT_BE_NULL);
            if ((uint)index > (uint)str.Length) throw new ArgumentOutOfRangeException(nameof(index), Exceptions.INDEX_OUTSIDE);
            if (length < 0) throw new ArgumentOutOfRangeException(nameof(length), Exceptions.CANNOT_BE_NEGATIVE);
            if (index + length > str.Length) throw new ArgumentOutOfRangeException(nameof(length), Exceptions.INDEX_OUTSIDE);

            if (length == 0) return "";
            if (index == 0 && length == str.Length) return str;

            char[] chars = new char[length];
            for (int i = 0; i < length; i++)
                chars[i] = str[index + i];
            return new string(chars);
        }

        public static string SubstringAt_(this string str, int index1, int index2)
        {
            if (str is null) throw new ArgumentNullException(nameof(str), Exceptions.CANNOT_BE_NULL);
            if ((uint)index1 > (uint)str.Length) throw new ArgumentOutOfRangeException(nameof(index1), Exceptions.INDEX_OUTSIDE);
            if ((uint)index2 > (uint)str.Length) throw new ArgumentOutOfRangeException(nameof(index2), Exceptions.INDEX_OUTSIDE);

            if (index2 < index1) (index1, index2) = (index2, index1); //Swap

            if (index1 == index2) return str[index1].ToString();
            if (index1 == 0 && index2 == str.Length) return str;

            char[] chars = new char[index2 - index1 + 1];
            for (int i = 0; i < chars.Length; i++)
                chars[i] = str[index1++];
            return new string(chars);
        }

        public static string[] Split_(this string str, char separator) => str.Split_(new char[] { separator }, str.Length - 1);
        public static string[] Split_(this string str, char separator, int maxResults) => str.Split_(new char[] { separator }, maxResults);
        public static string[] Split_(this string str, char[] separators) => str.Split_(separators, str.Length - 1);
        public static string[] Split_(this string str, char[] separators, int maxResults)
        {
            if (str is null) throw new ArgumentNullException(nameof(str), Exceptions.CANNOT_BE_NULL);
            if (maxResults < 0) throw new ArgumentOutOfRangeException(nameof(maxResults), Exceptions.CANNOT_BE_NEGATIVE);

            if (str == "") return Array.Empty<string>();
            if (maxResults == 0) return new string[] { str };
            if (str.Length == 1) return separators.Contains_(str[0]) ? Array.Empty<string>() : new string[] { str };

            if (maxResults > str.Length - 1) maxResults = str.Length - 1;
            int[] ranges = new int[maxResults + 2];
            int stringCount = 0;
            int splitCount = 0;

            bool selector = false; // Selects begining or end of output string
            for (int i = 0; stringCount < maxResults && i < str.Length; i++)
            {
                if (separators.Contains_(str[i]) != selector) continue;

                if (selector) ranges[splitCount++] = i; // End of new string
                else
                {
                    ranges[splitCount++] = i; // Beginning of new string
                    stringCount++;
                }
                selector ^= true; // Change selector
            }

            if (stringCount == 0) return Array.Empty<string>();

            string[] strings = new string[stringCount];

            for (int i = 0; i < splitCount - 1; i++)
                strings[i / 2] = str.SubstringAt_(ranges[i], ranges[++i] - 1);

            if (!(strings[stringCount - 1] is null)) return strings;

            strings[stringCount - 1] = str.SubstringAt_(ranges[splitCount - 1], str.Length - 1);
            return strings;
        }
    }
}
