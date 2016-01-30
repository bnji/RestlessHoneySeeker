using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Library
{
    public class CommandEventArgs : EventArgs
    {
        public ECommand Command { get; set; }

        public CommandEventArgs()
        {

        }
    }
}
