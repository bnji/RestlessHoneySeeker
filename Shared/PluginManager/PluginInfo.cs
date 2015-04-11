using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace PluginManager
{
    public class PluginInfo : IPluginInfo
    {
        public FileInfo File { get; set; }
        public String AssemblyName { get; set; }
        public String Version { get; set; }
        public String Description { get; set; }

        public PluginInfo() { }

        public PluginInfo(String _assemblyName, String _version, String _description)
        {
            AssemblyName = _assemblyName;
            Version = _version;
            Description = _description;
        }
    }
}
