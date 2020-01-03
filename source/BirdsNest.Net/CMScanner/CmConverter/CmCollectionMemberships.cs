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
    public class CmCollectionMemberships: ICmCollector
    {
        public string ProgressMessage { get { return "Creating collection membership relationships: "; } }
        public string Query
        {
            get
            {
                return "UNWIND $Properties AS prop " +
                "MATCH (n:" + Types.CMConfigurationItem + " {id: prop.resourceid}) " +
                "MATCH (c:" + Types.CMCollection + " {id: prop.collectionid}) " +
                "MERGE p=(n)-[r:" + Types.CMMemberOf + "]->(c) " +
                "SET r.lastscan=$ScanID " +
                "SET r.scannerid=$ScannerID " +
                "RETURN p";
            }
        }

        public NeoQueryData CollectData()
        {
            NeoQueryData querydata = new NeoQueryData();
            List<object> propertylist = new List<object>();

            try
            {
                // This query selects all collections
                string cmquery = "select CollectionID,ResourceID from SMS_FullCollectionMembership";
                List<object> retlist = new List<object>();

                // Run query
                using (IResultObject results = Connector.Instance.Connection.QueryProcessor.ExecuteQuery(cmquery))
                {
                    // Enumerate through the collection of objects returned by the query.
                    foreach (IResultObject resource in results)
                    {
                        string colids = ResultObjectHandler.GetString(resource, "CollectionID"); 
                        string resourceid = ResultObjectHandler.GetString(resource, "ResourceID");

                        //split the collection list and 
                        foreach (string collectionid in colids.Split(','))
                        {
                            propertylist.Add(new { resourceid, collectionid });
                        }


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
