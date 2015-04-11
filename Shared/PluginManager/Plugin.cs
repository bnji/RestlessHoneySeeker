using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Forms;

namespace PluginManager
{
    public class Plugin : IPlugin
    {
        public IPluginClient Client { get; set; }
        public IPluginHost Host { get; set; }
        public PluginInfo Info { get; set; }
        
        public Plugin()
        {

        }

        public Plugin(IPluginHost _host, PluginInfo _info)
        {
            Host = _host;
            Info = _info;
        }

    }
}
