using System;
using System.Runtime.Serialization;

namespace ExceptionsNS
{
    [Serializable]
    public class ArrayTooShortExcpetion : ArgumentException
    {
        private const string EXCEPTION = "Destination array was too short.";

        public ArrayTooShortExcpetion() : base(EXCEPTION) { }
        public ArrayTooShortExcpetion(string paramName) : base(EXCEPTION, paramName) { }
        public ArrayTooShortExcpetion(string message, Exception innerException) : base(message, innerException) { }
        public ArrayTooShortExcpetion(string message, string paramName) : base(message, paramName) { }
        protected ArrayTooShortExcpetion(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}
