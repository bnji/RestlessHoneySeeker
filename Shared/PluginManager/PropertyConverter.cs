using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json.Converters;

namespace PluginManager
{
    public class PropertyConverter : CustomCreationConverter<IPlugin>
    {
        public override IPlugin Create(Type objectType)
        {
            return new Plugin();
        }
    }
}
