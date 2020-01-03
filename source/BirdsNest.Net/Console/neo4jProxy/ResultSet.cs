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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Console.neo4jProxy
{
    public class ResultSet
    {
        public List<BirdsNestNode> Nodes { get; private set; } = new List<BirdsNestNode>();
        public List<BirdsNestRelationship> Edges { get; private set; } = new List<BirdsNestRelationship>();

        public bool PropertyFiltersApplied { get; private set; } = false;

        /// <summary>
        /// String property name, with an int index number, suitable for use as a column number
        /// </summary>
        public Dictionary<string, int> PropertyFilters { get; private set; } = new Dictionary<string, int>();

        /// <summary>
        /// Property name and an index number for that name
        /// </summary>
        public Dictionary<string, int> PropertyNames { get; private set; } = new Dictionary<string, int>();

        public void Append (ResultSet additionalresults)
        {
            if (additionalresults != null)
            {
                this.Nodes.AddRange(additionalresults.Nodes);
                this.Edges.AddRange(additionalresults.Edges);
                int index;

                foreach (BirdsNestNode node in additionalresults.Nodes)
                {
                    foreach (string key in node.Properties.Keys)
                    {
                        if (!this.PropertyNames.TryGetValue(key, out index))
                        {
                            index = this.PropertyNames.Count;
                            this.PropertyNames.Add(key, index);
                        }
                    }
                }
                foreach (BirdsNestRelationship rel in additionalresults.Edges)
                {
                    foreach (string key in rel.Properties.Keys)
                    {
                        if (!this.PropertyNames.TryGetValue(key, out index))
                        {
                            index = this.PropertyNames.Count;
                            this.PropertyNames.Add(key, index);
                        }
                    }
                }
            }
        }

        public void AddPropertyFilter(string propname)
        {
            int i;
            if (!this.PropertyFilters.TryGetValue(propname, out i))
            {
                int newindex = this.PropertyFilters.Count;
                PropertyFilters.Add(propname, newindex);
            }
            this.PropertyFiltersApplied = true;
        }

        public void ClearPropertyFiltes()
        {
            this.PropertyFiltersApplied = false;
            this.PropertyFilters.Clear();
        }
    }
}
