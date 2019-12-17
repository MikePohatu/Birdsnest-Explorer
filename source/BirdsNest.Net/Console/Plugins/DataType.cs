using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Console.Plugins
{
    public class DataType
    {
        private List<string> _propertynames;
        [JsonProperty("propertynames")]
        public List<string> PropertyNames 
        { 
            get 
            { 
                if (this._propertynames == null)
                {
                    this._propertynames = this.Properties.Select(x => x.Name).OrderBy(name => name).ToList();
                }
                return this._propertynames; 
            } 
        }

        [JsonProperty("properties")]
        public List<Property> Properties { get; private set; } = new List<Property>();

        [JsonProperty("default")]
        public string Default { get; private set; } = string.Empty;

        [JsonProperty("subtype")]
        public string SubType { get; private set; } = string.Empty;

        [JsonProperty("description")]
        public string Description { get; private set; } = string.Empty;

        [JsonProperty("icon")]
        public string Icon { get; private set; } = string.Empty;
    }
}
