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
                throw new ArgumentException("Cannot build Node tokenized search string. Node has not been tokenized");
            }

            if (string.IsNullOrEmpty(this.Label)) { return "()"; }

            string name = tokenized ? this.TokenizedName : this.Name;
            return $"({name}:{this.Label})";
        }

        public void Tokenize(SearchTokens tokens)
        {
            this.TokenizedName = tokens.GetNameToken(this.Name);
        }
    }
}
