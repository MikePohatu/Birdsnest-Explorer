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
using common;

namespace ADScanner.ActiveDirectory
{
    public abstract class ADObject: IBirdsNestNode
    {
        public string Name { get; protected set; }
        public virtual string Type { get { return "AD_Object"; } }
        public string ID { get; protected set; }
        public string Path { get; private set; }
        public string CN { get; private set; }
        public string DN { get; private set; }
        public string Description { get; private set; }
        public string Info { get; private set; }
        public Dictionary<string, object> Properties { get; private set; }
        public string ScanId;

        public ADObject(SearchResult result, string scanid)
        {
            this.ScanId = scanid;
            this.Path = ADSearchResultConverter.GetSinglestringValue(result, "distinguishedname");
            this.ID = this.Path;
            this.DN = this.Path;
            this.CN = ADSearchResultConverter.GetSinglestringValue(result, "cn");
            this.Name = ADSearchResultConverter.GetSinglestringValue(result, "Name");
            this.Info = ADSearchResultConverter.GetSinglestringValue(result, "Info");
            this.Description = ADSearchResultConverter.GetSinglestringValue(result, "Description");

            this.Properties = new Dictionary<string, object>
            {
                {"name", this.Name},
                {"id", this.ID},
                {"path", this.Path},
                {"dn", this.DN},
                {"cn", this.CN },
                {"description", this.Description },
                {"info", this.Info },
                {"scanid",this.ScanId }
            };
        }
    }
}
