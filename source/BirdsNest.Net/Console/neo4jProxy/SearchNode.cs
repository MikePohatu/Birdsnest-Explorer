using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Console.neo4jProxy
{
    public class SearchNode
    {
        

        public string Identifier { get; set; }
        public string Type { get; set; }

        public List<SearchCondition> Conditions { get; private set; } = new List<SearchCondition>();

        public string NodeString()
        {
            return string.IsNullOrEmpty(this.Type) ? "(" + this.Identifier + ")" : "(" + this.Identifier + ":" + this.Type + ")";
        }

        public string WhereString()
        {
            StringBuilder builder = new StringBuilder();
            return builder.ToString();
        }
    }
}
