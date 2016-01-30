using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Library
{
    public class TooLowTimeoutValueException : Exception
    {
        public TooLowTimeoutValueException() : base() { }
    }
}
