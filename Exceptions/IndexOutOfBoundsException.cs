using System;
using System.Runtime.Serialization;

namespace ExceptionsNS
{
    [Serializable]
    public class IndexOutOfBoundsException : ArgumentException
    {
        private const string EXCEPTION = "Index was outside of bounds.";

        public IndexOutOfBoundsException() : base(EXCEPTION) { }
        public IndexOutOfBoundsException(string paramName) : base(EXCEPTION, paramName) { }
        public IndexOutOfBoundsException(string message, Exception innerException) : base(message, innerException) { }
        public IndexOutOfBoundsException(string message, string paramName) : base(message, paramName) { }
        protected IndexOutOfBoundsException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}
