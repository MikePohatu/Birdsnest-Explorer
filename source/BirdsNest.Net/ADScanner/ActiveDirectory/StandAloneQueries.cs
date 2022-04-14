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
    public static class StandAloneQueries
    {
        /// <summary>
        /// Cleanup AD group memberships that have been removed from AD. Assumes use with NeoWriter and 
        /// NeoQueryData as the parameters object
        /// </summary>
        public static string DeletedGroupMemberships
        {
            get
            {
                return "MATCH (n:" + Types.ADObject + ") -[r:" + Types.MemberOf + " {domainid:$ScannerID}]->(g:" + Types.Group + ") " +
                "WHERE NOT EXISTS(r.lastscan) OR r.lastscan <> $ScanID " +
                "DELETE r ";
            }
        }

        public static string DeletedForeignGroupMemberShips
        {
            get
            {
                return "MATCH (fsp:" + Types.ForeignSecurityPrincipal + " {domainid: $ScannerID})" +
                    " MATCH(o: " + Types.ADObject + " {id: fsp.id}) -[r:" + Types.MemberOf + "]->(g:" + Types.Group + " { domainid: $ScannerID})" +
                    " WHERE NOT(fsp)-[:" + Types.MemberOf + "]->(g)" +
                    " WITH o, g" +
                    " MATCH(o)-[rf:" + Types.MemberOf + "]-(g)" +
                    " DELETE rf";
            }
        }

        /// <summary>
        /// Cleanup AD manager relationships that have been removed from AD. Assumes use with NeoWriter and 
        /// NeoQueryData as the parameters object
        /// </summary>
        public static string DeletedManagers
        {
            get
            {
                return "MATCH (n:" + Types.ADObject + ") -[r:" + Types.Manages + " {domainid:$ScannerID}]->(g:" + Types.ADObject + ") " +
                "WHERE NOT EXISTS(r.lastscan) OR r.lastscan <> $ScanID " +
                "DELETE r ";
            }
        }

        public static string SetPrimaryGroupRelationships
        {
            get
            {
                return "MATCH(n {domainid:$ScannerID}) " +
                    "WHERE n:" + Types.Computer + " OR n:" + Types.User + " " +
                    "WITH n " +
                    "MATCH (g:" + Types.Group + " {domainid:$ScannerID}) WHERE g.rid = n.primarygroupid " +
                    "MERGE(n)-[r:" + Types.MemberOf + "]->(g) " +
                    "SET r.primarygroup = true " +
                    "SET r.lastscan = $ScanID " +
                    "SET r.domainid = $ScannerID " +
                    "SET r.layout='mesh' " +
                    "RETURN n.name,g.name ";
            }
        }

        public static string UpdateMemberCounts
        {
            get
            {
                return "MATCH (n:" + Types.Group + " {domainid:$ScannerID}) " +
                    "SET n.member_count = 0 " +
                    "WITH n " +
                    "MATCH (o:" + Types.ADObject+ ")-[r:" + Types.MemberOf + "]->(n) " +
                    "WHERE o.domainid = $ScannerID " +
                    "WITH n,count(r) AS i " +
                    "SET n.member_count = i " +
                    "RETURN n";
            }
        }

        public static string SetGroupScope
        {
            get
            {
                return "MATCH (o)-[:" + Types.MemberOf + " *]->(g:" + Types.Group + ")" +
                " WHERE o:" + Types.User + " OR o:" + Types.Computer +
                " WITH collect(DISTINCT o) as nodes, g" +
                " SET g.scope = size(nodes)" +
                " RETURN g";
            }
        }


        public static string GetMarkDeletedObjectsQuery(string label)
        {
            return 
            " MATCH(n:" + label + ") " +
            " WHERE n.domainid = $ScannerID AND n.lastscan <> $ScanID " +
            " SET n:" + Types.Deleted + " " +
            " REMOVE n:" + label + " " +
            " SET n.type='" + label + "' " +
            " RETURN n ";
        }
    }
}
