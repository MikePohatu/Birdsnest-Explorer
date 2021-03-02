#region license
// Copyright (c) 2019-2020 "20Road"
// 20Road Limited [https://www.20road.com]
//
// This file is part of Birdsnest Explorer.
//
// Birdsnest Explorer is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, version 3 of the License.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.
#endregion
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
        public List<SearchEdge> Edges { get; set; }

        [JsonProperty("condition")]
        [JsonConverter(typeof(ConditionConverter))]
        public ICondition Condition { get; set; }

        public SearchTokens Tokens { get; private set; } = new SearchTokens();

        public string ToSharableSearchString()
        {
            StringBuilder builder = this.BuildBaseSearchString();
            builder.Append(" RETURN p");
            string s = builder.ToString();
            return s;
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
            builder.Append(" UNWIND nodes(p) as bnest_nodes RETURN DISTINCT bnest_nodes ORDER BY toLower(bnest_nodes.name)");
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
                builder.Append(" UNWIND nodes(p) as bnest_nodes RETURN DISTINCT bnest_nodes ORDER BY toLower(bnest_nodes.name)");
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
