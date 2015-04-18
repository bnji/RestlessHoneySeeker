using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Models
{
    public class ComputerData
    {
        public string Name { get; set; }
        public string IpExternal { get; set; }
        public string IpInternal { get; set; }
        public DateTime? LastActive { get; set; }
        public string FileUploaded { get; set; }
        public int BytesUploaded { get; set; }
        public string Hash { get; set; }
    }
}
