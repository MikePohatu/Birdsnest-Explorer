using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Console.neo4jProxy.AdvancedSearch.Conditions
{
    public class RegExCondition: ICondition
    {
        [JsonProperty("type")]
        public string Type { get { return "REGEX"; } }

        [JsonProperty("name")]
        public string Name { get; set; }

        public string ToSearchString()
        {
            return string.Empty;
        }
    }
}
