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

namespace ADScanner.ActiveDirectory
{
    class ForeignSecurityPrincipalConnectionCollector: IDataCollector
    {
        public string ProgressMessage { get { return "Creating foreign security principal relationships"; } }

        public string Query
        {
            get
            {
                return "MATCH (n:" + Types.ForeignSecurityPrincipal + " {domainid: $ScannerID})" +
                " MATCH (o:" + Types.ADObject + ") WHERE n.id = o.id AND NOT o:" + Types.ForeignSecurityPrincipal +
                " MERGE (o)-[:REPRESENTED_BY]->(n) " +
                " WITH o,n" +
                " MATCH (o)-[:REPRESENTED_BY]->(n)-[ref:" + Types.MemberOf + "]->(g)" +
                " MERGE (o)-[r:" + Types.MemberOf + "]->(g)" +
                " SET r.foreignmember = true" +
                " SET r.foreignreference = n.id" + 
                " SET r.lastscan = $ScanID" +
                " SET r.refrence = id(ref)" +
                " SET r.layout='mesh' " +
                "  RETURN n,o";
            }
        }

        public NeoQueryData CollectData()
        {
            NeoQueryData querydata = new NeoQueryData();
            return querydata;
        }
    }
}
