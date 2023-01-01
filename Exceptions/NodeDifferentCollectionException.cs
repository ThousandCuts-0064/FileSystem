using System;
using System.Runtime.Serialization;

namespace ExceptionsNS
{
    [Serializable]
    public class NodeDifferentCollectionException : ArgumentException
    {
        private const string EXCEPTION = "The node was from different collection.";

        public NodeDifferentCollectionException() : base(EXCEPTION) { }
        public NodeDifferentCollectionException(string paramName) : base(EXCEPTION, paramName) { }
        public NodeDifferentCollectionException(string message, Exception innerException) : base(message, innerException) { }
        public NodeDifferentCollectionException(string message, string paramName) : base(message, paramName) { }
        protected NodeDifferentCollectionException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}
