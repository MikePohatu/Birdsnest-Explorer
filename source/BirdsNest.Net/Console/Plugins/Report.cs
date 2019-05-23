using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Console.Plugins
{
    public class Report
    {
        public string DisplayName { get; set; }
        public string Description { get; set; }
        public string Query { get; set; }
        public List<string> PropertyFilters { get; set; } = new List<string>();
    }
}
