using System.Collections.Generic;
using Neo4j.Driver.V1;
using System;
using System.Text;
using common;
using System.Collections.Concurrent;

namespace FSScanner
{
    public class Writer
    {
        private ConcurrentBag<Folder> _queuedfolders = new ConcurrentBag<Folder>();

        public int FolderCount { get; set; } = 0;
        public int PermissionCount { get; set; } = 0;

        public void UpdateFolder(Folder newfolder, IDriver driver)
        {
            //try to send the folder first, queue it if not
            try
            {
                this.FolderCount++;
                SendFolder(newfolder, driver);
            }
            catch (Exception e)
            {
                ConsoleWriter.WriteError("Failed to send folder: " + newfolder.Path + ": " + e.Message);
                this._queuedfolders.Add(newfolder);
            }
        }

        public void FlushFolderQueue(IDriver driver)
        {
            foreach (Folder f in this._queuedfolders)
            {
                try
                {
                    int nodecount = SendFolder(f, driver);
                    this.FolderCount++;
                }
                catch (Exception e)
                { ConsoleWriter.WriteError("Failed to send folder: " + f.Path + ": " + e.Message); }
            }
        }

        public int SendFolder(Folder folder, IDriver driver)
        {
            string query = "MERGE(folder {path:$path}) " +
                "SET folder:" + folder.Type + ", " +
                "folder.name=$name, " +
                "folder.lastpermission=$lastfolder, " +
                "folder.inheritancedisabled=$inheritancedisabled, " +
                "folder.lastscan=$scanid, " +
                "folder.blocked=$blocked, " +
                "folder.layout='tree' " +
                "RETURN folder";

            int nodescreated = 0;
            using (ISession session = driver.Session())
            {
                IStatementResult result = session.WriteTransaction(tx => tx.Run(query, new
                {
                    path = folder.Path,
                    lastfolder = folder.PermParent,
                    inheritancedisabled = folder.InheritanceDisabled,
                    blocked = folder.Blocked.ToString(),
                    name = folder.Name,
                    scanid = folder.ScanId
                }));
                nodescreated = result.Summary.Counters.NodesCreated;
            }

            if (string.IsNullOrEmpty(folder.PermParent) == false)
            {
                ConnectFolderToParent(folder, driver);
            }

            if (folder.Permissions?.Count > 0)
            {
                this.SendPermissions(folder.Permissions, driver);
            }

            return nodescreated;
        }

        public int ConnectFolderToParent(Folder folder, IDriver driver)
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("MERGE(folder {path:$path}) ");
            builder.AppendLine("WITH folder ");
            builder.AppendLine("MERGE(lastfolder {path:$lastfolder}) ");

            if (folder.InheritanceDisabled) { builder.AppendLine("MERGE (lastfolder) -[r:" + Types.BlockedInheritance + "]->(folder) "); }
            else { builder.AppendLine("MERGE (lastfolder) -[r:" + Types.AppliesInhertitanceTo + "]->(folder) "); }

            builder.AppendLine("SET r.lastscan = $scanid ");
            builder.AppendLine("RETURN folder.path");

            int relcreated = 0;
            using (ISession session = driver.Session())
            {
                IStatementResult result = session.WriteTransaction(tx => tx.Run(builder.ToString(), new
                {
                    path = folder.Path,
                    lastfolder = folder.PermParent,
                    scanid = folder.ScanId
                }));
                relcreated = result.Summary.Counters.RelationshipsCreated;
            }
            return relcreated;
        }

        public int SendPermissions(List<Permission> permissions, IDriver driver)
        {
            string query = "UNWIND $perms as p " +
            "MERGE(folder {path:p.Path}) " +
            "WITH folder,p " +
            "MERGE(n {id:p.ID})  " +
            "ON CREATE SET n:" + Types.Orphaned + ",n.lastscan = p.ScanId " +
            "MERGE (n) -[r:" + Types.GivesAccessTo + "]->(folder) " +
            "SET r.right=p.Right " +
            "SET r.lastscan=p.ScanId " +
            "RETURN folder.path ";

            int relcreated = 0;
            using (ISession session = driver.Session())
            {
                IStatementResult result = session.WriteTransaction(tx => tx.Run(query, new { perms = permissions }));
                relcreated = result.Summary.Counters.RelationshipsCreated;
            }
            return relcreated;
        }

        public int SendDatastore(DataStore ds, string scanid, IDriver driver)
        {
            string query = "MERGE(n:" + Types.Datastore + " {name:$Name}) " +           
            "SET n.comment=$Comment " +
            "SET n.host=$Host " +
            "SET n.layout='tree' " +
            "MERGE(host:" + Types.Device + " {name:$Host}) " +
            "MERGE (n)-[r:" + Types.ConnectedTo + "]->(host) " +
            "RETURN n ";

            int relcreated = 0;
            using (ISession session = driver.Session())
            {
                IStatementResult result = session.WriteTransaction(tx => tx.Run(query, ds));
                relcreated = result.Summary.Counters.RelationshipsCreated;
            }

            return relcreated;
        }

        public int AttachRootToDataStore(DataStore ds, string rootpath, string scanid, IDriver driver)
        {
            string query = "MERGE(datastore:" + Types.Datastore + " {name:$dsname}) " +
            "MERGE(root:" + Types.Folder + " {path:$rootpath}) " +
            "MERGE (datastore)-[r:" + Types.Hosts + "]->(root) " +
            "RETURN * ";

            int relcreated = 0;
            using (ISession session = driver.Session())
            {
                IStatementResult result = session.WriteTransaction(tx => tx.Run(query, new { dsname = ds.Name, rootpath = rootpath?.ToLower() }));
                relcreated = result.Summary.Counters.RelationshipsCreated;
            }

            return relcreated;
        }

        public int CleanupChangedFolders(string rootpath, string scanid, IDriver driver)
        {
            string query = "MATCH(f:" + Types.Folder + ") " +
            "WHERE f.path STARTS WITH $rootpath AND f.lastscan<>$scanid " +
            "DETACH DELETE f ";

            int nodesdeleted = 0;
            using (ISession session = driver.Session())
            {
                IStatementResult result = session.WriteTransaction(tx => tx.Run(query, new { rootpath = rootpath, scanid = scanid }));
                nodesdeleted = result.Summary.Counters.NodesDeleted;
            }

            return nodesdeleted;
        }

        public int CleanupInheritanceMappings(string rootpath, string scanid, IDriver driver)
        {
            string query = "MATCH (f:" + Types.Folder +") "+
                "WHERE f.path STARTS WITH $rootpath " +
                "WITH f " +
                "MATCH(f) -[r]->() WHERE r.lastscan <> $scanid " +
                "DELETE r ";

            int nodesdeleted = 0;
            using (ISession session = driver.Session())
            {
                IStatementResult result = session.WriteTransaction(tx => tx.Run(query, new { rootpath = rootpath, scanid = scanid }));
                nodesdeleted = result.Summary.Counters.NodesDeleted;
            }

            return nodesdeleted;
        }

        public static void UpdateMetadata(IDriver driver)
        {
            List<string> types = new List<string>() { Types.Folder, Types.Datastore };

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
