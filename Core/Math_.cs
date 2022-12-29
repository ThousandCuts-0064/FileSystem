namespace Core
{
    public static class Math_
    {
        public static ushort DivCeiling(ushort a, ushort b) => (ushort)(a / b + (a % b == 0 ? 0 : 1));
        public static int DivCeiling(int a, int b) => a / b + (a % b == 0 ? 0 : 1);
        public static long DivCeiling(long a, long b) => a / b + (a % b == 0 ? 0 : 1);

        public static int NextPrime(int number)
        {
            bool isPrime;
            do
            {
                isPrime = true;
                number++;
                int squaredNumber = (int)System.Math.Sqrt(number);

                for (int i = 2; i <= squaredNumber; i++)
                    if (number % i == 0)
                    {
                        isPrime = false;
                        break;
                    }
            }
            while (!isPrime);

            return number;
        }
    }
}
