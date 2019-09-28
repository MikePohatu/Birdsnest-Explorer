using System.Collections.Generic;
using System.Text;
using Neo4j.Driver.V1;
using common;
using Microsoft.UpdateServices.Administration;

namespace WUScanner.Neo4j
{
    public static class Writer
    {
        public static int MergeUpdates(IEnumerable<object> updates, IDriver driver, string scanid)
        {
            Dictionary<string, object> scanprops = new Dictionary<string, object>();
            scanprops.Add("scanid", scanid);
            scanprops.Add("updates", updates);

            string query = "UNWIND $updates as update " +
                "MERGE (n:" + Types.WUUpdate + " { id: update.ID }) " +
                "SET n.IsDeclined = update.IsDeclined " +
                "SET n.IsSuperseded = update.IsSuperseded " +
                "SET n.Title = update.Title " +
                "SET n.UpdateType = update.UpdateType " +
                "SET n.Description = update.Description " +
                "SET n.IsDeclined = update.IsDeclined " +
                "SET n.AdditionalInformation = update.AdditionalInformationUrls " +
                "SET n.CreationDate = update.CreationDate " +
                "SET n.lastscan = $scanid " +
                "RETURN n ";

            using (ISession session = driver.Session())
            {
                IStatementResult result = session.WriteTransaction(tx => tx.Run(query, scanprops));
                return result.Summary.Counters.NodesCreated;
            }
        }

        public static int MergeSupersedence(IEnumerable<object> mappings, IDriver driver, string scanid)
        {
            Dictionary<string, object> scanprops = new Dictionary<string, object>();
            scanprops.Add("scanid", scanid);
            scanprops.Add("mappings", mappings);

            //KeyValuePair<string, string> test;

            string query = "UNWIND $mappings as mapping " +
                "MATCH (new:" + Types.WUUpdate + " { id: mapping.relatedid }) " +
                "MATCH (old:" + Types.WUUpdate + " { id: mapping.updateid }) " +
                "WITH new, old, mapping " +
                "MERGE p=(new)-[r:" + Types.Supersedes + "]->(old) " +
                "SET r.lastscan = $scanid " +
                "RETURN p ";

            using (ISession session = driver.Session())
            {
                IStatementResult result = session.WriteTransaction(tx => tx.Run(query, scanprops));
                return result.Summary.Counters.RelationshipsCreated;
            }
        }

        public static void UpdateMetadata(IDriver driver)
        {
            List<string> types = new List<string>() { Types.ADObject, Types.Computer, Types.User, Types.Group };

            foreach (string type in types)
            {
                string query =
                "MATCH (n:" + type + ") " +
                "WITH DISTINCT keys(n) as props " +
                "UNWIND props as p " +
                "WITH DISTINCT p as disprops " +
                "WITH collect(disprops) as allprops " +
                "MERGE(i: _Metadata { name: 'NodeProperties'}) " +
                "SET i." + type + " = allprops " +
                "RETURN i";
                using (ISession session = driver.Session())
                {
                    session.WriteTransaction(tx => tx.Run(query));
                }
            }
        }
    }
}
