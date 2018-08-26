using common;
using Neo4j.Driver.V1;
using System.Collections.Generic;
using CMScanner.Sccm;

namespace CMScanner.Neo4j
{
    public static class Writer
    {
        public static void SetGroupScope(ISession session)
        {
            string query = "MATCH (o) " +
                "WHERE o:" + Types.User + " OR o:" + Types.Computer + " " +
                "MATCH (o)-[:AD_MEMBER_OF *]->(g:" + Types.Group + ") " +
                "WITH collect(DISTINCT o) as nodes, g " +
                "SET g.scope = size(nodes) " +
                "RETURN g";

            session.WriteTransaction(tx => tx.Run(query));
        }

        public static int MergeCollections(List<SccmCollection> collections, ISession session)
        {
            List<object> propertylist = new List<object>();

            foreach (SccmCollection c in collections)
            {
                propertylist.Add(new {
                    ID = c.ID,
                    LimitingCollectionID = c.LimitingCollectionID,
                    Name = c.Name,
                    Comment = c.Comment
                });
            }

            string query = "UNWIND $propertylist AS prop " +
                "MERGE (n:" + Types.CMCollection + "{id:prop.ID}) " +
                "SET n.limitingcollection = prop.LimitingCollectionID " +
                "SET n.comment = prop.Comment " +
                "SET n.name = prop.Name " +
                "SET n.type = prop.CollectionType " +
                "RETURN n.name";

            var result = session.WriteTransaction(tx => tx.Run(query, new { propertylist = propertylist }));
            return result.Summary.Counters.NodesCreated;
        }

        private static void SetScanId(IBirdsNestNode node, string scanid)
        {
            object lastscancurrent;
            if (node.Properties.TryGetValue("scanid", out lastscancurrent))
            { node.Properties["scanid"] = scanid; }
            else { node.Properties.Add("scanid", scanid); }
        }
    }
}
