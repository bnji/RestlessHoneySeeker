using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Models
{
    public class AuthData
    {
        public string HostName { get; set; }
        public string Data { get; set; }
        public string PublicKey { get; set; }
        public string Hash { get; set; }

        public string IPInternal { get; set; }

        public string IpExternal { get; set; }
    }
}
