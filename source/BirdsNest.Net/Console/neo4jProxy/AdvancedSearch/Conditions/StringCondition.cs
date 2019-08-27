using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Console.neo4jProxy.AdvancedSearch.Conditions
{
    public class StringCondition: ICondition
    {
        private string _operator = "=";

        [JsonProperty("Type")]
        public string Type { get { return "STRING"; } }

        [JsonProperty("Name")]
        public string Name { get; set; }

        [JsonProperty("Property")]
        public string Property { get; set; }

        [JsonProperty("Value")]
        public string Value { get; set; }

        [JsonProperty("Operator")]
        public string Operator
        {
            get { return this._operator; }
            set { this.SetComparator(value); }
        }

        public string ToSearchString()
        {
            return this.Name + "." + this.Property + " " + this.Operator + " '" + this.Value + "'";
        }

        private void SetComparator(string s)
        {
            switch (s.ToUpper())
            {
                case "=":
                    this._operator = "=";
                    break;
                case "EQUALS":
                    this._operator = "=";
                    break;
                case "CONTAINS":
                    this._operator = "CONTAINS";
                    break;
                case "STARTSWITH":
                    this._operator = "STARTS WITH";
                    break;
                case "ENDSWITH":
                    this._operator = "ENDS WITH";
                    break;
                default:
                    break;
            }
        }
    }
}
