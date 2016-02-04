using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Library
{
    public class HandlerInitData
    {
        public Form HostForm { get; set; }
        public Uri Url { get; set; }
        public string APIKEY_PRIVATE { get; set; }
        public string APIKEY_PUBLIC { get; set; }
        public int CONNECTION_TIMEOUT { get; set; }
        public int CONNECTION_INTERVAL { get; set; }
        public bool StartNewProcessOnExit { get; set; }

        public bool HideOnStart { get; set; }
    }
}
