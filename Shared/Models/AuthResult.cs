using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace Models
{
    [DataContract]
    public class AuthResult
    {
        [DataMember]
        public string Token { get; set; }

        [DataMember]
        public string IpInternal { get; set; }

        [DataMember]
        public string IpExternal { get; set; }

        [DataMember]
        public string HostName { get; set; }

        [DataMember]
        public bool IsAuthenticated
        {
            get
            {
                // Check if the token value has the length of a standard SHA1 value (which is 40)
                if (string.IsNullOrEmpty(Token)) return false;
                return Token.Length == 40;
            }
        }

    }
}
