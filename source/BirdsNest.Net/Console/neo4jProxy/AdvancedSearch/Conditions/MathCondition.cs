using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Console.neo4jProxy.AdvancedSearch.Conditions
{
    public class MathCondition: ICondition
    {
        private string[] _validoperators = new string[] { "=", ">", "<" };
        private string _operator = "=";

        [JsonProperty("Type")]
        public string Type { get { return "MATH"; } }

        [JsonProperty("Name")]
        public string Name { get; set; }

        [JsonProperty("Property")]
        public string Property { get; set; }

        [JsonProperty("Value")]
        public double Value { get; set; }

        [JsonProperty("Operator")]
        public string Operator
        {
            get { return this._operator; }
            set
            {
                if (this._validoperators.Any(s => value.Contains(value)))
                {
                    this._operator = value;
                }
                else
                {
                    throw new ArgumentException(value + " is not a valid operator. Valid operators are: " + string.Join(", ", this._validoperators));
                }
            }
        }

        public string TokenizedName { get; set; }
        public string TokenizedValue { get; set; }

        public string ToSearchString()
        {
            return this.Name + "." + this.Property + " " + this.Operator + " " + this.Value;
        }

        public string ToTokenizedSearchString()
        {
            return this.TokenizedName + "." + this.Property + " " + this.Operator + " $" + this.TokenizedValue + "";
        }

        public void Tokenize(SearchTokens tokens)
        {
            this.TokenizedName = tokens.GetNameToken(this.Name);
            this.TokenizedValue = tokens.GetValueToken(this.Value);
        }
    }
}
