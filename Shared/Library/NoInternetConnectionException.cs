using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Library
{
    public class NoInternetConnectionException : Exception
    {
        public NoInternetConnectionException() : base() { }
    }
}
