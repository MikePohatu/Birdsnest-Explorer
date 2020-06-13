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
    public class CmSoftwareUpdateSupersedence : IDataCollector
    {
        public string ProgressMessage { get { return "Creating software update supersedence relationships: "; } }
        public string Query
        {
            get
            {
                return "UNWIND $Properties AS prop " +
                "MATCH (from:" + Types.CMSoftwareUpdate + "{id:prop.FromCIID}) " +
                "MATCH (to:" + Types.CMSoftwareUpdate + "{id:prop.ToCIID}) " +
                "MERGE p=(to)-[r:" + Types.CMSuperSededBy + "]->(from) " +
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
                string cmquery = "select * from SMS_CIRelation WHERE RelationType=6";

                // Run query
                using (IResultObject results = Connector.Instance.Connection.QueryProcessor.ExecuteQuery(cmquery))
                {
                    // Enumerate through the collection of objects returned by the query.
                    foreach (IResultObject resource in results)
                    {
                        propertylist.Add(new
                        {
                            FromCIID = ResultObjectHandler.GetString(resource, "FromCIID"),
                            ToCIID = ResultObjectHandler.GetString(resource, "ToCIID")
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
            return summary.Counters.RelationshipsCreated + " created";
        }

        public static CmSoftwareUpdateSupersedence GetInstance() { return new CmSoftwareUpdateSupersedence(); }
    }
}
