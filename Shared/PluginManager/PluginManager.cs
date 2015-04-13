using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace PluginManager
{
    public class PluginManager
    {
        public IPluginHost Host { get; internal set; }
        public List<IPlugin> Plugins { get; private set; }
        public String ConfigFiletype { get; set; }
        public String PluginAppPath { get; private set; }

        public IPlugin GetPlugin(string assemblyName)
        {
            if (Plugins != null)
            {
                return Plugins.Where(x => x.Info.AssemblyName == assemblyName).FirstOrDefault();
            }
            return null;
        }

        public bool RemovePlugin(IPlugin plugin)
        {
            var result = false;
            if (plugin != null)
            {
                try
                {
                    plugin.Client.Kill();
                    result = Plugins.Remove(plugin);
                }
                catch
                {
                    //damn
                }
            }
            return result;
        }

        public PluginManager(IPluginHost _host, String _pluginAppPath, String _configFiletype = "*.json")
        {
            Host = _host;
            PluginAppPath = _pluginAppPath;
            ConfigFiletype = _configFiletype;
            Plugins = LoadPlugins();
        }

        public List<IPlugin> LoadPlugins()
        {
            var plugins = new List<IPlugin>();
            foreach (PluginInfo info in GetValidPlugins())
            {
                try
                {
                    var objAsm = Assembly.LoadFile(info.File.FullName);
                    var types = objAsm.GetTypes();
                    foreach (Type t in types)
                    {
                        if (!typeof(IPlugin).IsAssignableFrom(t))
                            continue;
                        //http://stackoverflow.com/questions/19656/how-to-find-an-implementation-of-a-c-sharp-interface-in-the-current-assembly-wit
                        //MessageBox.Show(t.FullName + " implements " + typeof(IPlugin).FullName);
                        {
                            IPlugin plugin = null;
                            try
                            {
                                plugin = (IPlugin)Activator.CreateInstance(objAsm.GetType(t.FullName));
                            }
                            catch (Exception ex)
                            {
                                //MessageBox.Show("" + ex);
                                //Console.Write(ex);
                                //return null;
                            }
                            if (plugin != null)
                            {
                                plugin.Host = this.Host;
                                plugin.Info = info;
                                Int32 index = plugins.FindIndex((IPlugin p) => p.Info.AssemblyName == plugin.Info.AssemblyName);
                                if (index == -1)
                                {
                                    plugins.Add(plugin);
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }
            return plugins;
        }

        public List<PluginInfo> GetValidPlugins()
        {
            List<PluginInfo> plugins = new List<PluginInfo>();
            foreach (var file in Directory.GetFiles(this.PluginAppPath, this.ConfigFiletype, SearchOption.AllDirectories))
            {
                if (!file.StartsWith(".") && !file.StartsWith("_"))
                {
                    PluginInfo pi = null;
                    using (FileStream fs = File.Open(file, FileMode.Open))
                    using (StreamReader sr = new StreamReader(fs))
                    using (JsonReader jr = new JsonTextReader(sr))
                    {
                        try
                        {
                            JsonSerializer js = new JsonSerializer();
                            pi = js.Deserialize<PluginInfo>(jr);
                        }
                        catch (JsonSerializationException ex)
                        {
                            //Not a valid plugin information file
                        }
                    }
                    if (pi != null)
                    {
                        String dllFile = Path.Combine(Path.GetDirectoryName(file), pi.AssemblyName);
                        pi.File = new FileInfo(dllFile);
                        if (pi.File.Exists)
                        {
                            plugins.Add(pi);
                        }
                    }
                }
            }
            return plugins;
        }
    }
}
