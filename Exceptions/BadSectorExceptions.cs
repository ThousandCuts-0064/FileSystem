using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ExceptionsNS
{
    [Serializable]
    public class BadSectorException : Exception
    {
        private const string EXCEPTION = "Tried to access a bad sector.";

        public BadSectorException() : base(EXCEPTION) { }
        public BadSectorException(long address) : base($"Sector at {address} is bad, but was tried to be accessed.") { }
        public BadSectorException(string messege) : base(messege) { }
        public BadSectorException(string message, Exception innerException) : base(message, innerException) { }
        protected BadSectorException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}
