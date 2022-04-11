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
    public class CmPackagePrograms: IDataCollector
    {
        public string ProgressMessage { get { return "Creating package programs"; } }
        public string Query
        {
            get
            {
                return "UNWIND $Properties AS prop " +
                "MERGE (n:" + Types.CMPackageProgram + " {id:prop.ID}) " +
                "SET n:" + Types.CMConfigurationItem + " " +
                "SET n.name = prop.Name " +
                "SET n.packageid = prop.PackageID " +
                "SET n.dependantid = prop.DependantID " +
                "SET n.commandline = prop.CommandLine " +
                "SET n.lastscan=$ScanID " +
                "SET n.scannerid=$ScannerID " +
                "MERGE (parent:" + Types.CMPackage + " {id:prop.PackageID}) " +
                "MERGE (parent)-[r:" + Types.CMHasProgram + "]->(n) " +
                "SET r.lastscan=$ScanID " +
                "SET r.scannerid=$ScannerID " +
                "SET r.layout='mesh' " +
                "RETURN n.name";
            }
        }

        public NeoQueryData CollectData()
        {
            NeoQueryData querydata = new NeoQueryData();
            List<object> propertylist = new List<object>();

            try
            {
                // This query selects all collections
                string cmquery = "select * from SMS_Program";

                // Run query
                using (IResultObject results = Connector.Instance.Connection.QueryProcessor.ExecuteQuery(cmquery))
                {
                    // Enumerate through the collection of objects returned by the query.
                    foreach (IResultObject resource in results)
                    {
                        string programName = ResultObjectHandler.GetString(resource, "ProgramName");
                        string packageID = ResultObjectHandler.GetString(resource, "PackageID");

                        propertylist.Add(new
                        {
                            ProgramName = ResultObjectHandler.GetString(resource, "ProgramName"),
                            DependentProgram = ResultObjectHandler.GetString(resource, "DependentProgram"),
                            Description = ResultObjectHandler.GetString(resource, "Description"),
                            PackageName = ResultObjectHandler.GetString(resource, "PackageName"),
                            PackageID = ResultObjectHandler.GetString(resource, "PackageID"),
                            ID = packageID + ";;" + programName,
                            //Name = PackageName + " (" + ProgramName + ")";
                            Name = programName
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
            return summary.Counters.NodesCreated + " nodes created, " + summary.Counters.RelationshipsCreated + " relationships created";        
        }

        public static CmPackagePrograms GetInstance() { return new CmPackagePrograms(); }

    }
}
