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
using common;
using System.Collections.Generic;

namespace ADScanner.ActiveDirectory
{
    public class GroupMembershipsCollector: IDataCollector
    {
        private List<object> _mappings;

        public string ProgressMessage { get { return "Creating group membership relationships"; } }

        public string Query
        {
            get
            {
                return "UNWIND $Properties as prop " +
                    "MATCH (g:" + Types.Group + " {id:prop.id, domainid: $ScannerID}) " +
                    "MATCH (n:" + Types.ADObject + "{dn: prop.memberdn, domainid: $ScannerID }) " +
                    "WITH n,g " +
                    "MERGE (n)-[r:" + Types.MemberOf + "]->(g) " +
                    "SET r.domainid = $ScannerID " +
                    "SET r.lastscan = $ScanID " +
                    "SET r.layout='mesh' " +
                    "RETURN n.id,g.name";
            }
        }


        public GroupMembershipsCollector(List<object> mappings)
        {
            this._mappings = mappings;
        }

        public NeoQueryData CollectData()
        {
            NeoQueryData querydata = new NeoQueryData();
            querydata.Properties = this._mappings;
            return querydata;
        }
    }
}
