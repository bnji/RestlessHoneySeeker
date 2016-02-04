using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Forms;

namespace PluginManager
{
    public interface IPluginHandler
    {
        void Kill(string file);

        byte[] Execute(string file, string parameters);

        void Reload(IPluginHost host, string pluginsDirectory);
    }
}
