using System.Collections.Generic;
using Neo4j.Driver.V1;
using System;
using System.Text;
using common;

namespace FSScanner
{
    public class Writer
    {
        private List<Folder> _queuedfolders = new List<Folder>();

        public int FolderCount { get; set; } = 0;
        public int PermissionCount { get; set; } = 0;

        public void UpdateFolder(Folder newfolder, ISession session)
        {
            //try to send the folder first, queue it if not
            try
            {
                this.FolderCount++;
                SendFolder(newfolder, session);
            }
            catch (Exception e)
            {
                Console.WriteLine("Failed to send folder: " + newfolder.Path + ": " + e.Message);
                this._queuedfolders.Add(newfolder);
            }
        }

        public void FlushFolderQueue(ISession session)
        {
            foreach (Folder f in this._queuedfolders)
            {
                try
                {
                    SendFolder(f, session);
                    this._queuedfolders.Remove(f);
                    this.FolderCount++;
                }
                catch (Exception e)
                { Console.WriteLine("Failed to send folder: " + f.Path + ": " + e.Message); }
            }
        }

        public int SendFolder(Folder folder, ISession session)
        {
            string query = "MERGE(folder {path:$path}) " +
                "SET folder:" + folder.Type + ", " +
                "folder.name=$name, " +
                "folder.lastpermission=$lastfolder, " +
                "folder.inheritancedisabled=$inheritancedisabled, " +
                "folder.lastscan=$scanid, " +
                "folder.blocked=$blocked " +
                "RETURN folder";
            IStatementResult result = session.WriteTransaction(tx => tx.Run(query, new
            {
                path = folder.Path,
                lastfolder = folder.PermParent,
                inheritancedisabled = folder.InheritanceDisabled,
                blocked = folder.Blocked,
                name = folder.Name,
                scanid = folder.ScanId
            }));

            if (string.IsNullOrEmpty(folder.PermParent) == false)
            {
                ConnectFolderToParent(folder, session);
            }

            if (folder.Permissions?.Count > 0)
            {
                this.SendPermissions(folder.Permissions, session);
            }

            return result.Summary.Counters.NodesCreated;
        }

        public int ConnectFolderToParent(Folder folder, ISession session)
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("MERGE(folder {path:$path}) ");
            builder.AppendLine("WITH folder ");
            builder.AppendLine("MERGE(lastfolder {path:$lastfolder}) ");

            if (folder.InheritanceDisabled) { builder.AppendLine("MERGE (folder) -[r:" + Types.BlocksInheritanceFrom + "]->(lastfolder) "); }
            else { builder.AppendLine("MERGE (folder) -[r:" + Types.InheritsFrom + "]->(lastfolder) "); }

            builder.AppendLine("SET r.lastscan = $scanid ");
            builder.AppendLine("RETURN folder.path");

            IStatementResult result = session.WriteTransaction(tx => tx.Run(builder.ToString(), new {
                path = folder.Path,
                lastfolder = folder.PermParent,
                scanid = folder.ScanId
            }));
            return result.Summary.Counters.RelationshipsCreated;
        }

        public int SendPermissions(List<Permission> permissions, ISession session)
        {
            string query = "UNWIND $perms as p " +
            "MERGE(folder {path:p.Path}) " +
            "WITH folder,p " +
            "MERGE(n {id:p.ID})  " +
            "ON CREATE SET n:" + CommonTypes.Orphaned + ",n.lastscan = p.ScanId " +
            "MERGE (n) -[r:" + CommonTypes.GivesAccessTo + "]->(folder) " +
            "SET r.right=p.Right " +
            "SET r.lastscan=p.ScanId " +
            "RETURN folder.path ";

            IStatementResult result = session.WriteTransaction(tx => tx.Run(query, new { perms = permissions }));
            return result.Summary.Counters.RelationshipsCreated;
        }

        public int SendDatastore(DataStore ds, string scanid, ISession session)
        {
            string query = "MERGE(n:" + Types.Datastore + " {name:$Name}) " +           
            "SET n.comment=$Comment " +
            "SET n.host=$Host " +
            "MERGE(host:" + CommonTypes.Device + " {name:$Host}) " +
            "MERGE (n)-[r:" + CommonTypes.ConnectedTo + "]->(host) " +
            "RETURN n ";

            IStatementResult result = session.WriteTransaction(tx => tx.Run(query, ds));
            return result.Summary.Counters.RelationshipsCreated;
        }

        public int AttachRootToDataStore(DataStore ds, string rootpath, string scanid, ISession session)
        {
            string query = "MERGE(datastore:" + Types.Datastore + " {name:$dsname}) " +
            "MERGE(root:" + Types.Folder + " {path:$rootpath}) " +
            "MERGE (root)-[r:" + Types.HostedOn + "]->(datastore) " +
            "RETURN * ";

            IStatementResult result = session.WriteTransaction(tx => tx.Run(query, new { dsname=ds.Name, rootpath=rootpath?.ToLower()}));
            return result.Summary.Counters.RelationshipsCreated;
        }

        public int CleanupChangedFolders(string rootpath, string scanid, ISession session)
        {
            string query = "MATCH(f:" + Types.Folder + ") " +
            "WHERE f.path STARTS WITH $rootpath AND f.lastscan<>$scanid " +
            "DETACH DELETE f ";

            IStatementResult result = session.WriteTransaction(tx => tx.Run(query, new { rootpath = rootpath, scanid=scanid }));
            return result.Summary.Counters.NodesDeleted;
        }

        public int CleanupInheritanceMappings(string rootpath, string scanid, ISession session)
        {
            string query = "MATCH (f:" + Types.Folder +") "+
                "WHERE f.path STARTS WITH $rootpath " +
                "WITH f " +
                "MATCH(f) -[r]->() WHERE r.lastscan <> $scanid " +
                "DELETE r ";

            IStatementResult result = session.WriteTransaction(tx => tx.Run(query, new { rootpath = rootpath, scanid = scanid }));
            return result.Summary.Counters.NodesDeleted;
        }
    }
}
