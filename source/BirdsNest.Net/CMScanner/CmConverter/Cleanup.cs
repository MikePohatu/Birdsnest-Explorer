using common;
using Neo4j.Driver.V1;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMScanner.CmConverter
{
    public static class Cleanup
    {
        public static int CleanupCmObjects(IDriver driver, string scanid, string scannerid)
        {
            int count= 0;
            IResultSummary summary;
            NeoQueryData collectionsdata = new NeoQueryData();
            collectionsdata.ScanID = scanid;
            collectionsdata.ScannerID = scannerid;

            //nodes first
            string query = "MATCH (n:" + Types.CMConfigurationItem + ") " +
                "WHERE n.lastscan<>$ScanID " +
                "DETACH DELETE n " +
                "RETURN n";

            summary = NeoWriter.RunQuery(query, collectionsdata, driver.Session());
            count = count + summary.Counters.NodesDeleted;

            //any remaining edges
            query = "MATCH ()-[r:" + Types.CMHasObject + "]->() " +
                "WHERE r.lastscan<>$ScanID " +
                "DELETE r " +
                "RETURN r";

            summary = NeoWriter.RunQuery(query, collectionsdata, driver.Session());
            count = count + summary.Counters.RelationshipsDeleted;

            query = "MATCH ()-[r:" + Types.CMLimitingCollection + "]->() " +
                "WHERE r.lastscan<>$ScanID " +
                "DELETE r " +
                "RETURN r";

            summary = NeoWriter.RunQuery(query, collectionsdata, driver.Session());
            count = count + summary.Counters.RelationshipsDeleted;

            return count;
        }
    }
}
