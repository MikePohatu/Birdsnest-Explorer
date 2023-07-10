#region license
// Copyright (c) 2019-2023 "20Road"
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

namespace ADScanner.ActiveDirectory
{
    public class ManagerCollector : IDataCollector
    {
        public string ProgressMessage { get { return "Creating manager relationships"; } }

        public string Query
        {
            get
            {
                return "MATCH (n:" + Types.User + " {domainid: $ScannerID}) " +
                    "MATCH (m:" + Types.User + " {dn: n.manager, domainid: $ScannerID }) " +
                    "WITH n,m " +
                    "MERGE (m)-[r:" + Types.Manages + "]->(n) " +
                    "SET r.domainid = $ScannerID " +
                    "SET r.lastscan = $ScanID " +
                    "SET r.layout='tree' " +
                    "RETURN n.id,n.name";
            }
        }

        public NeoQueryData CollectData()
        {
            NeoQueryData querydata = new NeoQueryData();
            return querydata;
        }
    }
}
