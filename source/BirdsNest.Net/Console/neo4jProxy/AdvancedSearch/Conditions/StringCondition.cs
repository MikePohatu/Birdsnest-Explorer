using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Console.neo4jProxy.AdvancedSearch.Conditions
{
    public class StringCondition: ICondition
    {
        private string _operator = "=";
        private string _regexprefix = string.Empty;
        private string _regexsuffix = string.Empty;

        [JsonProperty("Type")]
        public string Type { get { return "STRING"; } }

        [JsonProperty("Name")]
        public string Name { get; set; }

        [JsonProperty("Property")]
        public string Property { get; set; }

        [JsonProperty("Value")]
        public string Value { get; set; }

        [JsonProperty("CaseSensitive")]
        public bool CaseSensitive { get; set; } = true;

        [JsonProperty("Operator")]
        public string Operator
        {
            get { return this._operator; }
            set { this.SetComparator(value); }
        }

        public string TokenizedName { get; set; }
        public string TokenizedValue { get; set; }

        public string ToSearchString()
        {
            string s = string.Empty;
            if (this.CaseSensitive == true) { s = this.Name + "." + this.Property + " " + this.Operator + " '" + this.Value + "'"; }
            else { s = this.Name + "." + this.Property + " =~ (?i)" + this._regexprefix + Regex.Escape(this.Value) + this._regexsuffix; }
            return s;
        }

        public string ToTokenizedSearchString()
        {
            string s = string.Empty;
            if (this.CaseSensitive == true) { s = this.TokenizedName + "." + this.Property + " " + this.Operator + " " + this.TokenizedValue; }
            else { s = this.TokenizedName + "." + this.Property + " =~ " + this.TokenizedValue; }
            return s;
        }

        public void Tokenize(SearchTokens tokens)
        {
            this.TokenizedName = tokens.GetNameToken(this.Name);
            if (this.CaseSensitive == true )
            {
                this.TokenizedValue = tokens.GetValueToken(this.Value);
            }
            else
            {
                string regex = "(?i)" + this._regexprefix + Regex.Escape(this.Value) + this._regexsuffix;
                this.TokenizedValue = tokens.GetValueToken(regex);
            }
            
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
                    this._regexprefix = ".*";
                    this._regexsuffix = ".*";
                    break;
                case "STARTSWITH":
                    this._operator = "STARTS WITH";
                    this._regexsuffix = ".*";
                    break;
                case "ENDSWITH":
                    this._operator = "ENDS WITH";
                    this._regexprefix = ".*";
                    break;
                default:
                    throw new ArgumentException("Invalid operator: " + s);
            }
        }
    }
}
