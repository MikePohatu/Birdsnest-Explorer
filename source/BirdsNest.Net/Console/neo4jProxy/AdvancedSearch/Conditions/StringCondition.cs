#region license
// Copyright (c) 2019-2020 "20Road"
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
using System;
using System.Text.RegularExpressions;

namespace Console.neo4jProxy.AdvancedSearch.Conditions
{
    public class StringCondition : ICondition
    {
        private string _operator = "=";
        private string _regexprefix = string.Empty;
        private string _regexsuffix = string.Empty;


        public string Type { get { return "STRING"; } }

        public string Name { get; set; }
        public string Property { get; set; }

        private string _not = string.Empty;  //this will equal " NOT " in a negation situation
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

        public string Value { get; set; }
        public bool CaseSensitive { get; set; } = true;
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
            if (this.CaseSensitive == true) { s = this._not + this.Name + "." + this.Property + " " + this.Operator + " '" + this.Value + "'"; }
            else { s = this._not + this.Name + "." + this.Property + " =~ '(?i)" + this._regexprefix + Regex.Escape(this.Value).Replace("\\", "\\\\") + this._regexsuffix + "'"; }
            return s;
        }

        public string ToTokenizedSearchString()
        {
            string s = string.Empty;
            if (this.CaseSensitive == true) { s = this._not + this.TokenizedName + "." + this.Property + " " + this.Operator + " " + this.TokenizedValue; }
            else { s = this._not + this.TokenizedName + "." + this.Property + " =~ " + this.TokenizedValue; }
            return s;
        }

        public void Tokenize(SearchTokens tokens)
        {
            this.TokenizedName = tokens.GetNameToken(this.Name);
            if (this.CaseSensitive == true)
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
