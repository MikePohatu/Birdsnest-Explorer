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

//for use with Vue's v-for which requires a :key. The index is a number or
//string which should be unique if using in vue v-for.
//Name can be used for :key, LabelledNodes uses Node label for the key, and
//a List of nodes for that label 
using Console.neo4jProxy;
using System.Collections.Generic;
using System.Linq;

namespace Console.Vue
{
    public class VForLabelledNodeList
    {
        public string Name { get; set; }

        private SortedDictionary<string, List<BirdsNestNodeSimple>> _labelledNodes;
        public SortedDictionary<string, List<BirdsNestNodeSimple>> LabelledNodes
        {
            get
            {
                if (this._labelledNodes == null)
                {
                    this.Sort();
                    return this._labelledNodes;
                }
                else
                {
                    return this._labelledNodes;
                }
            }
        }



        /// <summary>
        /// Dictionary<label, Dictionary<DbId, BirdsNestNodeSimple>>
        /// </summary>
        private SortedDictionary<string, Dictionary<long, BirdsNestNodeSimple>> _labelledDictionary { get; set; } = new SortedDictionary<string, Dictionary<long, BirdsNestNodeSimple>>();

        public VForLabelledNodeList(string name)
        {
            this.Name = name;
        }

        public void AddNode(BirdsNestNode node, string label)
        {
            Dictionary<long, BirdsNestNodeSimple> labeldic;

            if (this._labelledDictionary.TryGetValue(label, out labeldic))
            {
                if (labeldic.ContainsKey(node.DbId) == false)
                {
                    labeldic.Add(node.DbId, node.GetSimple());
                }
            }
            else
            {
                labeldic = new Dictionary<long, BirdsNestNodeSimple>();
                this._labelledDictionary.Add(label, labeldic);
                labeldic.Add(node.DbId, node.GetSimple());
            }
        }

        public void Sort()
        {
            Dictionary<long, BirdsNestNodeSimple> current;
            this._labelledNodes = new SortedDictionary<string, List<BirdsNestNodeSimple>>();

            foreach (string key in this._labelledDictionary.Keys)
            {
                current = this._labelledDictionary[key];
                List<BirdsNestNodeSimple> simples = current.Values.ToList();
                simples.Sort(delegate (BirdsNestNodeSimple x, BirdsNestNodeSimple y)
                {
                    return x.Name.CompareTo(y.Name);
                });
                this._labelledNodes.Add(key, simples);
            }
        }
    }
}
