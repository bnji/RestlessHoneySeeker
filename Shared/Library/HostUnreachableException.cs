using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Library
{
    public class HostUnreachableException : Exception
    {
        public HostUnreachableException() : base() { }
        public HostUnreachableException(string message) : base(message) { }
        public HostUnreachableException(string message, Exception inner) : base(message, inner) { }

        // A constructor is needed for serialization when an
        // exception propagates from a remoting server to the client. 
        protected HostUnreachableException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) { }
    }
}
