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
using System.Collections.Generic;

namespace Console.Plugins
{
    public class Plugin
    {
        public string Name { get; set; }
        public string Extends { get; set; }
        public List<string> ExtendedBy { get; } = new List<string>();
        public string DisplayName { get; set; }
        public string Link { get; set; } = string.Empty;

        //don't be tempted to change to SortedDictionary for Nodes to help with UI layout. These need to honor ordering for icons
        public Dictionary<string, DataType> NodeDataTypes { get; private set; } = new Dictionary<string, DataType>();

        public SortedDictionary<string, DataType> EdgeDataTypes { get; private set; } = new SortedDictionary<string, DataType>();
        public SortedDictionary<string, Report> Reports { get; private set; } = new SortedDictionary<string, Report>();
    }
}
