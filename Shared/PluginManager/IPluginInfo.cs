using System;
using System.IO;
namespace PluginManager
{
    interface IPluginInfo
    {
        string AssemblyName { get; set; }
        string Description { get; set; }
        FileInfo File { get; set; }
        string Version { get; set; }
    }
}
