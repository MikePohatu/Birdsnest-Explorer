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

        [JsonProperty("Left")]
        [JsonConverter(typeof(ConditionConverter))]
        public ICondition Left { get; set; }

        [JsonProperty("Right")]
        [JsonConverter(typeof(ConditionConverter))]
        public ICondition Right { get; set; }

        public string ToSearchString()
        {
            return "(" + this.Left.ToSearchString() + " " + this.Type + " " + this.Right.ToSearchString() + ")";
        }
    }
}
