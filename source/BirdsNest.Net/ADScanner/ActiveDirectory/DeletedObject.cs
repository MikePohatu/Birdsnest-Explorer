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
using System.Collections.Generic;
using System.DirectoryServices;
using ADScanner.Neo4j;
using common;

namespace ADScanner.ActiveDirectory
{
    public class DeletedObject: ADObject
    {
        public override string Type { get { return Types.Deleted; } }
        public bool IsDeleted { get; private set; }
        public bool IsRecycled { get; private set; }
        public string LastKnownParent { get; private set; }

        public DeletedObject(SearchResult result, string scanid) : base(result, scanid)
        {
            this.IsDeleted = ADSearchResultConverter.GetBoolSingleValue(result, "isdeleted");
            this.IsRecycled = ADSearchResultConverter.GetBoolSingleValue(result, "isrecycled");
            this.LastKnownParent = ADSearchResultConverter.GetSinglestringValue(result, "lastKnownParent");

            this.Properties.Add("isdeleted", this.IsDeleted);
            this.Properties.Add("isrecycled", this.IsRecycled);
            this.Properties.Add("lastknownparent", this.LastKnownParent);
            this.Properties.Add("type", this.Type);
        }
    }
}
