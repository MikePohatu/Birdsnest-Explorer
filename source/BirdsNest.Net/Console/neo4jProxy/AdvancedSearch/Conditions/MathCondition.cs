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

        [JsonProperty("type")]
        public string Type { get { return "MATH"; } }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("property")]
        public string Property { get; set; }

        [JsonProperty("value")]
        public double Value { get; set; }

        [JsonProperty("operator")]
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

        public string ToSearchString()
        {
            return this.Name + "." + this.Property + " " + this.Operator + " " + this.Value;
        }
    }
}
