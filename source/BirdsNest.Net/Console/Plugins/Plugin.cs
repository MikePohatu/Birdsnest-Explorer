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
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Console.Plugins
{
    public class Plugin
    {
        public string Name { get; set; }
        public string DisplayName { get; set; }

        [JsonProperty("nodedatatypes")]
        public Dictionary<string, DataType> NodeDataTypes { get; private set; } = new Dictionary<string, DataType>();

        [JsonProperty("edgedatatypes")]
        public Dictionary<string, DataType> EdgeDataTypes { get; private set; } = new Dictionary<string, DataType>();
        
        /// <summary>
        /// SubTypeProperties sets the property on the node that specifies a sub-type. This is used to create another css
        /// class in the visualizer that can be used for styling
        /// </summary>
        //public Dictionary<string, string> SubTypeProperties { get; private set; } = new Dictionary<string, string>();
        public Dictionary<string,Report> Reports { get; private set; } = new Dictionary<string, Report>();
    }
}
