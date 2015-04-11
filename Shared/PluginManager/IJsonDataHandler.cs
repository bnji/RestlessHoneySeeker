using System;
using System.Collections.Generic;
namespace PluginManager
{
    interface IJsonDataHandler
    {
        String ConfigFile { get; set; }
        String JsonData { get; set; }
        List<IPlugin> Load();
        Boolean Save(List<IPlugin> plugins);
    }
}
