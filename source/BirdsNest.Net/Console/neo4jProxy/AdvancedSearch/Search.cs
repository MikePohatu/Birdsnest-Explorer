using Console.neo4jProxy.AdvancedSearch.Conditions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Console.neo4jProxy.AdvancedSearch
{
    public class Search
    {
        [JsonProperty("nodes")]
        public List<SearchNode> Nodes { get; set; }

        [JsonProperty("edges")]
        public List<SearchRelationship> Edges { get; set; }

        [JsonProperty("condition")]
        [JsonConverter(typeof(ConditionConverter))]
        public ICondition Condition { get; set; }

        public string ToSearchString()
        {
            StringBuilder builder = new StringBuilder();
            string ret = string.Empty;
            builder.Append("MATCH p=");
            try
            {
                using (IEnumerator<SearchNode> nodeEmumerator = this.Nodes.GetEnumerator())
                {
                    while (nodeEmumerator.MoveNext())
                    {
                        SearchNode n = nodeEmumerator.Current;
                        using (IEnumerator<SearchRelationship> edgeEmumerator = this.Edges.GetEnumerator())
                        {
                            while (edgeEmumerator.MoveNext())
                            {
                                SearchRelationship r = edgeEmumerator.Current;
                                builder.Append(n.ToSearchString() + r.ToSearchString());
                                nodeEmumerator.MoveNext();
                                n = nodeEmumerator.Current;
                            }
                        }
                        builder.Append(n.ToSearchString());
                    }
                }
                string cond = this.Condition.ToSearchString();
                if (string.IsNullOrWhiteSpace(cond) == false) { cond = " WHERE " + cond; }
                ret = builder.ToString() + cond + " RETURN p";
            }
            catch (Exception e)
            {
                return "There was an error building the query string: " + e.Message;
            }

            return ret;
        }
    }
}
