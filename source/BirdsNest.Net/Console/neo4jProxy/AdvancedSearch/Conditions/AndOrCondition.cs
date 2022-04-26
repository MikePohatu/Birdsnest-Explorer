﻿#region license
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
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace Console.neo4jProxy.AdvancedSearch.Conditions
{
    public class AndOrCondition : ICondition
    {
        public string Type { get; set; }

        [JsonProperty(ItemConverterType = typeof(ConditionConverter))]
        public List<ICondition> Conditions { get; set; } = new List<ICondition>();

        public string ToSearchString()
        {
            string s = string.Empty;
            List<string> searchStrings = this.Conditions.Select(o => o.ToSearchString()).ToList();
            if (searchStrings.Count > 0) { s = "(" + string.Join(" " + this.Type + " ", searchStrings) + ")"; }
            return s;
        }

        public string ToTokenizedSearchString()
        {
            string s = string.Empty;
            List<string> searchStrings = this.Conditions.Select(o => o.ToTokenizedSearchString()).ToList();
            if (searchStrings.Count > 0) { s = "(" + string.Join(" " + this.Type + " ", searchStrings) + ")"; }
            return s;
        }

        public void Tokenize(SearchTokens tokens)
        {
            foreach (ICondition cond in this.Conditions)
            {
                cond.Tokenize(tokens);
            }
        }
    }
}
