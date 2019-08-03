using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Console.neo4jProxy.AdvancedSearch
{
    public class SearchRelationship
    {
        [JsonProperty("name")]
        public string Name { get; set; } = string.Empty;

        [JsonProperty("label")]
        public string Label { get; set; } = string.Empty;

        [JsonProperty("min")]
        public int Min { get; set; } = -1;

        [JsonProperty("max")]
        public int Max { get; set; } = -1;

        public string ToSearchString()
        {
            return "-[" + this.Name + ":" + this.Label + "]-";
        }
    }
}
