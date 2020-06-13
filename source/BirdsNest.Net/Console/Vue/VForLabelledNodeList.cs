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

//for use with Vue's v-for which requires a :key. The index is a number or
//string which should be unique if using in vue v-for.
//Name can be used for :key, LabelledNodes uses Node label for the key, and
//a List of nodes for that label 
using System.Collections.Generic;
using Console.neo4jProxy;

namespace Console.Vue
{
    public class VForLabelledNodeList
    {
        public string Name { get; set; }
        public Dictionary<string, List<long>> LabelledNodes { get; set; } = new Dictionary<string, List<long>>();

        public VForLabelledNodeList(string name)
        {
            this.Name = name;
        }
    }
}
