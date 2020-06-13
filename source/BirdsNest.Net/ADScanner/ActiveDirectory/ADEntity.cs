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
using System.Collections.Generic;
using System.DirectoryServices;
using ADScanner.Neo4j;

namespace ADScanner.ActiveDirectory
{
    /// <summary>
    /// ADEntity is the base class for User and Computer classes
    /// </summary>
    public abstract class ADEntity: ADGroupMemberObject
    {
        public string PrimaryGroupID { get; private set; }

        public ADEntity(SearchResult result, string scanid) : base(result, scanid)
        {
            this.PrimaryGroupID = ADSearchResultConverter.GetSinglestringValue(result, "primaryGroupID");
            this.Properties.Add("primarygroupid", this.PrimaryGroupID);
        }
    }
}
