using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Console.neo4jProxy
{
    public class SearchCondition
    {
        public string Property { get; set; }
        public string Value { get; set; }
        public bool CaseSensitive { get; set; } = false;
        public bool UseWildCards { get; set; } = false;
    }
}
