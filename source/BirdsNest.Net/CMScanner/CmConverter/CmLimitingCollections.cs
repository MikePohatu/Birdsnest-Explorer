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
    public class CmLimitingCollections: ICmCollector
    {
        public string ProgressMessage { get { return "Creating limiting collection connections: "; } }
        public string Query
        {
            get
            {
                return "MATCH (n:" + Types.CMCollection + ")" +
                "MATCH (l:" + Types.CMCollection + " {id:n.limitingcollection}) " +
                "MERGE (n)-[r:" + Types.CMLimitingCollection + "]->(l) " +
                "SET r.lastscan=$ScanID " +
                "SET r.scannerid=$ScannerID " +
                "RETURN n.name";
            }
        }

        public NeoQueryData CollectData()
        {
            return new NeoQueryData();
        }

        public string GetSummaryString(IResultSummary summary)
        {
            return summary.Counters.RelationshipsCreated + " connected";
        }
    }
}
