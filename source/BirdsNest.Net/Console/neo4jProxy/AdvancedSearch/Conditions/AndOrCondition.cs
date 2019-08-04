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
            string s;
            if ((this.Right == null ) && ( this.Left == null )) { return string.Empty; }

            if (this.Right == null) { s = "(" + this.Left.ToSearchString() + ")"; }
            else if (this.Left == null) { s = "(" + this.Right.ToSearchString() + ")"; }
            else { s = "(" + this.Left.ToSearchString() + " " + this.Type + " " + this.Right.ToSearchString() + ")"; }
            
            return s;
        }
    }
}
