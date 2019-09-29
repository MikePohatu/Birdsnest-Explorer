using common;
using Neo4j.Driver.V1;
using System.Collections.Generic;
using CMScanner.Sccm;

namespace CMScanner.Neo4j
{
    public static class Writer
    {
        public static string ScanID { get; set; }

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
                "SET n.lastscan=$scanid " +
                "RETURN n.name";

            var result = session.WriteTransaction(tx => tx.Run(query, new { propertylist = propertylist, scanid = Writer.ScanID }));
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
                "SET n.lastscan=$scanid " +
                "RETURN n.name";

            var result = session.WriteTransaction(tx => tx.Run(query, new { propertylist = propertylist, scanid = Writer.ScanID }));
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
                "SET n.lastscan=$scanid " +
                "RETURN n.name";

            var result = session.WriteTransaction(tx => tx.Run(query, new { propertylist = propertylist, scanid = Writer.ScanID }));
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
                "SET n.lastscan=$scanid " +
                "MERGE (parent:" + Types.CMPackage +" {id:prop.PackageID}) " +
                "MERGE (parent)-[r:CONTAINS]->(n) " +
                "SET r.lastscan=$scanid " +
                "RETURN n.name";

            var result = session.WriteTransaction(tx => tx.Run(query, new { propertylist = propertylist, scanid = Writer.ScanID }));
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
                "SET n.lastscan=$scanid " +
                "RETURN n.name";

            var result = session.WriteTransaction(tx => tx.Run(query, new { propertylist = propertylist, scanid = Writer.ScanID }));
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
                "SET n.lastscan=$scanid " +
                "RETURN n.name";

            var result = session.WriteTransaction(tx => tx.Run(query, new { propertylist = propertylist, scanid = Writer.ScanID }));
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
                "SET r.lastscan=$scanid " +
                "RETURN r";

            var result = session.WriteTransaction(tx => tx.Run(query, new { deploymentlist = deploymentlist, scanid = Writer.ScanID }));
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
                "SET r.lastscan=$scanid " +
                "RETURN r";

            var result = session.WriteTransaction(tx => tx.Run(query, new { deploymentlist = deploymentlist, scanid = Writer.ScanID }));
            return result.Summary.Counters.RelationshipsCreated;
        }

        public static int ConnectLimitingCollections(ISession session)
        {
            string query = "MATCH (n:" + Types.CMCollection + ")" +
                "MATCH (l:" + Types.CMCollection + " {id:n.limitingcollection}) " +
                "MERGE (l)-[r:" +Types.CMLimitingCollection+ "]->(n) " +
                "SET r.lastscan=$scanid " +
                "RETURN n.name";

            var result = session.WriteTransaction(tx => tx.Run(query, new { scanid = Writer.ScanID}));
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
                "SET n.lastscan=$scanid " +
                "RETURN n.name";

            var result = session.WriteTransaction(tx => tx.Run(query, new { propertylist, scanid = Writer.ScanID }));
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
                "SET n.lastscan=$scanid " +
                "RETURN n.name";

            var result = session.WriteTransaction(tx => tx.Run(query, new { propertylist, scanid = Writer.ScanID }));
            return result.Summary.Counters.NodesCreated;
        }

        public static int MergeCollectionMembers(List<object> propertylist, ISession session)
        {
            string query = "UNWIND $propertylist AS prop " +
                "MATCH (n:" + Types.CMConfigurationItem + " {id: prop.resourceid}) " +
                "MATCH (c:" + Types.CMCollection + " {id: prop.collectionid}) " +
                "MERGE p=(n)-[r:" + Types.CMMemberOf + "]->(c) " +
                "SET r.lastscan=$scanid " +
                "RETURN p";

            var result = session.WriteTransaction(tx => tx.Run(query, new { propertylist, scanid = Writer.ScanID }));
            return result.Summary.Counters.RelationshipsCreated;
        }

        public static int ConnectCmToAdObjects(ISession session)
        {
            int count = 0;
            //devices first
            string query = "MATCH (dev:" + Types.CMDevice + ") " +
                "MATCH (addev:" + Types.Computer + " {id: dev.sid }) " +
                "MERGE p=(dev)-[r:" + Types.CMObjectFor + "]->(addev) " +
                "SET r.lastscan=$scanid " +
                "RETURN p";

            var result = session.WriteTransaction(tx => tx.Run(query, new { scanid = Writer.ScanID }));
            count = result.Summary.Counters.RelationshipsCreated;

            //now users
            query = "MATCH (user:" + Types.CMUser + ") " +
                "MATCH (aduser:" + Types.User + " {id: user.sid }) " +
                "MERGE p=(user)-[r:" + Types.CMObjectFor + "]->(aduser) " +
                "SET r.lastscan=$scanid " +
                "RETURN p";

            result = session.WriteTransaction(tx => tx.Run(query, new {scanid = Writer.ScanID }));
            return count + result.Summary.Counters.RelationshipsCreated;
        }

        public static int CleanupCmObjects(ISession session)
        {
            int count = 0;
            //nodes first
            string query = "MATCH (n:" + Types.CMConfigurationItem + ") " +
                "WHERE n.lastscan<>$scanid " +
                "DETACH DELETE n " +
                "RETURN n";

            var result = session.WriteTransaction(tx => tx.Run(query, new { scanid = Writer.ScanID }));
            count = result.Summary.Counters.NodesDeleted;

            //any remaining edges
            query = "MATCH ()-[r:" + Types.CMObjectFor + "]->() " +
                "WHERE r.lastscan<>$scanid " +
                "DELETE r " +
                "RETURN r";

            result = session.WriteTransaction(tx => tx.Run(query, new { scanid = Writer.ScanID }));
            count = count + result.Summary.Counters.RelationshipsDeleted;

            query = "MATCH ()-[r:" + Types.CMLimitingCollection + "]->() " +
                "WHERE r.lastscan<>$scanid " +
                "DELETE r " +
                "RETURN r";

            result = session.WriteTransaction(tx => tx.Run(query, new { scanid = Writer.ScanID }));
            return count + result.Summary.Counters.RelationshipsDeleted;
        }
    }
}
