using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileSystemNS
{
    public class Polynomial
    {
        private byte[] _coefs;
        public byte[] Coefficients { get => _coefs; set => _coefs = value; }
        public int Length => _coefs.Length;

        public byte this[int index] { get => _coefs[index]; set => _coefs[index] = value; }

        public Polynomial(byte[] coefficients) => _coefs = coefficients;

        public static Polynomial CreateGeneratorPolynomial(int degree)
        {
            // Create a polynomial with the specified degree
            byte[] coefficients = new byte[degree + 1];
            coefficients[0] = 1;
            return new Polynomial(coefficients);
        }

        public Polynomial Modulo(Polynomial divisor)
        {

            // Perform polynomial division and return the remainder
            Polynomial quotient = new Polynomial(new byte[Length]);
            Polynomial remainder = new Polynomial(_coefs);

            for (int i = Length - divisor.Length; i >= 0; i--)
            {
                quotient[i] = remainder[remainder.Length - 1];
                for (int j = 0; j < divisor.Length; j++)
                    remainder[i + j] = (byte)(remainder[i + j] ^ (divisor[j] * quotient[i]));
            }

            Array.Resize(ref remainder._coefs, divisor.Length - 1);
            return remainder;
        }
    }
}
