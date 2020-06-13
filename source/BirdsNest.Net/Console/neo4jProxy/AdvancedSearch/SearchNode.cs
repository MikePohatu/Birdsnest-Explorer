#region license
// Copyright (c) 2019-2020 "20Road"
// 20Road Limited [https://20road.com]
//
// This file is part of BirdsNest.
//
// BirdsNest is free software: you can redistribute it and/or modify
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
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Console.neo4jProxy.AdvancedSearch
{
    public class SearchNode
    {
        [JsonProperty("Name")]
        public string Name { get; set; } = string.Empty;

        [JsonProperty("Label")]
        public string Label { get; set; } = string.Empty;

        public string TokenizedName { get; set; }

        public string ToSearchString()
        {
            return this.BuildSearchString(false);
        }

        public string ToTokenizedSearchString()
        {
            return this.BuildSearchString(true);
        }

        private string BuildSearchString(bool tokenized)
        {
            if (tokenized && string.IsNullOrWhiteSpace(this.TokenizedName))
            {
                throw new ArgumentException("Cannot build Edge tokenized search string. Edge has not been tokenized");
            }

            string name = tokenized ? this.TokenizedName : this.Name;
            string s = string.Empty;
            if (string.IsNullOrEmpty(this.Label)) { s = "(" + this.Name + ")"; }
            else { s = "(" + name + ":" + this.Label + ")"; }
            return s;
        }

        public void Tokenize(SearchTokens tokens)
        {
            this.TokenizedName = tokens.GetNameToken(this.Name);
        }
    }
}
