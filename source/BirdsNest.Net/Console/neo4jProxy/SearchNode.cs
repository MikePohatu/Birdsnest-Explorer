using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Console.neo4jProxy
{
    public class SearchNode
    {
        public string Type { get; set; }
        public string Property { get; set; }
        public string Value { get; set; }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();

            return builder.ToString();
        }
    }
}
