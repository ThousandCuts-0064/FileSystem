using System;
using System.Runtime.Serialization;

namespace ExceptionsNS
{
    public class NumberNotPositiveException : ArgumentException
    {
        private const string EXCEPTION = "Number must be possitive.";

        public NumberNotPositiveException() : base(EXCEPTION) { }
        public NumberNotPositiveException(string paramName) : base(EXCEPTION, paramName) { }
        public NumberNotPositiveException(string message, Exception innerException) : base(message, innerException) { }
        public NumberNotPositiveException(string message, string paramName) : base(message, paramName) { }
        protected NumberNotPositiveException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}
