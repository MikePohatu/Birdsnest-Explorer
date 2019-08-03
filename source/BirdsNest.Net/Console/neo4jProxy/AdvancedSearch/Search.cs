using Console.neo4jProxy.AdvancedSearch.Conditions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Console.neo4jProxy.AdvancedSearch
{
    public class Search
    {
        [JsonProperty("condition")]
        [JsonConverter(typeof(ConditionConverter))]
        public ICondition Condition { get; set; }

        public string ToSearchString()
        {
            return "MATCH ..." + this.Condition.ToSearchString();
        }
    }
}
