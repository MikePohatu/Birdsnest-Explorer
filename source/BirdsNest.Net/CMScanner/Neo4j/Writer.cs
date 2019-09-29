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

        public static int MergeApplications(List<SccmApplication> apps, ISession session)
        {
            List<object> propertylist = new List<object>();

            foreach (SccmApplication i in apps)
            {
                propertylist.Add(new
                {
                    ID = i.ID,
                    Name = i.Name
                });
            }

            string query = "UNWIND $propertylist AS prop " +
                "MERGE (n:" + Types.CMApplication + "{id:prop.ID}) " +
                "SET n:" + Types.CMConfigurationItem + " " +
                "SET n.name = prop.Name " +
                "RETURN n.name";

            var result = session.WriteTransaction(tx => tx.Run(query, new { propertylist = propertylist }));
            return result.Summary.Counters.NodesCreated;
        }

        public static int MergePackages(List<SccmPackage> items, ISession session)
        {
            List<object> propertylist = new List<object>();

            foreach (SccmPackage i in items)
            {
                propertylist.Add(new
                {
                    ID = i.ID,
                    Name = i.Name
                });
            }

            string query = "UNWIND $propertylist AS prop " +
                "MERGE (n:" + Types.CMPackage + "{id:prop.ID}) " +
                "SET n:" + Types.CMConfigurationItem + " " +
                "SET n.name = prop.Name " +
                "RETURN n.name";

            var result = session.WriteTransaction(tx => tx.Run(query, new { propertylist = propertylist }));
            return result.Summary.Counters.NodesCreated;
        }

        public static int MergePackagePrograms(List<SccmPackageProgram> items, ISession session)
        {
            List<object> propertylist = new List<object>();

            foreach (SccmPackageProgram i in items)
            {
                propertylist.Add(new
                {
                    ID = i.ID,
                    Name = i.Name,
                    PackageID = i.PackageID,
                    CommandLine = i.CommandLine,
                    DependantID = i.DependentProgram
                });
            }

            string query = "UNWIND $propertylist AS prop " +
                "MERGE (n:" + Types.CMPackageProgram + "{id:prop.ID}) " +
                "SET n:" + Types.CMConfigurationItem + " " +
                "SET n.name = prop.Name " +
                "SET n.packageid = prop.PackageID " +
                "SET n.dependantid = prop.DependantID " +
                "SET n.commandline = prop.CommandLine " +
                "MERGE (parent:"+ Types.CMPackage +" {id:prop.PackageID}) " +
                "MERGE (parent)-[:CONTAINS]->(n) " +
                "RETURN n.name";

            var result = session.WriteTransaction(tx => tx.Run(query, new { propertylist = propertylist }));
            return result.Summary.Counters.NodesCreated;
        }

        public static int MergeTaskSequences(List<SccmTaskSequence> items, ISession session)
        {
            List<object> propertylist = new List<object>();

            foreach (SccmTaskSequence i in items)
            {
                propertylist.Add(new
                {
                    ID = i.ID,
                    Name = i.Name
                });
            }

            string query = "UNWIND $propertylist AS prop " +
                "MERGE (n:" + Types.CMTaskSequence + "{id:prop.ID}) " +
                "SET n:" + Types.CMConfigurationItem + " " +
                "SET n.name = prop.Name " +
                "RETURN n.name";

            var result = session.WriteTransaction(tx => tx.Run(query, new { propertylist = propertylist }));
            return result.Summary.Counters.NodesCreated;
        }

        public static int MergeSoftwareUpdateGroups(List<SccmSoftwareUpdateGroup> items, ISession session)
        {
            List<object> propertylist = new List<object>();

            foreach (SccmSoftwareUpdateGroup i in items)
            {
                propertylist.Add(new
                {
                    ID = i.ID,
                    Name = i.Name
                });
            }

            string query = "UNWIND $propertylist AS prop " +
                "MERGE (n:" + Types.CMSoftwareUpdateGroup + "{id:prop.ID}) " +
                "SET n:" + Types.CMConfigurationItem + " " +
                "SET n.name = prop.Name " +
                "RETURN n.name";

            var result = session.WriteTransaction(tx => tx.Run(query, new { propertylist = propertylist }));
            return result.Summary.Counters.NodesCreated;
        }

        public static int MergeApplicationDeployments(List<SMS_DeploymentSummary> items, ISession session)
        {
            List<object> deploymentlist = new List<object>();

            foreach (SMS_DeploymentSummary i in items)
            {
                deploymentlist.Add(new
                {
                    itemid = i.CIID,
                    collectionid = i.CollectionID
                });
            }

            string query = "UNWIND $deploymentlist AS dep " +
                "MATCH (n:" + Types.CMConfigurationItem + "{id:dep.itemid}) " +
                "MATCH (c:" + Types.CMCollection + "{id:dep.collectionid}) " +
                "MERGE (n)-[r:DEPLOYED_TO]->(c) " +
                "RETURN r";

            var result = session.WriteTransaction(tx => tx.Run(query, new { deploymentlist = deploymentlist }));
            return result.Summary.Counters.RelationshipsCreated;
        }

        public static int MergePackageProgramDeployments(List<SMS_DeploymentSummary> items, ISession session)
        {
            List<object> deploymentlist = new List<object>();

            foreach (SMS_DeploymentSummary i in items)
            {
                deploymentlist.Add(new
                {
                    itemid = i.PackageID + ";;"+ i.ProgramName,
                    collectionid = i.CollectionID
                });
            }

            string query = "UNWIND $deploymentlist AS dep " +
                "MATCH (n:" + Types.CMConfigurationItem + "{id:dep.itemid}) " +
                "MATCH (c:" + Types.CMCollection + "{id:dep.collectionid}) " +
                "MERGE (n)-[r:DEPLOYED_TO]->(c) " +
                "RETURN r";

            var result = session.WriteTransaction(tx => tx.Run(query, new { deploymentlist = deploymentlist }));
            return result.Summary.Counters.RelationshipsCreated;
        }

        public static int ConnectLimitingCollections(ISession session)
        {
            string query = "MATCH (n:" + Types.CMCollection + ")" +
                "MATCH (l:" + Types.CMCollection + " {id:n.limitingcollection}) " +
                "MERGE (l)-[:LIMITING_COLLECTION_FOR]->(n) " +
                "RETURN n.name";

            var result = session.WriteTransaction(tx => tx.Run(query));
            return result.Summary.Counters.NodesCreated;
        }

        public static int MergeDevices(List<SccmDevice> items, ISession session)
        {
            List<object> propertylist = new List<object>();

            foreach (SccmDevice i in items)
            {
                propertylist.Add(i.GetObject());
            }

            string query = "UNWIND $propertylist AS prop " +
                "MERGE (n:" + Types.CMDevice + "{id:prop.ID}) " +
                "SET n:" + Types.CMConfigurationItem + " " +
                "SET n.name = prop.Name " +
                "SET n.sid = prop.SID " +
                "SET n.dn = prop.DN " +
                "RETURN n.name";

            var result = session.WriteTransaction(tx => tx.Run(query, new { propertylist }));
            return result.Summary.Counters.NodesCreated;
        }

        public static int MergeUsers(List<SccmUser> items, ISession session)
        {
            List<object> propertylist = new List<object>();

            foreach (SccmUser i in items)
            {
                propertylist.Add(i.GetObject());
            }

            string query = "UNWIND $propertylist AS prop " +
                "MERGE (n:" + Types.CMUser + "{id:prop.ID}) " +
                "SET n:" + Types.CMConfigurationItem + " " +
                "SET n.name = prop.Name " +
                "SET n.sid = prop.SID " +
                "SET n.dn = prop.DN " +
                "RETURN n.name";

            var result = session.WriteTransaction(tx => tx.Run(query, new { propertylist }));
            return result.Summary.Counters.NodesCreated;
        }

        public static int MergeCollectionMembers(List<object> propertylist, ISession session)
        {
            string query = "UNWIND $propertylist AS prop " +
                "MATCH (n:" + Types.CMConfigurationItem + " {id: prop.resourceid}) " +
                "MATCH (c:" + Types.CMCollection + " {id: prop.collectionid}) " +
                "MERGE p=(n)-[r:" + Types.CMMemberOf + "]->(c) " +
                //"SET r.scanid=$scanid " +
                "RETURN p";

            var result = session.WriteTransaction(tx => tx.Run(query, new { propertylist }));
            return result.Summary.Counters.RelationshipsCreated;
        }

        public static int ConnectCmToAdObjects(ISession session)
        {
            string query = "MATCH (dev:" + Types.CMDevice + ") " +
                "MATCH (addev:" + Types.Computer + " {id: dev.sid }) " +
                "MERGE p=(dev)-[r:" + Types.CMObjectFor + "]->(addev) " +
                //"SET r.scanid=$scanid " +
                "RETURN p";

            var result = session.WriteTransaction(tx => tx.Run(query));
            return result.Summary.Counters.RelationshipsCreated;
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
