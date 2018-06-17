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

            builder.AppendLine("MERGE (n:" + node.Label + " {path:'" + node.Path + "'})");
            builder.AppendLine("SET n.name = '" + node.Name + "'");
            builder.AppendLine("SET n.id = '" + node.ID + "'");
            builder.AppendLine("SET n.lastscan = '" + scanid + "'");

            if (node.Properties != null)
            {
                foreach (var prop in node.Properties)
                {
                    builder.AppendLine("SET n." + prop.Key + " = '" + prop.Value + "'");
                }
            }

            foreach (string dn in node.MemberOfDNs)
            {
                builder.AppendLine("MERGE (g" + i + ":AD_GROUP {path:'" + dn + "'})");
                builder.AppendLine("MERGE (n) -[r" + i + ": AD_MemberOf]->(g" + i + ")");
                builder.AppendLine("SET r" + i + ".lastscan = '" + scanid + "'");
                i++;
            }

            builder.AppendLine("RETURN n");

            var result = session.WriteTransaction(tx => tx.Run(builder.ToString()));
            return result.Summary.Counters.RelationshipsCreated;
        }

        public static void MergeNodeOnID(INode node, ISession session, string scanid)
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("MERGE (n:" + node.Label + " {id:'" + node.ID + "'})");

            builder.AppendLine("SET n.name = '" + node.Name + "'");
            builder.AppendLine("SET n.path = '" + node.Path + "'");
            builder.AppendLine("SET n.lastscan = '" + scanid + "'");

            if (node.Properties != null)
            {
                foreach (var prop in node.Properties)
                {
                    builder.AppendLine("SET n." + prop.Key + " = '" + prop.Value.ToString() + "'");
                }
            }

            builder.AppendLine("RETURN n");

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
        public static int AddIsMemberOfADGroups(INode node, List<string> groupDNs, ISession session, string scanid)
        {
            StringBuilder builder = new StringBuilder();
            int i = 0;

            builder.AppendLine("MERGE (n:" + node.Label + " {path:'" + node.Path + "'})");
            builder.AppendLine("SET n.lastscan = '" + scanid + "'");

            foreach (string dn in groupDNs)
            {
                builder.AppendLine("MERGE (g" + i + ":AD_GROUP {path:'" + dn + "'})");
                builder.AppendLine("MERGE (n)-[r" + i + ":AD_MemberOf]->(g" + i + ")");
                builder.AppendLine("SET r" + i + ".lastscan = '" + scanid + "'");
                i++;
            }
            builder.AppendLine("RETURN n");

            var result = session.WriteTransaction(tx => tx.Run(builder.ToString()));
            return result.Summary.Counters.RelationshipsCreated;
        }

        /// <summary>
        /// Take a group, and create relationships from members to it 
        /// Returns count of relationships created
        /// </summary>
        /// <param name="node"></param>
        /// <param name="memberDNs"></param>
        /// <param name="session"></param>
        /// <returns></returns>
        //public static int AddMembersOfADGroup(ADGroup node, List<string> memberDNs, ISession session, string scanid)
        //{
        //    StringBuilder builder = new StringBuilder();
        //    int i = 0;

        //    builder.AppendLine("MERGE (n:" + node.Label + " {path:'" + node.Path + "'})");
        //    foreach (string dn in memberDNs)
        //    {
        //        builder.AppendLine("MERGE (g" + i + ":AD_GROUP {path:'" + dn + "'})");
        //        builder.AppendLine("MERGE (g" + i + ") -[r" + i + ": AD_MemberOf]->(n)");
        //        builder.AppendLine("SET r.lastscan = '" + scanid + "'");
        //        i++;
        //    }
        //    builder.AppendLine("RETURN n");

        //    session.WriteTransaction(tx => tx.Run(builder.ToString()));
        //    return i;
        //}

        public static int CreatePrimaryGroupRelationships(ISession session, string scanid)
        {
            int count = 0;
            string query = "MATCH(n: AD_COMPUTER) " +
                "WITH n " +
                "MATCH (g: AD_GROUP) WHERE g.rid = n.primaryGroupID " +
                "MERGE(n) -[r: AD_MemberOf]->(g) " +
                "SET r.primarygroup = 'true' " +
                "SET r.lastscan = '" + scanid + "'";
            IStatementResult result = session.WriteTransaction(tx => tx.Run(query));
            count = result.Summary.Counters.RelationshipsCreated;

            query = "MATCH(n: AD_USER) " +
                "WITH n " +
                "MATCH (g: AD_GROUP) WHERE g.rid = n.primaryGroupID " +
                "MERGE(n) -[r: AD_MemberOf]->(g) " +
                "SET r.primarygroup = 'true'" +
                "SET r.lastscan = '" + scanid + "'";
            result = session.WriteTransaction(tx => tx.Run(query));
            count = count + result.Summary.Counters.RelationshipsCreated;

            return count;
        }

        public static int RemoveDeletedGroupMemberShips(ISession session, string scanid)
        {
            string query = "MATCH(n: AD_USER) " +
                "WHERE n.lastscan = '" + scanid + "'" +
                "WITH n " +
                "MATCH(n) -[r: AD_MemberOf]->(g: AD_GROUP) " +
                "WHERE NOT EXISTS(r.lastscan) OR r.lastscan <> '" + scanid + "' " +
                "DELETE r " +
                "RETURN r";
            IStatementResult result = session.WriteTransaction(tx => tx.Run(query));
            return result.Summary.Counters.RelationshipsDeleted;
        }

        //public static int MarkDeletedEntity(DeletedObject node, ISession session, string scanid)
        //{
        //    StringBuilder builder = new StringBuilder();
        //    builder.AppendLine("MERGE (n {id:'" + node.ID + "'})");

        //    //builder.AppendLine("SET node.name = '" + node.Name + "'");
        //    builder.AppendLine("SET n:" + node.Label);
        //    builder.AppendLine("SET n.path = '" + node.Path + "'");
        //    builder.AppendLine("SET n.lastscan = '" + scanid + "'");
        //    builder.AppendLine("MATCH (n)-[r*]->()");
        //    builder.AppendLine("DELETE r");

        //    if (node.Properties != null)
        //    {
        //        foreach (var prop in node.Properties)
        //        {
        //            builder.AppendLine("SET n." + prop.Key + " = '" + prop.Value.ToString() + "'");
        //        }
        //    }
        //    builder.AppendLine("RETURN n");

        //    IStatementResult result = session.WriteTransaction(tx => tx.Run(builder.ToString()));
        //    return result.Summary.Counters.RelationshipsDeleted;
        //}
        public static int FindAndMarkDeletedUsers(ISession session, string scanid)
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("MATCH(n:AD_USER)");
            builder.AppendLine("WHERE n.lastscan <> '" + scanid + "'");
            //builder.AppendLine("AND NOT n:AD_DELETED");
            builder.AppendLine("SET n:AD_DELETED");
            builder.AppendLine("WITH n");
            builder.AppendLine("MATCH (n)-[r]->()");
            builder.AppendLine("DELETE r");
            builder.AppendLine("RETURN r");

            IStatementResult result = session.WriteTransaction(tx => tx.Run(builder.ToString()));
            return result.Summary.Counters.RelationshipsDeleted;
        }

        public static int FindAndMarkDeletedComputers(ISession session, string scanid)
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("MATCH(n:AD_COMPUTER)");
            builder.AppendLine("WHERE n.lastscan <> '" + scanid + "'");
            //builder.AppendLine("AND NOT n:AD_DELETED");            
            builder.AppendLine("SET n:AD_DELETED");
            builder.AppendLine("WITH n");
            builder.AppendLine("MATCH (n)-[r]->()");
            builder.AppendLine("DELETE r");
            builder.AppendLine("RETURN r");

            IStatementResult result = session.WriteTransaction(tx => tx.Run(builder.ToString()));
            return result.Summary.Counters.RelationshipsDeleted;
        }
    }
}
