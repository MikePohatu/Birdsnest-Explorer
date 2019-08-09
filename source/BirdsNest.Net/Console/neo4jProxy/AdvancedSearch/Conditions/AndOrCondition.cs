using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Console.neo4jProxy.AdvancedSearch.Conditions
{
    public class AndOrCondition: ICondition
    {
        [JsonProperty("Type")]
        public string Type { get; set; }

        [JsonProperty(ItemConverterType = typeof(ConditionConverter))]
        public List<ICondition> Conditions { get; set; } = new List<ICondition>();

        public string ToSearchString()
        {
            string s = string.Empty;
            List<string> searchStrings = this.Conditions.Select(o => o.ToSearchString()).ToList();
            if (searchStrings.Count > 0) { s = "(" + string.Join(" " + this.Type + " ", searchStrings) + ")"; }
            return s;
        }
    }
}
