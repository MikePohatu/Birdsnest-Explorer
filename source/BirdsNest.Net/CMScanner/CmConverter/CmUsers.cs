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
    public class CmUsers: ICmCollector
    {
        public string ProgressMessage { get { return "Creating user nodes: "; } }
        public string Query
        {
            get
            {
                return "UNWIND $Properties AS prop " +
                "MERGE (n:" + Types.CMUser + " {id:prop.ID}) " +
                "SET n:" + Types.CMConfigurationItem + " " +
                "SET n.name = prop.Name " +
                "SET n.sid = prop.SID " +
                "SET n.dn = prop.DN " +
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
                string cmquery = "select * from SMS_R_User";

                // Run query
                using (IResultObject results = Connector.Instance.Connection.QueryProcessor.ExecuteQuery(cmquery))
                {
                    // Enumerate through the collection of objects returned by the query.
                    foreach (IResultObject resource in results)
                    {
                        propertylist.Add(new
                        {
                            ID = ResultObjectHandler.GetString(resource, "ResourceId"),
                            SID = ResultObjectHandler.GetString(resource, "SID"),
                            DN = ResultObjectHandler.GetString(resource, "DistinguishedName"),
                            //dev.OU = ResultObjectHandler.GetString(resource, "SystemOUName"),
                            Name = ResultObjectHandler.GetString(resource, "Name"),
                            //dev.CollectionIDs.Add(resource["CollectionID"].StringValue)
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
