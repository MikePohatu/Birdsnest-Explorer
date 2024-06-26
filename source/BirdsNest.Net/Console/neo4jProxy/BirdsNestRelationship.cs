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
using Neo4j.Driver;
using System.Collections.Generic;

namespace Console.neo4jProxy
{
    public class BirdsNestRelationship
    {
        public string DbId { get; private set; }
        public string Source { get; private set; }
        public string Target { get; private set; }

        /// <summary>
        /// Does the edge need to be shifted in the visualizer to avoid overlap
        /// </summary>
        public bool Shift { get; private set; } = false;
        public string Label { get; private set; } = string.Empty;
        public IReadOnlyDictionary<string, object> Properties { get; private set; }

        public static BirdsNestRelationship GetRelationship(IRelationship relrecord)
        {
            BirdsNestRelationship newrel = new BirdsNestRelationship();

            newrel.DbId = relrecord.ElementId;
            newrel.Properties = relrecord.Properties;
            newrel.Label = relrecord.Type;
            newrel.Source = relrecord.StartNodeElementId;
            newrel.Target = relrecord.EndNodeElementId;
            return newrel;
        }
    }
}
