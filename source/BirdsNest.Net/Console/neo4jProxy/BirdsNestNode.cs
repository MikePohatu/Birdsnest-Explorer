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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Neo4j.Driver.V1;

namespace Console.neo4jProxy
{
    public class BirdsNestNode
    {
        public long DbId { get; private set; } = 0;
        public string Name { get; private set; } = string.Empty;
        public List<string> Labels { get; private set; } = new List<string>();
        public long Scope { get; private set; } = 1;
        public IReadOnlyDictionary<string,object> Properties { get; private set; }

        public static BirdsNestNode GetNode(INode noderecord)
        {
            BirdsNestNode newnode = new BirdsNestNode();

            newnode.DbId = noderecord.Id;
            newnode.Properties = noderecord.Properties;

            object o;
            if (newnode.Properties.TryGetValue("name", out o))
            {
                newnode.Name = o.ToString();
            }

            if (newnode.Properties.TryGetValue("scope", out o))
            {
                newnode.Scope = (long)o;
            }

            newnode.Labels = noderecord.Labels as List<string>;

            return newnode;
        }
    }
}
