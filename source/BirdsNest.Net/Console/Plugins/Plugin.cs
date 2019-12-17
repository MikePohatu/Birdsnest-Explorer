using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Console.Plugins
{
    public class Plugin
    {
        public string Name { get; set; }
        public string DisplayName { get; set; }

        [JsonProperty("nodedatatypes")]
        public Dictionary<string, DataType> NodeDataTypes { get; private set; } = new Dictionary<string, DataType>();

        [JsonProperty("edgedatatypes")]
        public Dictionary<string, DataType> EdgeDataTypes { get; private set; } = new Dictionary<string, DataType>();
        
        /// <summary>
        /// SubTypeProperties sets the property on the node that specifies a sub-type. This is used to create another css
        /// class in the visualizer that can be used for styling
        /// </summary>
        //public Dictionary<string, string> SubTypeProperties { get; private set; } = new Dictionary<string, string>();
        public Dictionary<string,Report> Reports { get; private set; } = new Dictionary<string, Report>();
    }
}
