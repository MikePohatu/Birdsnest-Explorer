using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Console.neo4jProxy.AdvancedSearch.Conditions
{
    public class RegExCondition: ICondition
    {
        [JsonProperty("Type")]
        public string Type { get { return "REGEX"; } }

        [JsonProperty("Name")]
        public string Name { get; set; }

        [JsonProperty("Property")]
        public string Property { get; set; }

        [JsonProperty("Value")]
        public string Value { get; set; }

        public string TokenizedName { get; set; }
        public string TokenizedValue { get; set; }

        public string ToSearchString()
        {
            return string.Empty;
        }

        public string ToTokenizedSearchString()
        {
            return string.Empty;
        }

        public void Tokenize(SearchTokens tokens)
        {
            this.TokenizedName = tokens.GetNameToken(this.Name);
            //this.TokenizedValue = tokens.GetValueToken(this.Value);
        }
    }
}
