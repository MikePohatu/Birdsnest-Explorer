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
                    this._propertynames = this.Properties.Keys.ToList();
                }
                return this._propertynames; 
            } 
        }

        [JsonProperty("properties")]
        public Dictionary<string,Property> Properties { get; private set; } = new Dictionary<string, Property>();

        [JsonProperty("default")]
        public string Default { get; private set; } = string.Empty;

        [JsonProperty("subtype")]
        public string SubType { get; private set; } = string.Empty;

        [JsonProperty("displayname")]
        public string DisplayName { get; private set; } = string.Empty;

        [JsonProperty("description")]
        public string Description { get; private set; } = string.Empty;

        [JsonProperty("icon")]
        public string Icon { get; private set; } = string.Empty;
    }
}
