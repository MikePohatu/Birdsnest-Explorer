using System.Collections.Generic;
using System.Linq;
using System.Text;
using Neo4j.Driver.V1;

namespace neo4jlink
{
    public static class Writer
    {
        public static void MergeNodeOnID(INode node, IDriver driver)
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("MERGE (newnode:" + node.Label + " {id:'" + node.ID + "'})");
            //builder.AppendLine("SET newnode:" + node.SubLabel);
            builder.AppendLine("SET newnode.name = '" + node.Name + "'");
            builder.AppendLine("SET newnode.path = '" + node.Path + "'");

            if (node.Properties != null)
            {
                foreach (var prop in node.Properties)
                {
                    builder.AppendLine("SET newnode."+ prop.Key +" = '" + prop.Value.ToString() + "'");
                }
            }

            builder.AppendLine("RETURN newnode");

            using (var session = driver.Session())
            {
                var greeting = session.WriteTransaction(tx =>
                {
                    var result = tx.Run(builder.ToString());
                    return result.Single()[0].As<string>();
                });
            }
        }

        public static void MergeNodeOnPath(INode node, IDriver driver)
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("MERGE (newnode:" + node.Label + " {path:'" + node.Path + "'})");
            //builder.AppendLine("SET newnode:" + node.SubLabel);
            builder.AppendLine("SET newnode.name = '" + node.Name + "'");
            builder.AppendLine("SET newnode.id = '" + node.ID + "'");

            if (node.Properties != null)
            {
                foreach (var prop in node.Properties)
                {
                    builder.AppendLine("SET newnode." + prop.Key + " = '" + prop.Value + "'");
                }
            }

            builder.AppendLine("RETURN newnode");

            using (var session = driver.Session())
            {
                var greeting = session.WriteTransaction(tx =>
                {
                    var result = tx.Run(builder.ToString());
                    return result.Single()[0].As<string>();
                });
            }
        }

        public static void AddIsMemberOfADGroups(INode node, List<string> groupDNs, IDriver driver)
        {
            StringBuilder builder = new StringBuilder();
            int i = 0;

            //Example query
            //MATCH(a: AD_GROUP),(b: AD_GROUP)
            //WHERE a.name = 'Test2' AND b.name = 'Test3'
            //CREATE(a) -[r: AD_MemberOf]->(b)
            //RETURN type(r)
            builder.AppendLine("MERGE (node:" +node.Label + " {path:'" + node.Path + "'})");
            foreach (string dn in groupDNs)
            {
                builder.AppendLine("MERGE (g" + i + ":AD_Object {path:'" + dn + "'})");
                builder.AppendLine("MERGE (node)-[r" + i + ":AD_MemberOf]->(g" + i + ")");
                i++;
            }
            builder.AppendLine("RETURN node");

            using (var session = driver.Session())
            {
                session.WriteTransaction(tx =>
                {
                    var result = tx.Run(builder.ToString());
                    return result.Single()[0].As<string>();
                });
            }
        }

        public static void AddMembersOfADGroup(INode node, List<string> memberDNs, IDriver driver)
        {
            StringBuilder builder = new StringBuilder();
            int i = 0;

            //Example query
            //MATCH(a: AD_GROUP),(b: AD_GROUP)
            //WHERE a.name = 'Test2' AND b.name = 'Test3'
            //CREATE(a) -[r: AD_MemberOf]->(b)
            //RETURN type(r)
            builder.AppendLine("MERGE (node:" + node.Label + " {path:'" + node.Path + "'})");
            foreach (string dn in memberDNs)
            {
                builder.AppendLine("MERGE (g" + i + ":AD_Object {path:'" + dn + "'})");
                builder.AppendLine("MERGE (g" + i + ") -[r" + i + ": AD_MemberOf]->(node)");
                i++;
            }
            builder.AppendLine("RETURN node");

            using (var session = driver.Session())
            {
                var greeting = session.WriteTransaction(tx =>
                {
                    var result = tx.Run(builder.ToString());
                    return result.Single()[0].As<string>();
                });
            }
        }
    }
}
