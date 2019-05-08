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
        /// First/left most node of the search
        /// </summary>
        [JsonProperty("root")]
        public SearchNode Root { get; set; }

        public string GetSearchString()
        {
            SearchNode current = this.Root;
            string pathstr = string.Empty;
            string condstr = string.Empty;

            while (current != null)
            {
                pathstr += current.GetPathString();
                condstr += current.GetWhereString();
                if (current.Next != null)
                {
                    pathstr += current.NextEdge.GetPathString();
                    condstr += current.NextEdge.GetWhereString();
                }
                current = current.Next;
            }
            
            if (string.IsNullOrEmpty(pathstr)) { return "MATCH (n) RETURN n LIMIT " + this.Limit; }
            else { return "MATCH p=" + pathstr + " WHERE " + condstr + " UNWIND nodes(p) as n RETURN DISTINCT n LIMIT " + this.Limit + " ORDER BY LOWER(n.name)"; }
        }

    }
}
