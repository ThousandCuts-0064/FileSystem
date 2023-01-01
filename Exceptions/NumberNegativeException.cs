using System;
using System.Runtime.Serialization;

namespace ExceptionsNS
{
    [Serializable]
    public class NumberNegativeException : ArgumentException
    {
        private const string EXCEPTION = "Number must not be negative.";

        public NumberNegativeException() : base(EXCEPTION) { }
        public NumberNegativeException(string paramName) : base(EXCEPTION, paramName) { }
        public NumberNegativeException(string message, Exception innerException) : base(message, innerException) { }
        public NumberNegativeException(string message, string paramName) : base(message, paramName) { }
        protected NumberNegativeException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}
