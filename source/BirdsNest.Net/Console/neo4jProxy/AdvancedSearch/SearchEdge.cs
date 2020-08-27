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
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Console.neo4jProxy.AdvancedSearch
{
    public class SearchEdge
    {
        [JsonProperty("Name")]
        public string Name { get; set; } = string.Empty;

        [JsonProperty("Label")]
        public string Label { get; set; } = string.Empty;

        [JsonProperty("Direction")]
        public string Direction { get; set; } = ">";

        [JsonProperty("Min")]
        public int Min { get; set; } = -1;

        [JsonProperty("Max")]
        public int Max { get; set; } = -1;

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

            string left = string.Empty;
            string right = string.Empty;
            string pathlength;
            string label = string.IsNullOrWhiteSpace(this.Label) ? string.Empty : ":" + this.Label;           
            string min = "";
            string max = "";

            //variable names are not supported on variable length relationships and can cause slow downs with no label
            //or conditions
            if (this.Min < 0 || this.Max < 0 || string.IsNullOrWhiteSpace(this.Label))
            {
                name = string.Empty;
            }

            if (this.Min == 1 && this.Max == 1)
			{
				pathlength = string.Empty;
			}
			else if (this.Min < 0 && this.Max < 0)
            {
                pathlength = "*";
            }
            else if (this.Min == 1 && this.Max == 1)
            {
                pathlength = string.Empty;
            }
            else
            {
                if (this.Min > -1) { min = this.Min.ToString(); }
                if (this.Max > -1) { max = this.Max.ToString(); }
                pathlength = "*" + this.Min + ".." + this.Max;
            }

            if (this.Direction == ">")
            { right = ">"; }
            else
            { left = "<"; }

            return left + "-[" + name + label + pathlength + "]-" + right;
        }

        public void Tokenize(SearchTokens tokens)
        {
            this.TokenizedName = tokens.GetNameToken(this.Name);
        }
    }
}
