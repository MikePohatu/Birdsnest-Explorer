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
using System.Linq;

namespace Console.neo4jProxy
{
    public class ResultSet
    {
        private Dictionary<string, int> _propertyFilters = new Dictionary<string, int>();
        private Dictionary<string, int> _propertyNames = new Dictionary<string, int>();

        public List<BirdsNestNode> Nodes { get; private set; } = new List<BirdsNestNode>();
        public List<BirdsNestRelationship> Edges { get; private set; } = new List<BirdsNestRelationship>();

        public bool PropertyFiltersApplied { get; private set; } = false;

        public List<string> PropertyFilters { get { return this._propertyFilters.Keys.ToList(); } }
        public List<string> PropertyNames
        {
            get
            {
                List<string> propnames = this._propertyNames.Keys.ToList();
                propnames.Sort();
                return propnames;
            }
        }


        public void Append(ResultSet additionalresults)
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
                        if (!this._propertyNames.TryGetValue(key, out index))
                        {
                            index = this._propertyNames.Count;
                            this._propertyNames.Add(key, index);
                        }
                    }
                }
                foreach (BirdsNestRelationship rel in additionalresults.Edges)
                {
                    foreach (string key in rel.Properties.Keys)
                    {
                        if (!this._propertyNames.TryGetValue(key, out index))
                        {
                            index = this._propertyNames.Count;
                            this._propertyNames.Add(key, index);
                        }
                    }
                }
            }
        }

        public void AddPropertyFilter(string propname)
        {
            int i;
            if (!this._propertyFilters.TryGetValue(propname, out i))
            {
                int newindex = this._propertyFilters.Count;
                _propertyFilters.Add(propname, newindex);
            }
            this.PropertyFiltersApplied = true;
        }

        public void ClearPropertyFiltes()
        {
            this.PropertyFiltersApplied = false;
            this._propertyFilters.Clear();
        }
    }
}
