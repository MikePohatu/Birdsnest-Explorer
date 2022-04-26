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
using System.Collections.Generic;

namespace CMScanner.CmConverter
{
    public class CmTaskSequences : IDataCollector
    {
        public string ProgressMessage { get { return "Creating task sequence nodes"; } }
        public string Query
        {
            get
            {
                return "UNWIND $Properties AS prop " +
                "MERGE (n:" + Types.CMTaskSequence + " {id:prop.ID}) " +
                "SET n:" + Types.CMConfigurationItem + " " +
                "SET n.name = prop.Name " +
                "SET n.lastscan=$ScanID " +
                "SET n.scannerid=$ScannerID " +
                "RETURN n.name";
            }
        }

        public NeoQueryData CollectData()
        {
            NeoQueryData querydata = new NeoQueryData();
            List<object> propertylist = new List<object>();

            try
            {
                string cmquery = "select * from SMS_TaskSequencePackage";

                // Run query
                using (IResultObject results = Connector.Instance.Connection.QueryProcessor.ExecuteQuery(cmquery))
                {
                    // Enumerate through the collection of objects returned by the query.
                    foreach (IResultObject resource in results)
                    {
                        propertylist.Add(new
                        {
                            ID = ResultObjectHandler.GetString(resource, "PackageID"),
                            Name = ResultObjectHandler.GetString(resource, "Name"),
                            TaskSequenceType = ((TaskSequenceType)ResultObjectHandler.GetInt(resource, "Type")).ToString()
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
            return summary.Counters.NodesCreated + " created";
        }

        public static CmTaskSequences GetInstance() { return new CmTaskSequences(); }
    }
}
