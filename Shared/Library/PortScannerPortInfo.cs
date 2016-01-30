using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Library
{
    public class PortScannerPortInfo
    {
        public string Name { get; set; }
        public int Port { get; set; }
        public EPortType Type { get; set; }
        public string Description { get; set; }

        public override string ToString()
        {
            return "Port " + Port + (!string.IsNullOrEmpty(Name) ? (Name + " ") : "") + (!string.IsNullOrEmpty(Description) ? "(" + Description + ")" : "");
        }
    }

}
