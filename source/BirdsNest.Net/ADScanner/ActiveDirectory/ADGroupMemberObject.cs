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

namespace ADScanner.ActiveDirectory
{
    public abstract class ADGroupMemberObject:ADObject
    {
        public List<string> MemberOfDNs { get; protected set; }
        public string SamAccountName { get; protected set; }

        public ADGroupMemberObject(SearchResult result, string scanid) : base(result, scanid)
        {
            this.Name = ADSearchResultConverter.GetSinglestringValue(result, "name");
            this.ID = ADSearchResultConverter.GetSidAsString(result);
            this.MemberOfDNs = ADSearchResultConverter.GetStringList(result, "memberof");
            this.SamAccountName = ADSearchResultConverter.GetSinglestringValue(result, "samaccountname");

            this.Properties.Add("samaccountname", this.SamAccountName);
            this.Properties["id"] = this.ID;
        }
    }
}
