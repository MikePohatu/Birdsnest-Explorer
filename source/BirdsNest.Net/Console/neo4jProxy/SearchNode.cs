using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Console.neo4jProxy
{
    public class SearchNode: ISearchObject
    {
        [JsonProperty("identifier")]
        public string Identifier { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("next")]
        public SearchNode Next { get; set; }

        [JsonProperty("nextedge")]
        public SearchEdge NextEdge { get; set; }

        [JsonProperty("condition")]
        public ISearchConditionObject Condition { get; private set; }

        public string GetPathString()
        {
            return string.IsNullOrEmpty(this.Type) ? "(" + this.Identifier + ")" : "(" + this.Identifier + ":" + this.Type + ")";
        }

        public string GetWhereString()
        {
            return this.Condition.GetSearchString();
        }
    }
}
