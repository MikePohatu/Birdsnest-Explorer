using System.Collections.Generic;
using System.Text;
using Neo4j.Driver.V1;
using ADScanner.ActiveDirectory;

namespace ADScanner.Neo4j
{
    public static class Writer
    {
        public static int MergeADGroup(ADGroup node, ISession session, string scanid)
        {
            SetScanId(node, scanid);

            string query = "MERGE (n:" + node.Type + "{id:$id}) " +
            "SET n.name = $name " +
            "SET n.id = $id " +
            "SET n.lastscan = $scanid " +
            "SET n.rid = $rid " +
            "SET n.grouptype = $grouptype " +
            "RETURN n.name";

            var result = session.WriteTransaction(tx => tx.Run(query, node.Properties));
            return result.Summary.Counters.NodesCreated;
        }

        public static int MergeAdUser(ADUser node, ISession session, string scanid)
        {
            SetScanId(node, scanid);

            string query = "MERGE (n:" + node.Type + "{id:$id}) " +
            "SET n.name = $name " +
            "SET n.id = $id " +
            "SET n.lastscan = $scanid " +
            "SET n.displayname = $displayname " +
            "SET n.state = $state " +
            "RETURN n.name";

            var result = session.WriteTransaction(tx => tx.Run(query, node.Properties));
            return result.Summary.Counters.NodesCreated;
        }

        public static int MergeAdComputer(ADComputer node, ISession session, string scanid)
        {
            SetScanId(node, scanid);

            string query = "MERGE (n:" + node.Type + "{id:$id}) " +
            "SET n.name = $name " +
            "SET n.id = $id " +
            "SET n.lastscan = $scanid " +
            "SET n.operatingsystem = $operatingsystem " +
            "SET n.operatingsystemversion = $operatingsystemversion " +
            "SET n.state = $state " +
            "RETURN n.name";

            var result = session.WriteTransaction(tx => tx.Run(query, node.Properties));
            return result.Summary.Counters.NodesCreated;
        }

        public static int MergeGroupRelationships(ISession session, string scanid)
        { 

            string query = "MERGE (g:" + Types.Group + " {path:$dn}) " +
                "WITH g " +
                "MATCH (n) WHERE n.id=$id " +
                "MERGE (n)-[r:" + Types.MemberOf + "]->(g) " +
                "SET r.lastscan = $scanid " +
                "RETURN n.name";


            //foreach (string dn in node.MemberOfDNs)
            //{
            //    result = session.WriteTransaction(tx => tx.Run(relquery,new { dn = dn, scanid = scanid, nodeid = node.ID }));
            //    relcount = relcount + result.Summary.Counters.RelationshipsCreated;
            //}
            var result = session.WriteTransaction(tx => tx.Run(query, null));
            return result.Summary.Counters.RelationshipsCreated;
        }

        public static void MergeNodeOnID(INode node, ISession session, string scanid)
        {
            SetScanId(node, scanid);

            StringBuilder builder = new StringBuilder();
            builder.AppendLine("MERGE (n:" + node.Type + " {id:$id})");

            builder.AppendLine("SET n.name = $name");
            builder.AppendLine("SET n.path = $path");
            builder.AppendLine("SET n.lastscan = $scanid");

            builder.AppendLine("RETURN n");

            session.WriteTransaction(tx => tx.Run(builder.ToString(),node.Properties));
        }

        /// <summary>
        /// Take a node, and create relationships to groups it is a member of. 
        /// Returns count of relationships created
        /// </summary>
        /// <param name="node"></param>
        /// <param name="groupDNs"></param>
        /// <param name="session"></param>
        /// <returns></returns>
        public static int AddIsMemberOfADGroups(INode node, List<string> groupDNs, ISession session, string scanid)
        {
            //int i = 0;
            SetScanId(node, scanid);

            StringBuilder builder = new StringBuilder();

            builder.AppendLine("MERGE (n:" + node.Type + " {path:$path})");
            builder.AppendLine("SET n.lastscan = $scanid");

            //foreach (string dn in groupDNs)
            //{
            //    properties.Add("dn" + i, dn);
            //    builder.AppendLine("MERGE (g:AD_GROUP {path:$dn" + i + "})");
            //    builder.AppendLine("MERGE (n)-[r:AD_MemberOf]->(g)");
            //    builder.AppendLine("SET r.lastscan = $scanid");
            //    i++;
            //}
            builder.AppendLine("RETURN n");

            var result = session.WriteTransaction(tx => tx.Run(builder.ToString(), node.Properties));
            return result.Summary.Counters.RelationshipsCreated;
        }

        public static int CreatePrimaryGroupRelationships(ISession session, string scanid)
        {
            Dictionary<string, object> properties = new Dictionary<string, object>();
            properties.Add("scanid", scanid);

            StringBuilder builder = new StringBuilder();
            builder.AppendLine("MATCH(n)");
            builder.AppendLine("WHERE n:" + Types.Computer + " OR n:" + Types.User);
            builder.AppendLine("WITH n");
            builder.AppendLine("MATCH (g: AD_GROUP) WHERE g.rid = n.primaryGroupID");
            builder.AppendLine("MERGE(n)-[r: AD_MemberOf]->(g)");
            builder.AppendLine("SET r.primarygroup = 'true'");
            builder.AppendLine("SET r.lastscan = $scanid");
            builder.AppendLine("RETURN n.name,g.name");
            IStatementResult result = session.WriteTransaction(tx => tx.Run(builder.ToString(), properties));
            return result.Summary.Counters.RelationshipsCreated;
        }

        public static int RemoveDeletedGroupMemberShips(ISession session, string scanid)
        {
            Dictionary<string, object> properties = new Dictionary<string, object>();
            properties.Add("scanid", scanid);

            StringBuilder builder = new StringBuilder();
            builder.AppendLine("MATCH(n: AD_USER)");
            builder.AppendLine("WHERE n.lastscan = $scanid");
            builder.AppendLine("WITH n");
            builder.AppendLine("MATCH(n) -[r: AD_MemberOf]->(g: AD_GROUP)");
            builder.AppendLine("WHERE NOT EXISTS(r.lastscan) OR r.lastscan <> $scanid");
            builder.AppendLine("DELETE r");
            builder.AppendLine("RETURN r");
            IStatementResult result = session.WriteTransaction(tx => tx.Run(builder.ToString(), properties));
            return result.Summary.Counters.RelationshipsDeleted;
        }

        public static int FindAndMarkDeletedItems(string label, ISession session, string scanid)
        {
            Dictionary<string, object> properties = new Dictionary<string, object>();
            properties.Add("scanid", scanid);

            StringBuilder builder = new StringBuilder();
            builder.AppendLine("MATCH(n:" + label + ")");
            builder.AppendLine("WHERE n.lastscan <> $scanid");
            builder.AppendLine("SET n:AD_DELETED");
            builder.AppendLine("WITH n");
            builder.AppendLine("MATCH (n)-[r]->()");
            builder.AppendLine("DELETE r");
            builder.AppendLine("RETURN r");

            IStatementResult result = session.WriteTransaction(tx => tx.Run(builder.ToString(), properties));
            return result.Summary.Counters.RelationshipsDeleted;
        }

        private static void SetScanId(INode node, string scanid)
        {
            object lastscancurrent;
            if (node.Properties.TryGetValue("scanid", out lastscancurrent))
            { node.Properties["scanid"] = scanid; }
            else { node.Properties.Add("scanid", scanid); }
        }
    }
}
