namespace Core
{
    public static class Math_
    {
        public static int DivCeiling(ushort a, ushort b) => a / b + a % b == 0 ? 0 : 1;
        public static int DivCeiling(int a, int b) => a / b + a % b == 0 ? 0 : 1;
        public static int DivCeiling(long a, long b) => a / b + a % b == 0 ? 0 : 1;
    }
}
