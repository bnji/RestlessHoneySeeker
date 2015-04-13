using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Forms;

namespace PluginManager
{
    public interface IPluginClient
    {
        void Initialize();

        object Execute(string parameters);

        void Kill();
    }
}
