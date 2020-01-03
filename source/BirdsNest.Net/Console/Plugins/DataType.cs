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
//
#endregion
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Console.Plugins
{
    public class DataType
    {
        private List<string> _propertynames;
        [JsonProperty("propertynames")]
        public List<string> PropertyNames 
        { 
            get 
            { 
                if (this._propertynames == null)
                {
                    this._propertynames = this.Properties.Keys.ToList();
                }
                return this._propertynames; 
            } 
        }

        [JsonProperty("properties")]
        public Dictionary<string,Property> Properties { get; private set; } = new Dictionary<string, Property>();

        [JsonProperty("default")]
        public string Default { get; private set; } = string.Empty;

        [JsonProperty("subtype")]
        public string SubType { get; private set; } = string.Empty;

        [JsonProperty("displayname")]
        public string DisplayName { get; private set; } = string.Empty;

        [JsonProperty("description")]
        public string Description { get; private set; } = string.Empty;

        [JsonProperty("icon")]
        public string Icon { get; private set; } = string.Empty;
    }
}
