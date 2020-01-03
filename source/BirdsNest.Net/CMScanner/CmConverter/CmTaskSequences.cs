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
    public class CmTaskSequences: ICmCollector
    {
        public string ProgressMessage { get { return "Creating task sequence nodes: "; } }
        public string Query
        {
            get
            {
                return "UNWIND $Properties AS prop " +
                "MERGE (n:" + Types.CMTaskSequence + " {id:prop.ID}) " +
                "SET n:" + Types.CMConfigurationItem + " " +
                "SET n.name = prop.Name " +
                "SET n.layout='mesh' " +
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
    }
}
