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
namespace Console.neo4jProxy.AdvancedSearch.Conditions
{
    public class RegExCondition : ICondition
    {
        public string Type { get { return "REGEX"; } }
        public string Name { get; set; }
        public string Property { get; set; }
        public string Value { get; set; }

        public string TokenizedName { get; set; }
        public string TokenizedValue { get; set; }

        public string ToSearchString()
        {
            return string.Empty;
        }

        public string ToTokenizedSearchString()
        {
            return string.Empty;
        }

        public void Tokenize(SearchTokens tokens)
        {
            this.TokenizedName = tokens.GetNameToken(this.Name);
            //this.TokenizedValue = tokens.GetValueToken(this.Value);
        }
    }
}
