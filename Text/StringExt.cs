using System;
using ExceptionsNS;
using CustomQuery;

namespace Text
{
    public static class StringExt
    {
        public static bool IsNullOrEmpty_(this string str) => str is null || str == "";

        public static string ToUpperASCII_(this string str)
        {
            if (str is null) throw new ArgumentNullException(nameof(str));

            char[] chars = new char[str.Length];
            for (int i = 0; i < str.Length; i++)
                chars[i] = str[i].ToUpperASCII_();
            return new string(chars);
        }

        public static string ToLowerASCII_(this string str)
        {
            if (str is null) throw new ArgumentNullException(nameof(str));

            char[] chars = new char[str.Length];
            for (int i = 0; i < str.Length; i++)
                chars[i] = str[i].ToLowerASCII_();
            return new string(chars);
        }

        public static int LastIndexOf_(this string str, char c)
        {
            for (int i = str.Length - 1; i > 0; i--)
                if (str[i] == c)
                    return i;
            return -1;
        }

        public static string TrimStart_(this string str, char trim)
        {
            if (str is null) throw new ArgumentNullException(nameof(str));

            if (str == "") return str;

            int i = 0;
            while (i < str.Length && str[i] == trim) i++;
            return i == str.Length ? "" : str.SubstringAt_(i, str.Length - 1);
        }

        public static string TrimEnd_(this string str, char trim)
        {
            if (str is null) throw new ArgumentNullException(nameof(str));

            if (str == "") return str;

            int i = str.Length - 1;
            while (i >= 0 && str[i] == trim) i--;
            return i == -1 ? "" : str.SubstringAt_(0, i);
        }

        public static string PadLeft_(this string str, int widthTotal) => str.PadLeft_(widthTotal, ' ');
        public static string PadLeft_(this string str, int widthTotal, char pad)
        {
            if (str is null) throw new ArgumentNullException(nameof(str));
            if (widthTotal <= 0) throw new NumberNotPositiveException(nameof(widthTotal));

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
            if (str is null) throw new ArgumentNullException(nameof(str));
            if (widthTotal <= 0) throw new NumberNotPositiveException(nameof(widthTotal));

            if (widthTotal <= str.Length) return str;

            char[] chars = new char[widthTotal];
            int i = 0;
            while (i < str.Length) chars[i] = str[i++];
            while (i < widthTotal) chars[i++] = pad;
            return new string(chars);
        }

        public static string Substring_(this string str, int index) => str.Substring_(index, str.Length - index);
        public static string Substring_(this string str, int index, int length)
        {
            if (str is null) throw new ArgumentNullException(nameof(str));
            if ((uint)index > (uint)str.Length) throw new IndexOutOfBoundsException(nameof(index));
            if (length <= 0) throw new ArgumentOutOfRangeException(nameof(length));
            if (index + length > str.Length) throw new IndexOutOfBoundsException(nameof(index));

            if (index == 0 && length == str.Length) return str;

            char[] chars = new char[length];
            for (int i = 0; i < length; i++)
                chars[i] = str[index + i];
            return new string(chars);
        }

        public static string SubstringAt_(this string str, int index1, int index2)
        {
            if (str is null) throw new ArgumentNullException(nameof(str));
            if ((uint)index1 > (uint)str.Length) throw new IndexOutOfBoundsException(nameof(index1));
            if ((uint)index2 > (uint)str.Length) throw new IndexOutOfBoundsException(nameof(index2));

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
            if (str is null) throw new ArgumentNullException(nameof(str));
            if (str == "" || maxResults == 0) return Array.Empty<string>();
            if (maxResults < 0) throw new NumberNegativeException(nameof(maxResults));

            if (maxResults == 1) return new string[] { str };
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

        public static string Join_(this string[] strings, string value)
        {
            if (strings is null) throw new ArgumentNullException(nameof(strings));
            if (value is null) throw new ArgumentNullException(nameof(value));
            if (strings.Length == 0) throw new CollectionEmptyException(nameof(strings));
            if (value.Length == 0) throw new CollectionEmptyException(nameof(value));

            if (strings.Length == 1) return strings[0];

            int totalChars = -value.Length; // 1 extra will be added
            for (int i = 0; i < strings.Length; i++)
                totalChars += strings[i].Length + value.Length;
            char[] chars = new char[totalChars];
            int chI = 0;
            for (int i = 0; i < strings[0].Length; i++)
                chars[chI++] = strings[0][i];
            for (int i = 1; i < strings.Length; i++)
            {
                for (int y = 0; y < value.Length; y++)
                    chars[chI++] = value[y];
                for (int y = 0; y < strings[i].Length; y++)
                    chars[chI++] = strings[i][y];
            }
            return new string(chars);
        }
    }
}
