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
using Microsoft.ConfigurationManagement.ManagementProvider;
using Neo4j.Driver;
using System.Collections.Generic;

namespace CMScanner.CmConverter
{
    public class CmCollectionRelationships : IDataCollector
    {
        private enum RelationShipType { Limiting, Include, Exclude, Unknown }

        public string ProgressMessage { get { return "Creating collection relationships"; } }
        public string Query
        {
            get
            {
                return "UNWIND $Properties AS prop " +

                //limiting collections
                "UNWIND prop.limiting as limiting " +
                "MATCH (dep:" + Types.CMCollection + " {id: limiting.dependentCollectionID}) " +
                "MATCH (source:" + Types.CMCollection + " {id: limiting.sourceCollectionID}) " +
                "MERGE (source)-[r:" + Types.CMLimitingCollectionFor + "]->(dep) " +
                "SET r.lastscan=$ScanID " +
                "SET r.scannerid=$ScannerID " +
                "SET r.layout='mesh' " +

                //includes
                "WITH prop " +
                "UNWIND prop.includes as includes " +
                "MATCH (dep:" + Types.CMCollection + " {id: includes.dependentCollectionID}) " +
                "MATCH (source:" + Types.CMCollection + " {id: includes.sourceCollectionID}) " +
                "MERGE (dep)-[r:" + Types.CmIncludes + "]->(source) " +
                "SET r.lastscan=$ScanID " +
                "SET r.scannerid=$ScannerID " +
                "SET r.layout='mesh' " +

                //excludes
                "WITH prop " +
                "UNWIND prop.excludes as excludes " +
                "MATCH (dep:" + Types.CMCollection + " {id: excludes.dependentCollectionID}) " +
                "MATCH (source:" + Types.CMCollection + " {id: excludes.sourceCollectionID}) " +
                "MERGE (dep)-[r:" + Types.CmExcludes + "]->(source) " +
                "SET r.lastscan=$ScanID " +
                "SET r.scannerid=$ScannerID " +
                "SET r.layout='mesh' " +


                //unknowns
                "WITH prop " +
                "UNWIND prop.unknowns as unknowns " +
                "MATCH (dep:" + Types.CMCollection + " {id: unknowns.dependentCollectionID}) " +
                "MATCH (source:" + Types.CMCollection + " {id: unknowns.sourceCollectionID}) " +
                "MERGE (dep)-[r:" + Types.CMUnknownCollectionRelationship + "]->(source) " +
                "SET r.lastscan=$ScanID " +
                "SET r.scannerid=$ScannerID " +
                "SET r.layout='mesh' " +
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
                string cmquery = "select * from SMS_CollectionDependencies";
                List<object> excludes = new List<object>();
                List<object> includes = new List<object>();
                List<object> limiting = new List<object>();
                List<object> unknowns = new List<object>();

                // Run query
                using (IResultObject results = Connector.Instance.Connection.QueryProcessor.ExecuteQuery(cmquery))
                {
                    // Enumerate through the collection of objects returned by the query.
                    foreach (IResultObject resource in results)
                    {
                        string colids = ResultObjectHandler.GetString(resource, "CollectionID");
                        string resourceid = ResultObjectHandler.GetString(resource, "ResourceID");

                        var type = this.GetType(resource["RelationshipType"].IntegerValue);

                        string dependentCollectionID = resource["DependentCollectionID"].StringValue;
                        string sourceCollectionID = resource["SourceCollectionID"].StringValue;
                        object prop = new { sourceCollectionID, dependentCollectionID };

                        switch (type)
                        {
                            case RelationShipType.Exclude:
                                excludes.Add(prop);
                                break;
                            case RelationShipType.Include:
                                includes.Add(prop);
                                break;
                            case RelationShipType.Limiting:
                                limiting.Add(prop);
                                break;
                            default:
                                unknowns.Add(prop);
                                break;
                        }
                    }
                }

                propertylist.Add(new { excludes, includes, limiting, unknowns });
            }
            catch { }

            querydata.Properties = propertylist;
            return querydata;
        }

        public string GetSummaryString(IResultSummary summary)
        {
            return summary.Counters.RelationshipsCreated + " created";
        }

        public static CmCollectionRelationships GetInstance() { return new CmCollectionRelationships(); }

        /// <summary>
        /// Set the type based on int - 1=LIMITING,2=INCLUDE,3=EXCLUDE
        /// </summary>
        /// <param name="type"></param>
        private RelationShipType GetType(int type)
        {
            if (type == 1) { return RelationShipType.Limiting; }
            else if (type == 2) { return RelationShipType.Include; }
            else if (type == 3) { return RelationShipType.Exclude; }
            else { return RelationShipType.Unknown; }
        }
    }
}
