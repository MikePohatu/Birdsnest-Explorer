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
    public class CmPackagePrograms: ICmCollector
    {
        public string ProgressMessage { get { return "Creating package programs: "; } }
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
                "SET n.layout='mesh' " +
                "SET n.lastscan=$ScanID " +
                "SET n.scannerid=$ScannerID " +
                "MERGE (parent:" + Types.CMPackage + " {id:prop.PackageID}) " +
                "MERGE (parent)-[r:CONTAINS]->(n) " +
                "SET r.lastscan=$ScanID " +
                "SET r.scannerid=$ScannerID " +
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
            return summary.Counters.NodesCreated + " nodes created, " + summary.Counters.RelationshipsCreated + " relationships created";        }
    }
}
