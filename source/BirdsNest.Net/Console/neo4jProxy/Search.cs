using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Console.neo4jProxy
{
    public class Search
    {
        [JsonProperty("limit")]
        public int Limit { get; set; } = 1000;

        /// <summary>
        /// List of nodes 
        /// </summary>
        [JsonProperty("nodes")]
        public List<SearchNode> Nodes { get; set; } = new List<SearchNode>();

        [JsonProperty("edges")]
        public List<SearchEdge> Edges { get; set; } = new List<SearchEdge>();


        public string GetSearchString()
        {
            string pathstr = string.Empty;
            string condstr = string.Empty;

            if (this.Nodes.Count != this.Edges.Count +1 )
            {
                throw new ArgumentException("Number of edges doesn't match number of nodes");
            }

            using (var nodeenum = this.Nodes.GetEnumerator())
            using (var condenum = this.Edges.GetEnumerator())
            {
                if (nodeenum.MoveNext() != false)
                {
                    pathstr += nodeenum.Current.GetPathString();
                    condstr += nodeenum.Current.GetWhereString();

                    while (nodeenum.MoveNext() == true)
                    {
                        condenum.MoveNext();
                        pathstr += condenum.Current.GetPathString();
                        pathstr += nodeenum.Current.GetPathString();
                        condstr += condenum.Current.GetWhereString();
                        condstr += nodeenum.Current.GetWhereString();
                    }
                }
            }

            if (string.IsNullOrEmpty(pathstr)) { return "MATCH (n) RETURN n LIMIT " + this.Limit; }
            else if (string.IsNullOrEmpty(condstr)) { return "MATCH p=" + pathstr + " UNWIND nodes(p) as n RETURN DISTINCT n LIMIT " + this.Limit + " ORDER BY LOWER(n.name)"; }
            else { return "MATCH p=" + pathstr + " WHERE " + condstr + " UNWIND nodes(p) as n RETURN DISTINCT n LIMIT " + this.Limit + " ORDER BY LOWER(n.name)"; }
        }

    }
}
