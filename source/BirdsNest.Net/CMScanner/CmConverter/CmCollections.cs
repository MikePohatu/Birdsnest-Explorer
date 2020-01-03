using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.ConfigurationManagement.ManagementProvider;
using Microsoft.ConfigurationManagement.ManagementProvider.WqlQueryEngine;
using common;
using Neo4j.Driver.V1;

namespace CMScanner.CmConverter
{
    public class CmCollections: ICmCollector
    {
        public string ProgressMessage { get { return "Creating collection nodes: "; } }
        public string Query
        {
            get
            {
                return "UNWIND $Properties AS prop " +
                    "MERGE (n:" + Types.CMCollection + "{id:prop.ID}) " +
                    "SET n.limitingcollection = prop.LimitingCollectionID " +
                    "SET n.comment = prop.Comment " +
                    "SET n.name = prop.Name " +
                    "SET n.type = prop.CollectionType " +
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
                string cmquery = "select * from SMS_Collection";

                // Run query
                using (IResultObject results = Connector.Instance.Connection.QueryProcessor.ExecuteQuery(cmquery))
                {
                    // Enumerate through the collection of objects returned by the query.
                    foreach (IResultObject resource in results)
                    {
                        int typeint = ResultObjectHandler.GetInt(resource, "CollectionType");

                        propertylist.Add(new
                        {
                            ID = ResultObjectHandler.GetString(resource, "CollectionID"),
                            Name = ResultObjectHandler.GetString(resource, "Name"),
                            LimitingCollectionID = ResultObjectHandler.GetString(resource, "LimitToCollectionID"),
                            Comment = ResultObjectHandler.GetString(resource, "Comment"),
                            IncludeExcludeCollectionCount = ResultObjectHandler.GetInt(resource, "IncludeExcludeCollectionsCount"),                           
                            CollectionType = ((CollectionType)typeint).ToString()
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
