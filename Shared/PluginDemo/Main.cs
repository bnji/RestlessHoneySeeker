using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using PluginManager;

namespace PluginDemo
{
    class Main : IPlugin
    {
        public IPluginClient Client { get; set; }
        public IPluginHost Host { get; set; }
        public PluginInfo Info { get; set; }

        public Main()
        {
            Client = new Client();
        }
    }
}
