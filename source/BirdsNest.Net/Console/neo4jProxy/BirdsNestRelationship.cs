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
using Neo4j.Driver.V1;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Console.neo4jProxy
{
    public class BirdsNestRelationship
    {
        public long DbId { get; private set; } = 0;
        public long Source { get; private set; } = 0;
        public long Target { get; private set; } = 0;

        /// <summary>
        /// Does the edge need to be shifted in the visualizer to avoid overlap
        /// </summary>
        public bool Shift { get; private set; } = false;
        public string Label { get; private set; } = string.Empty;
        public IReadOnlyDictionary<string, object> Properties { get; private set; }

        public static BirdsNestRelationship GetRelationship(IRelationship relrecord)
        {
            BirdsNestRelationship newrel = new BirdsNestRelationship();

            newrel.DbId = relrecord.Id;
            newrel.Properties = relrecord.Properties;
            newrel.Label = relrecord.Type;
            newrel.Source = relrecord.StartNodeId;
            newrel.Target = relrecord.EndNodeId;
            return newrel;
        }
    }
}
