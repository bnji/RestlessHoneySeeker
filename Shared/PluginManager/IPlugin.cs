using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Forms;

namespace PluginManager
{
    public interface IPlugin
    {
        IPluginClient Client { get; set; }
        IPluginHost Host { get; set; }
        PluginInfo Info { get; set; }
    }
}
