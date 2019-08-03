using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Console.neo4jProxy.AdvancedSearch.Conditions
{
    public class StringCondition: ICondition
    {
        private string _comparator = "=";

        [JsonProperty("type")]
        public string Type { get { return "STRING"; } }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("property")]
        public string Property { get; set; }

        [JsonProperty("value")]
        public string Value { get; set; }

        [JsonProperty("comparator")]
        public string Comparator
        {
            get { return this._comparator; }
            set { this.SetComparator(value); }
        }

        public string ToSearchString()
        {
            return this.Name + "." + this.Property + " " + this.Comparator + " '" + this.Value + "'";
        }

        private void SetComparator(string s)
        {
            switch (s.ToUpper())
            {
                case "EQUALS":
                    this._comparator = "=";
                    break;
                case "STARTSWITH":
                    this._comparator = "STARTS WITH";
                    break;
                default:
                    break;
            }
        }
    }
}
