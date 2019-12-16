using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Console.neo4jProxy.AdvancedSearch.Conditions
{
    public class BooleanCondition: ICondition
    {
        private string _operator = "=";

        public string TokenizedName { get; set; }
        public string TokenizedValue { get; set; }

        [JsonProperty("Type")]
        public string Type { get { return "BOOLEAN"; } }

        [JsonProperty("Name")]
        public string Name { get; set; }

        [JsonProperty("Property")]
        public string Property { get; set; }

        [JsonProperty("Operator")]
        public string Operator
        {
            get { return this._operator; }
            set { this.SetComparator(value); }
        }

        private string _not = string.Empty;  //this will equal " NOT " in a negation situation
        [JsonProperty("Not")]
        public bool Not
        {
            get { return this._not == string.Empty ? false : true; }
            set
            {
                if (value == true)
                { this._not = "NOT "; }
                else
                { this._not = string.Empty; }
            }
        }

        [JsonProperty("Value")]
        public bool Value { get; set; }

        public string ToSearchString()
        {
            return this._not + this.Name + "." + this.Property + " " + this.Operator + " " + this.Value.ToString();
        }

        public string ToTokenizedSearchString()
        {
            return this._not + this.TokenizedName + "." + this.Property + " " + this.Operator + " " + this.TokenizedValue; ;
        }

        public void Tokenize(SearchTokens tokens)
        {
            this.TokenizedName = tokens.GetNameToken(this.Name);
            this.TokenizedValue = tokens.GetValueToken(this.Value);
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
                default:
                    throw new ArgumentException("Invalid operator: " + s);
            }
        }
    }
}
