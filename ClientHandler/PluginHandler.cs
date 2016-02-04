using Library;
using PluginManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace ClientHandler
{
    public class PluginHandler : IPluginHandler
    {        
        private PluginManager.PluginManager pluginManager;
        
        public PluginHandler(IPluginHost host, string pluginsDirectory)
        {
            Reload(host, pluginsDirectory);
        }

        public void Reload(IPluginHost host, string pluginsDirectory = null)
        {
            if (pluginManager == null)
            {
                pluginManager = new PluginManager.PluginManager(host, pluginsDirectory);
            }
            pluginManager.LoadPlugins();
            //Initialize the plugins
            foreach (var p in pluginManager.Plugins)
            {
                try
                {
                    p.Client.Initialize();
                }
                catch (Exception)
                {
                    // write better code ;) or throw in the trash (pun intended)
                }
            }
        }

        public void Kill(string file)
        {
            if (pluginManager != null)
            {
                var plugin = pluginManager.GetPlugin(file);
                if (plugin != null)
                {
                    pluginManager.RemovePlugin(plugin);
                }
            }
        }

        public byte[] Execute(string file, string parameters)
        {
            if (pluginManager != null)
            {
                var plugin = pluginManager.GetPlugin(file);
                if (plugin != null)
                {
                    object data = null;
                    try
                    {
                        data = plugin.Client.Execute(parameters);
                    }
                    catch (Exception ex)
                    {
                        data = ex.ToString();
                    }
                    if (data != null)
                    {
                        return Encoding.Default.GetBytes(Convert.ToString(data));
                    }
                }
            }
            return null;
        }
    }
}