using System.Collections.Generic;
using System.Text;
using Neo4j.Driver.V1;
using ADScanner.ActiveDirectory;
using common;

namespace ADScanner.Neo4j
{
    public static class Writer
    {
        public static void SetGroupScope(IDriver driver)
        {
            string query = "MATCH (o) "+
                "WHERE o:"+Types.User+ " OR o:" + Types.Computer + " " + 
                "MATCH (o)-[:AD_MEMBER_OF *]->(g:" +Types.Group+ ") "+
                "WITH collect(DISTINCT o) as nodes, g " +
                "SET g.scope = size(nodes) " +
                "RETURN g";

            using (ISession session = driver.Session())
            {
                session.WriteTransaction(tx => tx.Run(query));
            }
        }

        public static int MergeADGroups(List<Dictionary<string, object>> propertylist, IDriver driver)
        {
            string query = "UNWIND $propertylist AS g " +
                "MERGE (n:" + Types.Group + "{id:g.id}) " +
                "SET n: " + Types.ADObject + " " +
                "SET n.info = g.info " +
                "SET n.description = g.description " +
                "SET n.name = g.name " +
                "SET n.type = g.type " +
                "SET n.dn = g.dn " +
                "SET n.samaccountname = g.samaccountname " +
                "SET n.lastscan = g.scanid " +
                "SET n.path = g.path " +
                "SET n.rid = g.rid " +
                "SET n.grouptype = g.grouptype " +
                "SET n.layout = 'mesh' " +
                "RETURN n.name";

            using (ISession session = driver.Session())
            {
                var result = session.WriteTransaction(tx => tx.Run(query, new { propertylist = propertylist }));
                return result.Summary.Counters.NodesCreated;
            }
        }

        public static int UpdateMemberCounts(IDriver driver)
        {
            string query = "MATCH ()-[r:" + Types.MemberOf + "]-(n:"+ Types.Group + ") " +
                "WITH n,count(r) AS i " +
                "SET n.member_count = i " +
                "RETURN n";
            using (ISession session = driver.Session())
            {
                var result = session.WriteTransaction(tx => tx.Run(query));
                return result.Summary.Counters.PropertiesSet;
            }
        }

        public static int MergeAdUsers(List<Dictionary<string, object>> propertylist, IDriver driver)
        {
            string query = "UNWIND $propertylist AS u " +
            "MERGE (n:" + Types.User + "{id:u.id}) " +
            "SET n: " + Types.ADObject + " " +
            "SET n.info = u.info " +
            "SET n.description = u.description " +
            "SET n.name = u.name " +
            "SET n.dn = u.dn " +
            "SET n.path = u.path " +
            "SET n.type = u.type " +
            "SET n.samaccountname = u.samaccountname " +
            "SET n.lastscan = u.scanid " +
            "SET n.primarygroupid = u.primarygroupid " +
            "SET n.displayname = u.displayname " +
            "SET n.state = u.state " +
            "SET n.layout = 'mesh' " +
            "RETURN n.name";

            using (ISession session = driver.Session())
            {
                var result = session.WriteTransaction(tx => tx.Run(query, new { propertylist = propertylist }));
                return result.Summary.Counters.NodesCreated;
            }
        }

        public static int MergeAdComputers(List<Dictionary<string, object>> propertylist, IDriver driver)
        {
            string query = "UNWIND $propertylist AS c " +
            "MERGE (n:" + Types.Computer + "{id:c.id}) " +
            "SET n: " + Types.Device + " " +
            "SET n: " + Types.ADObject + " " +
            "SET n.info = c.info " +
            "SET n.description = c.description " +
            "SET n.name = c.name " +
            "SET n.path = c.path " +
            "SET n.dn = c.dn " +
            "SET n.type = c.type " +
            "SET n.samaccountname = c.samaccountname " +
            "SET n.lastscan = c.scanid " +
            "SET n.primarygroupid = c.primarygroupid " +
            "SET n.operatingsystem = c.operatingsystem " +
            "SET n.operatingsystemversion = c.operatingsystemversion " +
            "SET n.state = c.state " +
            "SET n.layout = 'mesh' " +
            "RETURN n.name";

            using (ISession session = driver.Session())
            {
                var result = session.WriteTransaction(tx => tx.Run(query, new { propertylist = propertylist }));
                return result.Summary.Counters.NodesCreated;
            }
        }

        public static object GetGroupRelationshipObject(string membertype, string memberid, string groupdn, string scanid)
        {
            return new { membertype = membertype, id = memberid, dn = groupdn, scanid = scanid };
        }

        public static int MergeGroupRelationships(string type, List<object> groupmappings, IDriver driver)
        {
            
            string query = "UNWIND $groupmappings as m " +
            "MERGE (g:" + Types.Group + "{dn:m.dn}) " +
            "MERGE (n:" + type + "{id:m.id}) " +
            "MERGE (n)-[r:" + Types.MemberOf + "]->(g) " +
            "SET r.lastscan = m.scanid " +
            "RETURN n.name,g.dn";

            using (ISession session = driver.Session())
            {
                var result = session.WriteTransaction(tx => tx.Run(query, new { groupmappings = groupmappings }));
                return result.Summary.Counters.RelationshipsCreated;
            }
        }

        public static void MergeNodeOnID(IBirdsNestNode node, IDriver driver, string scanid)
        {
            SetScanId(node, scanid);

            string query = "MERGE (n:" + node.Type + " {id:$id}) " +
            "SET n.name = $name " +
            "SET n.path = $path " +
            "SET n.lastscan = $scanid " +
            "RETURN n";

            using (ISession session = driver.Session())
            {
                session.WriteTransaction(tx => tx.Run(query, node.Properties));
            }
        }

        public static int CreatePrimaryGroupRelationships(IDriver driver, string scanid)
        {
            Dictionary<string, object> properties = new Dictionary<string, object>();
            properties.Add("scanid", scanid);

            string query = "MATCH(n) " +
            "WHERE n:" + Types.Computer + " OR n:" + Types.User + " " +
            "WITH n " +
            "MATCH (g:" + Types.Group + ") WHERE g.rid = n.primarygroupid " +
            "MERGE(n)-[r:" + Types.MemberOf + "]->(g) " +
            "SET r.primarygroup = true " +
            "SET r.lastscan = $scanid " +
            "RETURN n.name,g.name ";

            using (ISession session = driver.Session())
            {
                IStatementResult result = session.WriteTransaction(tx => tx.Run(query, properties));
                return result.Summary.Counters.RelationshipsCreated;
            }
        }

        public static int RemoveDeletedGroupMemberShips(IDriver driver, string scanid)
        {
            string query = "MATCH(n:" + Types.User + ") " +
                "WHERE n.lastscan = $scanid " +
                "WITH n " +
                "MATCH(n) -[r:" + Types.MemberOf + "]->(g:" + Types.Group + ") " +
                "WHERE NOT EXISTS(r.lastscan) OR r.lastscan <> $scanid " +
                "DELETE r " +
                "RETURN r ";

            using (ISession session = driver.Session())
            {
                IStatementResult result = session.WriteTransaction(tx => tx.Run(query, new { scanid = scanid }));
                return result.Summary.Counters.RelationshipsDeleted;
            }
        }

        public static int FindAndMarkDeletedItems(string label, IDriver driver, string scanid)
        {
            string query = "MATCH(n:" + label + ") " +
                "WHERE n.lastscan <> $scanid " +
                "SET n:" + Types.Deleted + " " +
                "REMOVE n:" + label + " " +
                "SET n.type='" + label + "' " +
                //"WITH n " +
                //"MATCH (n)-[r]->() " +
                //"DELETE r " +
                "RETURN n ";

            using (ISession session = driver.Session())
            {
                IStatementResult result = session.WriteTransaction(tx => tx.Run(query, new { scanid = scanid }));
                return result.Summary.Counters.RelationshipsDeleted;
            }
        }

        public static void UpdateMetadata(IDriver driver)
        {
            List<string> types = new List<string>() { Types.ADObject, Types.Computer, Types.User, Types.Group};

            foreach (string type in types)
            {
                string query = "MATCH (n:" + type + ") " +
                    "WITH DISTINCT keys(n) as props " +
                    "MERGE(i: _Metadata { name: 'NodeDetails'}) " +
                    "SET i." + type + " = props " +
                    "RETURN i";
                using (ISession session = driver.Session())
                {
                    session.WriteTransaction(tx => tx.Run(query));
                }
            }
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
