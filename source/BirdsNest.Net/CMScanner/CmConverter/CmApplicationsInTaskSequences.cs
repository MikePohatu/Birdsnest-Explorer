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
using Microsoft.ConfigurationManagement.ManagementProvider;
using Neo4j.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMScanner.CmConverter
{
    public class CmApplicationsInTaskSequences: IDataCollector
    {
        public string ProgressMessage { get { return "Creating application/task sequence references"; } }
        public string Query
        {
            get
            {
                return "UNWIND $Properties AS prop " + 
                "MATCH (app:" + Types.CMApplication + " {id:prop.appid}) " +
                "MATCH (ts:" + Types.CMTaskSequence + " {id:prop.tsid }) " +
                "MERGE p=(ts)-[r:" + Types.CMReferences + "]->(app) " +
                "SET r.lastscan=$ScanID " +
                "SET r.scannerid=$ScannerID " +
                "RETURN p";
            }
        }

        public NeoQueryData CollectData()
        {
            NeoQueryData querydata = new NeoQueryData();
            List<object> propertylist = new List<object>();

            try
            {
                // https://docs.microsoft.com/en-us/configmgr/develop/reference/osd/sms_tasksequenceappreferencesinfo-server-wmi-class
                string cmquery = "select * from SMS_TaskSequenceAppReferencesInfo";
                List<object> retlist = new List<object>();

                // Run query
                using (IResultObject results = Connector.Instance.Connection.QueryProcessor.ExecuteQuery(cmquery))
                {
                    // Enumerate through the collection of objects returned by the query.
                    foreach (IResultObject resource in results)
                    {
                        string tsid = ResultObjectHandler.GetString(resource, "PackageID");
                        string appid = ResultObjectHandler.GetString(resource, "RefAppCI_ID");
                        propertylist.Add(new { tsid, appid });
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

        public static CmApplicationsInTaskSequences GetInstance() { return new CmApplicationsInTaskSequences(); }
    }
}
