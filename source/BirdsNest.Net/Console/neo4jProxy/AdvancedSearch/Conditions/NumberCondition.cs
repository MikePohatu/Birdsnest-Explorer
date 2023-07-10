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
using System;
using System.Linq;

namespace Console.neo4jProxy.AdvancedSearch.Conditions
{
    public class NumberCondition : ICondition
    {
        private string[] _validoperators = new string[] { "=", ">", "<", "<=", ">=" };
        private string _operator = "=";

        public string Type { get { return "MATH"; } }
        public string Name { get; set; }
        public string Property { get; set; }
        public double Value { get; set; }
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
            return this.TokenizedName + "." + this.Property + " " + this.Operator + " " + this.TokenizedValue + "";
        }

        public void Tokenize(SearchTokens tokens)
        {
            this.TokenizedName = tokens.GetNameToken(this.Name);
            this.TokenizedValue = tokens.GetValueToken(this.Value);
        }
    }
}
