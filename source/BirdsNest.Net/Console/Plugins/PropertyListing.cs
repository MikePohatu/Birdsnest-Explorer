using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Console.Plugins
{
    public class PropertyListing
    {
        [JsonProperty("properties")]
        public List<Property> Properties { get; private set; } = new List<Property>();

        [JsonProperty("default")]
        public string Default { get; private set; } = string.Empty;
    }
}
