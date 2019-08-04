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

        [JsonProperty("Type")]
        public string Type { get { return "STRING"; } }

        [JsonProperty("Name")]
        public string Name { get; set; }

        [JsonProperty("Property")]
        public string Property { get; set; }

        [JsonProperty("Value")]
        public string Value { get; set; }

        [JsonProperty("Comparator")]
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
                case "=":
                    this._comparator = "=";
                    break;
                case "EQUALS":
                    this._comparator = "=";
                    break;
                case "CONTAINS":
                    this._comparator = "CONTAINS";
                    break;
                case "STARTSWITH":
                    this._comparator = "STARTS WITH";
                    break;
                case "ENDSWITH":
                    this._comparator = "ENDS WITH";
                    break;
                default:
                    break;
            }
        }
    }
}
