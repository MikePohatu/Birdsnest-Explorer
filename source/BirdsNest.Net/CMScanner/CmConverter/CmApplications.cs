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
    public class CmApplications: ICmCollector
    {
        public string ProgressMessage { get { return "Creating application nodes: "; } }
        public string Query
        {
            get
            {
                return "UNWIND $Properties AS prop " +
                "MERGE (n:" + Types.CMApplication + " {id:prop.ID}) " +
                "SET n:" + Types.CMConfigurationItem + " " +
                "SET n.name = prop.Name " +
                "SET n.lastscan=$ScanID " +
                "SET n.scannerid=$ScannerID " +
                "SET n.layout='mesh' " +
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
                string cmquery = "select * from SMS_Application WHERE IsLatest='TRUE'";

                // Run query
                using (IResultObject results = Connector.Instance.Connection.QueryProcessor.ExecuteQuery(cmquery))
                {
                    // Enumerate through the collection of objects returned by the query.
                    foreach (IResultObject resource in results)
                    {
                        propertylist.Add(new
                        {
                            IsDeployed = ResultObjectHandler.GetBool(resource, "IsDeployed"),
                            IsEnabled = ResultObjectHandler.GetBool(resource, "IsEnabled"),
                            IsSuperseded = ResultObjectHandler.GetBool(resource, "IsSuperseded"),
                            IsSuperseding = ResultObjectHandler.GetBool(resource, "IsSuperseding"),
                            IsLatest = ResultObjectHandler.GetBool(resource, "IsLatest"),
                            ID = ResultObjectHandler.GetString(resource, "CI_ID"),
                            Name = ResultObjectHandler.GetString(resource, "LocalizedDisplayName")
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
    }
}
