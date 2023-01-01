using System;
using System.Runtime.Serialization;

namespace ExceptionsNS
{
    [Serializable]
    public class CollectionEmptyException : ArgumentException
    {
        private const string EXCEPTION = "Collection cannot be empty.";

        public CollectionEmptyException() : base(EXCEPTION) { }
        public CollectionEmptyException(string paramName) : base(EXCEPTION, paramName) { }
        public CollectionEmptyException(string message, Exception innerException) : base(message, innerException) { }
        public CollectionEmptyException(string message, string paramName) : base(message, paramName) { }
        protected CollectionEmptyException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}
