using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace PluginManager
{
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "10.0.0.0")]
    public class JsonDataHandler : IJsonDataHandler
    {
        public String ConfigFile { get; set; }
        public String JsonData { get; set; }

        public JsonDataHandler(String _file)
        {
            this.ConfigFile = _file;
        }

        public Boolean Save(List<IPlugin> plugins)
        {
            Boolean isSuccess = false;

            try
            {
                using (FileStream fs = File.Open(ConfigFile, FileMode.Create))
                using (StreamWriter sw = new StreamWriter(fs))
                using (JsonWriter jw = new JsonTextWriter(sw))
                {
                    jw.Formatting = Formatting.Indented;
                    JsonSerializer serializer = new JsonSerializer();
                    serializer.Serialize(jw, plugins);
                    isSuccess = true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                isSuccess = false;
            }
            return isSuccess;
        }

        public List<IPlugin> Load()
        {
            List<IPlugin> data;

            using (FileStream fs = File.Open(ConfigFile, FileMode.Open))
            using (StreamReader sr = new StreamReader(fs))
            using (JsonReader jr = new JsonTextReader(sr))
            {
                JsonSerializer serializer = new JsonSerializer();
                JsonData = sr.ReadToEnd();
                try
                {
                    data = JsonConvert.DeserializeObject<List<IPlugin>>(JsonData, new PropertyConverter());
                }
                catch (JsonSerializationException ex)
                {
                    throw new JsonSerializationException("Couldn't read the json property file!" + ex.ToString());
                }
                //data = serializer.Deserialize<List<ISettingsContainer>>(jr);
                //this.Properties = data;
            }
            return data;
        }
    }
}
