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
        [JsonProperty("Nodes")]
        public List<SearchNode> Nodes { get; set; }

        [JsonProperty("Edges")]
        public List<SearchEdge> Edges { get; set; }

        [JsonProperty("Condition")]
        [JsonConverter(typeof(ConditionConverter))]
        public ICondition Condition { get; set; }

        public SearchTokens Tokens { get; private set; } = new SearchTokens();

        public string ToSharableSearchString()
        {
            StringBuilder builder = this.BuildBaseSearchString();
            builder.Append(" RETURN p");
            return builder.ToString();
        }

        private StringBuilder BuildBaseSearchString()
        {
            StringBuilder builder = new StringBuilder();

            builder.Append("MATCH p=");
            try
            {
                using (IEnumerator<SearchNode> nodeEmumerator = this.Nodes.GetEnumerator())
                {
                    while (nodeEmumerator.MoveNext())
                    {
                        SearchNode n = nodeEmumerator.Current;
                        using (IEnumerator<SearchEdge> edgeEmumerator = this.Edges.GetEnumerator())
                        {
                            while (edgeEmumerator.MoveNext())
                            {
                                SearchEdge edge = edgeEmumerator.Current;
                                builder.Append(n.ToSearchString() + edge.ToSearchString());
                                nodeEmumerator.MoveNext();
                                n = nodeEmumerator.Current;
                            }
                        }
                        builder.Append(n.ToSearchString());
                    }
                }
                string cond = this.Condition?.ToSearchString();
                if (string.IsNullOrWhiteSpace(cond) == false) { cond = " WHERE " + cond; }
                builder.Append(cond);
            }
            catch (Exception e)
            {
                throw new InvalidOperationException( "There was an error building the query string: " + e.Message);
            }
            return builder;
        }

        public string ToSearchString()
        {
            StringBuilder builder = this.BuildBaseSearchString();
            builder.Append(" UNWIND nodes(p) as bnest_nodes RETURN DISTINCT bnest_nodes ORDER BY LOWER(bnest_nodes.name)");
            return builder.ToString();
        }

        public string ToTokenizedSearchString()
        {
            this.Tokenize();
            StringBuilder builder = new StringBuilder();

            builder.Append("MATCH p=");
            try
            {
                using (IEnumerator<SearchNode> nodeEmumerator = this.Nodes.GetEnumerator())
                {
                    while (nodeEmumerator.MoveNext())
                    {
                        SearchNode n = nodeEmumerator.Current;
                        using (IEnumerator<SearchEdge> edgeEmumerator = this.Edges.GetEnumerator())
                        {
                            while (edgeEmumerator.MoveNext())
                            {
                                SearchEdge edge = edgeEmumerator.Current;
                                builder.Append(n.ToTokenizedSearchString() + edge.ToTokenizedSearchString());
                                nodeEmumerator.MoveNext();
                                n = nodeEmumerator.Current;
                            }
                        }
                        builder.Append(n.ToTokenizedSearchString());
                    }
                }
                string cond = this.Condition?.ToTokenizedSearchString();
                if (string.IsNullOrWhiteSpace(cond) == false) { cond = " WHERE " + cond; }
                builder.Append(cond);
                builder.Append(" UNWIND nodes(p) as bnest_nodes RETURN DISTINCT bnest_nodes ORDER BY LOWER(bnest_nodes.name)");
            }
            catch (Exception e)
            {
                return "There was an error building the query string: " + e.Message;
            }

            return builder.ToString();
        }

        public void Tokenize()
        {
            if (this.Nodes != null)
            {
                foreach (var node in this.Nodes)
                {
                    node.Tokenize(this.Tokens);
                }
            }
            
            if (this.Edges != null)
            {
                foreach (var edge in this.Edges)
                {
                    edge.Tokenize(this.Tokens);
                }
            }

            this.Condition?.Tokenize(this.Tokens);
        }

    } 
}
