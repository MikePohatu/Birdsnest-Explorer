#region license
// Copyright (c) 2019-2023 "20Road"
// 20Road Limited [https://www.20road.com]
//
// This file is part of Birdsnest Explorer.
//
// Birdsnest Explorer is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, version 3 of the License.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.
#endregion
using Newtonsoft.Json;
using System;

namespace Console.neo4jProxy.AdvancedSearch.Conditions
{
    public class BooleanCondition : ICondition
    {
        private string _operator = "=";

        public string TokenizedName { get; set; }
        public string TokenizedValue { get; set; }

        [JsonProperty("type")]
        public string Type { get { return "BOOLEAN"; } }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("property")]
        public string Property { get; set; }

        [JsonProperty("operator")]
        public string Operator
        {
            get { return this._operator; }
            set { this.SetComparator(value); }
        }

        private string _not = string.Empty;  //this will equal " NOT " in a negation situation
        [JsonProperty("not")]
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

        [JsonProperty("value")]
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
