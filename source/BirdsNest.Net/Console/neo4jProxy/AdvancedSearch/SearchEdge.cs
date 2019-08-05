using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Console.neo4jProxy.AdvancedSearch
{
    public class SearchEdge
    {
        [JsonProperty("Name")]
        public string Name { get; set; } = string.Empty;

        [JsonProperty("Label")]
        public string Label { get; set; } = string.Empty;

        [JsonProperty("Direction")]
        public string Direction { get; set; } = ">";

        [JsonProperty("Min")]
        public int Min { get; set; } = -1;

        [JsonProperty("Max")]
        public int Max { get; set; } = -1;

        public string ToSearchString()
        {
            string left = string.Empty;
            string right = string.Empty;
            string pathlength = string.Empty;
            string label = string.IsNullOrWhiteSpace(this.Label) ? string.Empty : ":" + this.Label;
            if (this.Min > -1 || this.Max > -1)
            {
                string min = "";
                string max = "";
                if (this.Min > -1) { min = this.Min.ToString(); }
                if (this.Max > -1) { max = this.Max.ToString(); }
                pathlength = "*" + min + ".." + max;
            }

            if (this.Direction == ">")
            { right = ">"; }
            else
            { left = "<"; }

            return left + "-[" + this.Name + label + pathlength + "]-" + right;
        }
    }
}
