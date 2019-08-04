using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Console.neo4jProxy.AdvancedSearch
{
    public class SearchNode
    {
        [JsonProperty("Name")]
        public string Name { get; set; } = string.Empty;

        [JsonProperty("Label")]
        public string Label { get; set; } = string.Empty;

        public string ToSearchString()
        {
            return "(" + this.Name + ":" + this.Label + ")";
        }
    }
}
