namespace Core
{
    public static class Utilities
    {
        public static T Get<T>(this T itemIn, out T itemOut)
        {
            itemOut = itemIn;
            return itemIn;
        }
    }
}
