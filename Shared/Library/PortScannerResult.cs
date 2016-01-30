using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Library
{
    public class PortScannerResult
    {
        public bool IsOpen { get; set; }
        public int Port { get; set; }

        public override string ToString()
        {
            return "Port " + Port + " is " + (IsOpen ? "OPEN" : "CLOSED") + "!";
        }

        public PortScannerPortInfo PortInfo { get; set; }
    }
}
