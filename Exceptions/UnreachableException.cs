using System;
using System.Runtime.Serialization;

namespace ExceptionsNS
{
    [Serializable]
    public class UnreachableException : Exception
    {
        private const string EXCEPTION = "This instruction was tought to be unreachable.";

        public UnreachableException() : base(EXCEPTION) { }
        public UnreachableException(string messege) : base(messege) { }
        public UnreachableException(string message, Exception innerException) : base(message, innerException) { }
        protected UnreachableException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}
