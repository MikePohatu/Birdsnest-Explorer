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
using Microsoft.ConfigurationManagement.ManagementProvider;
using Neo4j.Driver.V1;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMScanner.CmConverter
{
    public class CmSoftwareUpdateGroupMembers : ICmCollector
    {
        public string ProgressMessage { get { return "Creating software update groups: "; } }
        public string Query
        {
            get
            {
                return "UNWIND $Properties AS prop " +
                "MERGE (n:" + Types.CMSoftwareUpdateGroup + " {id:prop.id}) " +
                "SET n:" + Types.CMConfigurationItem + " " +
                "SET n.name = prop.name " +
                "SET n.description = prop.description " +
                "WITH n, prop " +
                "UNWIND prop.updates as update " +
                "MATCH (u:" + Types.CMSoftwareUpdate + " {id:update}) " +
                "MERGE (u)-[r: " + Types.CMMemberOf + "]->(n) " +
                "SET r.lastscan=$ScanID " +
                "SET r.scannerid=$ScannerID " +
                "RETURN r";
            }
        }

        public NeoQueryData CollectData()
        {
            NeoQueryData querydata = new NeoQueryData();
            List<object> propertylist = new List<object>();

            try
            {
                // This query selects all collections
                string cmquery = "select * from SMS_AuthorizationList";

                // Run query
                using (IResultObject results = Connector.Instance.Connection.QueryProcessor.ExecuteQuery(cmquery))
                {
                    // Enumerate through the collection of objects returned by the query.
                    foreach (IResultObject resource in results)
                    {
                        //https://docs.microsoft.com/en-us/configmgr/develop/core/understand/how-to-read-lazy-properties-by-using-managed-code
                        resource.Get();
                        int[] lazyupdates = resource["Updates"].IntegerArrayValue;
                        List<string> updates = new List<string>();

                        foreach (int i in lazyupdates)
                        {
                            updates.Add(i.ToString());
                        }

                        propertylist.Add(new
                        {
                            id = ResultObjectHandler.GetString(resource, "CI_ID"),
                            name = ResultObjectHandler.GetString(resource, "LocalizedDisplayName"),
                            description = ResultObjectHandler.GetString(resource, "LocalizedDescription"),
                            updates = updates

                    });
                    }
                }
            }
            catch { }

            querydata.Properties = propertylist;
            return querydata;
        }

        public string GetSummaryString(IResultSummary summary)
        {
            return summary.Counters.NodesCreated + " created, " + summary.Counters.RelationshipsCreated + " memberships added";
        }

        public static CmSoftwareUpdateGroupMembers GetInstance() { return new CmSoftwareUpdateGroupMembers(); }
    }
}
