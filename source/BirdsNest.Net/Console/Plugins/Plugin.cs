using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Console.Plugins
{
    public class Plugin
    {
        public string Name { get; set; }
        public string DisplayName { get; set; }
        public Dictionary<string, string> Icons { get; private set; } = new Dictionary<string, string>();
        public List<string> NodeLabels { get; private set; } = new List<string>();
        public List<string> EdgeLabels { get; private set; } = new List<string>();
        public Dictionary<string,Report> Reports { get; private set; } = new Dictionary<string, Report>();
    }
}
