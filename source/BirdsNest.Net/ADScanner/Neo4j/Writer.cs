using System.Collections.Generic;
using System.Text;
using Neo4j.Driver.V1;
using ADScanner.ActiveDirectory;

namespace ADScanner.Neo4j
{
    public static class Writer
    {
        //ADGroupMemberObject
        public static int MergeADGroupMemberObjectOnPath(ADGroupMemberObject node, ISession session, string scanid)
        {
            StringBuilder builder = new StringBuilder();            
            int i = 0;

            Dictionary<string, object> properties = GetBaseProperties(node, scanid);

            builder.AppendLine("MERGE (n:" + node.Label + "{path:$nodepath})");
            builder.AppendLine("SET n.name = $nodename");
            builder.AppendLine("SET n.id = $nodeid");
            builder.AppendLine("SET n.lastscan = $scanid");

            if (node.Properties != null)
            {
                foreach (var prop in node.Properties)
                {
                    builder.AppendLine("SET n." + prop.Key + " = $propval" + i);
                    properties.Add("propval" + i,prop.Value);
                    i++;
                }
            }

            foreach (string dn in node.MemberOfDNs)
            {
                properties.Add("dn" + i, dn);
                builder.AppendLine("MERGE (g" + i + ":AD_GROUP {path:$dn" + i + "})");
                builder.AppendLine("MERGE (n) -[r" + i + ": AD_MemberOf]->(g" + i + ")");
                builder.AppendLine("SET r" + i + ".lastscan = $scanid");
                i++;
            }

            builder.AppendLine("RETURN n");

            var result = session.WriteTransaction(tx => tx.Run(builder.ToString(),properties));
            return result.Summary.Counters.RelationshipsCreated;
        }

        public static void MergeNodeOnID(INode node, ISession session, string scanid)
        {
            int i = 0;

            Dictionary<string, object> properties = GetBaseProperties(node, scanid);

            StringBuilder builder = new StringBuilder();
            builder.AppendLine("MERGE (n:" + node.Label + " {id:$nodeid})");

            builder.AppendLine("SET n.name = $nodename");
            builder.AppendLine("SET n.path = $nodepath");
            builder.AppendLine("SET n.lastscan = $scanid");

            if (node.Properties != null)
            {
                foreach (var prop in node.Properties)
                {
                    builder.AppendLine("SET n." + prop.Key + " = $propval" + i);
                    properties.Add("propval" + i, prop.Value);
                    i++;
                }
            }

            builder.AppendLine("RETURN n");

            session.WriteTransaction(tx => tx.Run(builder.ToString(),properties));
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
            int i = 0;

            Dictionary<string, object> properties = GetBaseProperties(node, scanid);

            StringBuilder builder = new StringBuilder();

            builder.AppendLine("MERGE (n:" + node.Label + " {path:$nodepath})");
            builder.AppendLine("SET n.lastscan = $scanid");

            foreach (string dn in groupDNs)
            {
                properties.Add("dn" + i, dn);
                builder.AppendLine("MERGE (g" + i + ":AD_GROUP {path:$dn" + i + "})");
                builder.AppendLine("MERGE (n)-[r" + i + ":AD_MemberOf]->(g" + i + ")");
                builder.AppendLine("SET r" + i + ".lastscan = $scanid");
                i++;
            }
            builder.AppendLine("RETURN n");

            var result = session.WriteTransaction(tx => tx.Run(builder.ToString(), properties));
            return result.Summary.Counters.RelationshipsCreated;
        }

        public static int CreatePrimaryGroupRelationships(ISession session, string scanid)
        {
            Dictionary<string, object> properties = new Dictionary<string, object>();
            properties.Add("scanid", scanid);

            StringBuilder builder = new StringBuilder();
            builder.AppendLine("MATCH(n)");
            builder.AppendLine("WHERE n:" + Labels.Computer + " OR n:" + Labels.User);
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

        private static Dictionary<string,object> GetBaseProperties(INode node, string scanid)
        {
            Dictionary<string,object> properties = new Dictionary<string, object>();
            properties.Add("nodepath", node.Path);
            properties.Add("nodename", node.Name);
            properties.Add("nodeid", node.ID);
            properties.Add("scanid", scanid);
            return properties;
        }
    }
}
