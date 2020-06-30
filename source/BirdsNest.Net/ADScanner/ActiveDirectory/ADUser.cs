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
using System.DirectoryServices;
using System.Collections.Generic;
using common;

namespace ADScanner.ActiveDirectory
{
    public class ADUser : ADEntity
    {
        public override string Type { get { return Types.User; } }
        public bool Enabled { get; private set; }
        public string DisplayName { get; private set; }

        public string UPN { get; private set; }

        public ADUser(SearchResult result, string scanid) : base(result, scanid)
        {
            this.DisplayName = ADSearchResultConverter.GetSinglestringValue(result, "displayname");
            this.UPN = ADSearchResultConverter.GetSinglestringValue(result, "userPrincipalName");

            //find if the user is enabled. use bitwise comparison
            int istate = ADSearchResultConverter.GetIntSingleValue(result, "useraccountcontrol");
            this.Enabled = (((UserAccountControlDefinitions)istate & UserAccountControlDefinitions.ACCOUNTDISABLE) == UserAccountControlDefinitions.ACCOUNTDISABLE) ? false: true;

            this.Properties.Add("enabled", this.Enabled);
            this.Properties.Add("displayname", this.DisplayName);
            this.Properties.Add("type", this.Type);
            this.Properties.Add("userprincipalname", this.UPN);
        }
    }
}
