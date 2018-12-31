using System.Collections.Generic;
using System.Text;
using Neo4j.Driver.V1;
using ADScanner.ActiveDirectory;
using common;

namespace ADScanner.Neo4j
{
    public static class Writer
    {
        public static string DomainID { get; set; }

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
            Dictionary<string, object> scanprops = new Dictionary<string, object>();
            scanprops.Add("domainid", Writer.DomainID);

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
                "SET n.domainid = $scanprops.domainid " +
                "SET n.path = g.path " +
                "SET n.rid = g.rid " +
                "SET n.grouptype = g.grouptype " +
                "SET n.layout = 'mesh' " +
                "RETURN n.name";

            using (ISession session = driver.Session())
            {
                var result = session.WriteTransaction(tx => tx.Run(query, new { propertylist, scanprops }));
                return result.Summary.Counters.NodesCreated;
            }
        }

        public static int UpdateMemberCounts(IDriver driver)
        {
            string query = "MATCH (n:" + Types.Group + ") " +
                "SET n.member_count = 0 " +
                "WITH n " +
                "MATCH ()-[r:" + Types.MemberOf + "]->(n) " +
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
            Dictionary<string, object> scanprops = new Dictionary<string, object>();
            scanprops.Add("domainid", Writer.DomainID);

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
            "SET n.domainid = $scanprops.domainid " +
            "SET n.primarygroupid = u.primarygroupid " +
            "SET n.displayname = u.displayname " +
            "SET n.state = u.state " +
            "SET n.layout = 'mesh' " +
            "RETURN n.name";

            using (ISession session = driver.Session())
            {
                var result = session.WriteTransaction(tx => tx.Run(query, new { propertylist, scanprops }));
                return result.Summary.Counters.NodesCreated;
            }
        }

        public static int MergeAdComputers(List<Dictionary<string, object>> propertylist, IDriver driver)
        {
            Dictionary<string, object> scanprops = new Dictionary<string, object>();
            scanprops.Add("domainid", Writer.DomainID);

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
            "SET n.domainid = $scanprops.domainid " +
            "SET n.primarygroupid = c.primarygroupid " +
            "SET n.operatingsystem = c.operatingsystem " +
            "SET n.operatingsystemversion = c.operatingsystemversion " +
            "SET n.state = c.state " +
            "SET n.layout = 'mesh' " +
            "RETURN n.name";

            using (ISession session = driver.Session())
            {
                var result = session.WriteTransaction(tx => tx.Run(query, new { propertylist, scanprops }));
                return result.Summary.Counters.NodesCreated;
            }
        }

        public static object GetGroupRelationshipObject(string membertype, string memberid, string groupdn, string scanid)
        {
            return new { membertype = membertype, id = memberid, dn = groupdn, scanid = scanid };
        }

        public static int MergeGroupRelationships(string type, List<object> groupmappings, IDriver driver)
        {
            Dictionary<string, object> scanprops = new Dictionary<string, object>();
            scanprops.Add("domainid", Writer.DomainID);

            string query = "UNWIND $groupmappings as m " +
            "MERGE (g:" + Types.Group + "{dn:m.dn}) " +
            "MERGE (n:" + type + "{id:m.id}) " +
            "MERGE (n)-[r:" + Types.MemberOf + "]->(g) " +
            "SET r.lastscan = m.scanid " +
            "SET r.domainid = $scanprops.domainid " +
            "RETURN n.name,g.dn";

            using (ISession session = driver.Session())
            {
                var result = session.WriteTransaction(tx => tx.Run(query, new { groupmappings, scanprops }));
                return result.Summary.Counters.RelationshipsCreated;
            }
        }

        public static int CreatePrimaryGroupRelationships(IDriver driver, string scanid)
        {
            Dictionary<string, object> scanprops = new Dictionary<string, object>();
            scanprops.Add("scanid", scanid);
            scanprops.Add("domainid", Writer.DomainID);

            string query = "MATCH(n) " +
            "WHERE n:" + Types.Computer + " OR n:" + Types.User + " " +
            "WITH n " +
            "MATCH (g:" + Types.Group + ") WHERE g.rid = n.primarygroupid " +
            "MERGE(n)-[r:" + Types.MemberOf + "]->(g) " +
            "SET r.primarygroup = true " +
            "SET r.lastscan = $scanid " +
            "SET r.domainid = $domainid " +
            "RETURN n.name,g.name ";

            using (ISession session = driver.Session())
            {
                IStatementResult result = session.WriteTransaction(tx => tx.Run(query, scanprops));
                return result.Summary.Counters.RelationshipsCreated;
            }
        }

        public static int RemoveDeletedGroupMemberShips(IDriver driver, string scanid)
        {
            Dictionary<string, object> scanprops = new Dictionary<string, object>();
            scanprops.Add("scanid", scanid);
            scanprops.Add("domainid", Writer.DomainID);

            string query = "MATCH(n:" + Types.ADObject + ") " +
                "WHERE n.domainid = $domainid " +
                "WITH n " +
                "MATCH(n) -[r:" + Types.MemberOf + " {domainid:$domainid}]->(g:" + Types.Group + ") " +
                "WHERE NOT EXISTS(r.lastscan) OR r.lastscan <> $scanid " +
                "DELETE r " +
                "RETURN r ";

            using (ISession session = driver.Session())
            {
                IStatementResult result = session.WriteTransaction(tx => tx.Run(query, scanprops));
                return result.Summary.Counters.RelationshipsDeleted;
            }
        }

        public static int FindAndMarkDeletedItems(string label, IDriver driver, string scanid)
        {
            Dictionary<string, object> scanprops = new Dictionary<string, object>();
            scanprops.Add("domainid", Writer.DomainID);
            scanprops.Add("scanid", scanid);

            string query = "MATCH(n:" + label + ") " +
                "WHERE n.lastscan <> $scanid AND n.domainid = $domainid " +
                "SET n:" + Types.Deleted + " " +
                "REMOVE n:" + label + " " +
                "SET n.type='" + label + "' " +
                "RETURN n ";

            using (ISession session = driver.Session())
            {
                IStatementResult result = session.WriteTransaction(tx => tx.Run(query, scanprops));
                return result.Summary.Counters.RelationshipsDeleted;
            }
        }

        public static void UpdateMetadata(IDriver driver)
        {
            List<string> types = new List<string>() { Types.ADObject, Types.Computer, Types.User, Types.Group};

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
