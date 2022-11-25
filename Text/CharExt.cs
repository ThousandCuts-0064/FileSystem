using static Text.Constants;

namespace Text
{
    public static class CharExt
    {
        public static char ToUpperASCII_(this char c) =>
            c < 'a' || c > 'z'
                ? c
                : (char)(c + CHAR_TO_UPPER);

        public static char ToLowerASCII_(this char c) =>
            c < 'A' || c > 'Z'
                ? c
                : (char)(c + CHAR_TO_LOWER);
    }
}
