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
        public Dictionary<string, string> Icons { get; private set; } = new Dictionary<string, string>();
        public List<string> NodeLabels { get; private set; } = new List<string>();
        public List<string> EdgeLabels { get; private set; } = new List<string>();

        [JsonProperty("nodepropertydetails")]
        public Dictionary<string, PropertyListing> NodePropertyDetails { get; private set; } = new Dictionary<string, PropertyListing>();

        [JsonProperty("edgepropertydetails")]
        public Dictionary<string, PropertyListing> EdgePropertyDetails { get; private set; } = new Dictionary<string, PropertyListing>();
        /// <summary>
        /// SubTypeProperties sets the property on the node that specifies a sub-type. This is used to create another css
        /// class in the visualizer that can be used for styling
        /// </summary>
        public Dictionary<string, string> SubTypeProperties { get; private set; } = new Dictionary<string, string>();
        public Dictionary<string,Report> Reports { get; private set; } = new Dictionary<string, Report>();
    }
}
