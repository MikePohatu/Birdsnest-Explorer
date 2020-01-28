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
using common;
using Microsoft.Graph;
using Neo4j.Driver.V1;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureADScanner.Azure
{
    public class AadGroupMemberships: IDataCollector
    {
        public List<string> GroupIDs { get; set; }

        public string ProgressMessage { get { return "Creating group membership connections: "; } }
        public string Query
        {
            get
            {
                return "UNWIND $Properties AS prop " +
                    "MATCH(g:" +Types.AadGroup+" { id: prop.groupid}) " +
                    "MATCH(m:" + Types.AadObject + " { id: prop.memberid}) " +
                    "MERGE p = (m)-[r:" +Types.AadMemberOf+"]->(g) " +
                    "SET r.lastscan = $ScanID " +
                    "SET r.scannerid = $ScannerID " +
                    "RETURN p";
            }
        }

        public NeoQueryData CollectData()
        {
            NeoQueryData querydata = new NeoQueryData();
            List<object> propertylist = new List<object>();

            try
            {
                while (this.GroupIDs?.Count > 0)
                {
                    foreach (string groupid in ListExtensions.ListPop<string>(this.GroupIDs, 20))
                    {
                        IGroupMembersCollectionWithReferencesPage page = Connector.Instance.Client.Groups[groupid].Members
                            .Request()
                            .GetAsync().Result;

                        while (page != null)
                        {
                            foreach (DirectoryObject member in page.CurrentPage)
                            {
                                propertylist.Add(new
                                {
                                    groupid = groupid,
                                    memberid = member.Id
                                });
                            }

                            page = page.NextPageRequest?.GetAsync().Result;
                        }
                    }
                }            
            }
            catch { }

            querydata.Properties = propertylist;
            return querydata;
        }
    }
}
