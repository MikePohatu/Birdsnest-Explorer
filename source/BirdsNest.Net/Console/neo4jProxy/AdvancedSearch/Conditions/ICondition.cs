using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Console.neo4jProxy.AdvancedSearch.Conditions
{
    public interface ICondition
    {
        [JsonProperty("type")]
        string Type { get; }

        string ToSearchString();
        string ToTokenizedSearchString();
        void Tokenize(SearchTokens tokens);
    }
}
