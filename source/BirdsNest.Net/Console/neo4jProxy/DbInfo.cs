﻿#region license
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
namespace Console.neo4jProxy
{
    public class DbInfo
    {
        public static DbInfo Instance { get; } = new DbInfo();
        private DbInfo() { }

        public string Version { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Edition { get; set; } = string.Empty;
    }
}
