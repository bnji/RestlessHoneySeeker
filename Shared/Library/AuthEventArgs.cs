using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Library
{
    public class AuthEventArgs : EventArgs
    {
        public bool IsAuthenticated { get; set; }
        public AuthEventArgs()
        {

        }
    }
}
