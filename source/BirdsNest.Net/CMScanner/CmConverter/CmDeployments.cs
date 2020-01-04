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
using System.Collections.Generic;

namespace CMScanner.CmConverter
{
    public class CmDeployments: ICmCollector
    {
        public string ProgressMessage { get { return "Creating deployment relationships: "; } }
        public string Query
        {
            get
            {
                return "UNWIND $Properties AS dep " +
                "MATCH (n:" + Types.CMConfigurationItem + " {id:dep.CIID}) " +
                "MATCH (c:" + Types.CMCollection + " {id:dep.CollectionID}) " +
                "MERGE (c)-[r:" + Types.CMHasDeployment + "]->(n) " +
                "SET r.lastscan=$ScanID " +
                "SET r.scannerid=$ScannerID " +
                "SET r.deploymentid=dep.DeploymentID " +
                "SET r.deploymentintent=dep.DeploymentIntent " +
                "RETURN r";
            }
        }

        public NeoQueryData CollectData()
        {
            NeoQueryData querydata = new NeoQueryData();
            List<object> propertylist = new List<object>();
            querydata.Properties = propertylist;

            try
            {
                // This query selects all collections
                string cmquery = "select * from SMS_DeploymentSummary";

                // Run query
                using (IResultObject results = Connector.Instance.Connection.QueryProcessor.ExecuteQuery(cmquery))
                {
                    // Enumerate through the collection of objects returned by the query.
                    foreach (IResultObject resource in results)
                    {
                        string ciid = ResultObjectHandler.GetString(resource, "CI_ID");
                        if (string.IsNullOrWhiteSpace(ciid))
                        {
                            ciid = ResultObjectHandler.GetString(resource, "PackageID");
                        }

                        propertylist.Add(new
                        {
                            CollectionID = ResultObjectHandler.GetString(resource, "CollectionID"),
                            //CollectionName = ResultObjectHandler.GetString(resource, "CollectionName"),
                            DeploymentID = ResultObjectHandler.GetString(resource, "DeploymentID"),
                            DeploymentIntent = ResultObjectHandler.GetInt(resource, "DeploymentIntent"),
                            SoftwareName = ResultObjectHandler.GetString(resource, "SoftwareName"),
                            //PackageID = ResultObjectHandler.GetString(resource, "PackageID"),
                            //ProgramName = ResultObjectHandler.GetString(resource, "ProgramName"),
                            CIID = ciid
                            //FeatureType = ((SccmItemType)ResultObjectHandler.GetInt(resource, "FeatureType")).ToString()
                        });
                    }
                }
            }
            catch { }

            return querydata;
        }

        public string GetSummaryString(IResultSummary summary)
        {
            return summary.Counters.RelationshipsCreated + " created";
        }

        public static CmDeployments GetInstance() { return new CmDeployments(); }
    }
}
