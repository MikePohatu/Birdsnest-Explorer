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

namespace ADScanner.ActiveDirectory
{
    public class ADComputer: ADEntity
    {
        public override string Type { get { return "AD_COMPUTER"; } }
        public string OperatingSystem { get; private set; }
        public string OperatingSystemVersion { get; private set; }
        public string State { get; private set; }

        public ADComputer(SearchResult result, string scanid) : base(result, scanid)
        {
            this.OperatingSystem = ADSearchResultConverter.GetSinglestringValue(result, "operatingsystem");
            this.OperatingSystemVersion = ADSearchResultConverter.GetSinglestringValue(result, "operatingsystemversion");

            //find if the computer is enabled
            int istate = ADSearchResultConverter.GetIntSingleValue(result, "useraccountcontrol");
            this.State = (istate == 4098) ? "disabled" : "enabled";

            this.Properties.Add("state", this.State);
            this.Properties.Add("operatingsystem", this.OperatingSystem);
            this.Properties.Add("operatingsystemversion", this.OperatingSystemVersion);
            this.Properties.Add("type", this.Type);
        }
    }
}
