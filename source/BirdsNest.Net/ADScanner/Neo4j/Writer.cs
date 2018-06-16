using System.Collections.Generic;
using System.Text;
using Neo4j.Driver.V1;
using ADScanner.ActiveDirectory;

namespace ADScanner.Neo4j
{
    public static class Writer
    {
        //ADGroupMemberObject
        public static int MergeADGroupMemberObjectOnPath(ADGroupMemberObject node, ISession session)
        {
            StringBuilder builder = new StringBuilder();
            int i = 0;

            builder.AppendLine("MERGE (node:" + node.Label + " {path:'" + node.Path + "'})");
            builder.AppendLine("SET node.name = '" + node.Name + "'");
            builder.AppendLine("SET node.id = '" + node.ID + "'");

            if (node.Properties != null)
            {
                foreach (var prop in node.Properties)
                {
                    builder.AppendLine("SET node." + prop.Key + " = '" + prop.Value + "'");
                }
            }

            foreach (string dn in node.MemberOfDNs)
            {
                builder.AppendLine("MERGE (g" + i + ":AD_GROUP {path:'" + dn + "'})");
                builder.AppendLine("MERGE (node) -[r" + i + ": AD_MemberOf]->(g" + i + ")");
                i++;
            }

            builder.AppendLine("RETURN node");

            session.WriteTransaction(tx => tx.Run(builder.ToString()));

            return i;
        }

        public static void MergeNodeOnID(INode node, ISession session)
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("MERGE (newnode:" + node.Label + " {id:'" + node.ID + "'})");

            builder.AppendLine("SET newnode.name = '" + node.Name + "'");
            builder.AppendLine("SET newnode.path = '" + node.Path + "'");

            if (node.Properties != null)
            {
                foreach (var prop in node.Properties)
                {
                    builder.AppendLine("SET newnode." + prop.Key + " = '" + prop.Value.ToString() + "'");
                }
            }

            builder.AppendLine("RETURN newnode");

            session.WriteTransaction(tx => tx.Run(builder.ToString()));
        }

        /// <summary>
        /// Take a node, and create relationships to groups it is a member of. 
        /// Returns count of relationships created
        /// </summary>
        /// <param name="node"></param>
        /// <param name="groupDNs"></param>
        /// <param name="session"></param>
        /// <returns></returns>
        public static int AddIsMemberOfADGroups(INode node, List<string> groupDNs, ISession session)
        {
            StringBuilder builder = new StringBuilder();
            int i = 0;

            builder.AppendLine("MERGE (node:" +node.Label + " {path:'" + node.Path + "'})");
            foreach (string dn in groupDNs)
            {
                builder.AppendLine("MERGE (g" + i + ":AD_GROUP {path:'" + dn + "'})");
                builder.AppendLine("MERGE (node)-[r" + i + ":AD_MemberOf]->(g" + i + ")");
                i++;
            }
            builder.AppendLine("RETURN node");

            session.WriteTransaction(tx => tx.Run(builder.ToString()));
            return i;
        }

        /// <summary>
        /// Take a group, and create relationships from members to it 
        /// Returns count of relationships created
        /// </summary>
        /// <param name="node"></param>
        /// <param name="memberDNs"></param>
        /// <param name="session"></param>
        /// <returns></returns>
        //public static int AddMembersOfADGroup(ADGroup node, List<string> memberDNs, ISession session)
        //{
        //    StringBuilder builder = new StringBuilder();
        //    int i = 0;

        //    builder.AppendLine("MERGE (node:" + node.Label + " {path:'" + node.Path + "'})");
        //    foreach (string dn in memberDNs)
        //    {
        //        builder.AppendLine("MERGE (g" + i + ":AD_GROUP {path:'" + dn + "'})");
        //        builder.AppendLine("MERGE (g" + i + ") -[r" + i + ": AD_MemberOf]->(node)");
        //        i++;
        //    }
        //    builder.AppendLine("RETURN node");

        //    session.WriteTransaction(tx => tx.Run(builder.ToString()));
        //    return i;
        //}

        public static int CreatePrimaryGroupRelationships(ISession session)
        {
            int count = 0;
            string query = "MATCH(n: AD_COMPUTER) " +
                "WITH n " +
                "MATCH(g: AD_GROUP) WHERE n.primaryGroupID = g.rid " +
                "MERGE(n) -[r: AD_MemberOf]->(g) " +
                "SET r.primarygroup = 'true'";
            IStatementResult result = session.WriteTransaction(tx => tx.Run(query));
            count = result.Keys.Count;

            query = "MATCH(n: AD_USER) " +
                "WITH n " +
                "MATCH(g: AD_GROUP) WHERE n.primaryGroupID = g.rid " +
                "MERGE(n) -[r: AD_MemberOf]->(g) " +
                "SET r.primarygroup = 'true'";
            result = session.WriteTransaction(tx => tx.Run(query));
            count = count + result.Keys.Count;

            return count;
        }
    }
}
