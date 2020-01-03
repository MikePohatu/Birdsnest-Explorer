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
    public class CmApplicationDependencies: ICmCollector
    {
        public string ProgressMessage { get { return "Creating application dependencies: "; } }
        public string Query
        {
            //TO DO
            get
            {
                //return "UNWIND $Properties AS prop " +
                //"MERGE (n:" + Types.CMApplication + "{id:prop.ID}) " +
                //"SET n:" + Types.CMConfigurationItem + " " +
                //"SET n.name = prop.Name " +
                //"SET n.lastscan=$ScanID " +
                //"SET n.scannerid=$ScannerID " +
                //"SET n.layout='mesh' " +
                //"RETURN n.name";
                return string.Empty;
            }
        }

        public NeoQueryData CollectData()
        {
            NeoQueryData querydata = new NeoQueryData();
            List<object> propertylist = new List<object>();

            try
            {
                // This query selects all collections
                string cmquery = "select * from SMS_AppDependenceRelation";

                // Run query
                using (IResultObject results = Connector.Instance.Connection.QueryProcessor.ExecuteQuery(cmquery))
                {
                    // Enumerate through the collection of objects returned by the query.
                    foreach (IResultObject resource in results)
                    {
                        propertylist.Add(new
                        {
                            FromApplicationCIID = ResultObjectHandler.GetString(resource, "FromApplicationCIID"),
                            ToApplicationCIID = ResultObjectHandler.GetString(resource, "ToApplicationCIID"),
                            ToDeploymentTypeCIID = ResultObjectHandler.GetString(resource, "ToDeploymentTypeCIID"),
                            FromDeploymentTypeCIID = ResultObjectHandler.GetString(resource, "FromDeploymentTypeCIID")
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
    }
}
