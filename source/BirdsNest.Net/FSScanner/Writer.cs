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
        private ISession _session;

        public int FolderCount { get; set; } = 0;
        public int PermissionCount { get; set; } = 0;

        public Writer (ISession session)
        {
            this._session = session;
        }

        public void QueueFolder(Folder newfolder)
        {
            //try to send the folder first, queue it if not
            try
            {
                this.FolderCount++;
                SendFolder(newfolder);
            }
            catch (Exception e)
            {
                Console.WriteLine("Failed to send folder: " + newfolder.Path + ": " + e.Message);
                this._queuedfolders.Add(newfolder);
            }
        }

        public void FlushFolderQueue()
        {
            foreach (Folder f in this._queuedfolders)
            {
                try
                {
                    SendFolder(f);
                    this._queuedfolders.Remove(f);
                    this.FolderCount++;
                }
                catch (Exception e)
                { Console.WriteLine("Failed to send folder: " + f.Path + ": " + e.Message); }
            }

            //try
            //{
            //    this.SendPermissions(this._permissions);
            //}
            //catch (Exception e)
            //{ Console.WriteLine("Failed to send permissions: " + e.Message); }
        }

        private int SendRoot(Folder folder)
        {
            string query = "MERGE(folder:" + folder.Type + "{path:$path}) " +
            "RETURN folder.path ";

            IStatementResult result = this._session.WriteTransaction(tx => tx.Run(query, new { path = folder.Path, lastfolder = folder.PermParent }));
            return result.Summary.Counters.NodesCreated;
        }

        public int SendFolder(Folder folder)
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("MERGE(folder:" + folder.Type + "{path:$path}) ");

            if (string.IsNullOrEmpty(folder.PermParent) == false)
            {
                ConnectFolderToParent(folder);
            }

            if (folder.Permissions?.Count > 0)
            {
                this.SendPermissions(folder.Permissions);
            }

            IStatementResult result = this._session.WriteTransaction(tx => tx.Run(builder.ToString(), new { path = folder.Path, lastfolder = folder.PermParent }));
            return result.Summary.Counters.NodesCreated;
        }

        public int ConnectFolderToParent(Folder folder)
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("MERGE(folder:" + folder.Type + "{path:$path}) ");
            builder.AppendLine("WITH folder ");
            builder.AppendLine("MERGE(lastfolder:" + Types.Folder + "{path:$lastfolder}) ");

            if (folder.InheritanceDisabled) { builder.AppendLine("MERGE (folder) -[:" + Types.BlocksInheritanceFrom + "]->(lastfolder) RETURN folder.path"); }
            else { builder.AppendLine("MERGE (folder) -[:" + Types.InheritsFrom + "]->(lastfolder) RETURN folder.path"); }

            IStatementResult result = this._session.WriteTransaction(tx => tx.Run(builder.ToString(), new { path = folder.Path, lastfolder = folder.PermParent }));
            return result.Summary.Counters.RelationshipsCreated;
        }

        public int SendBlockedInheritanceFolder(Folder folder)
        {
            string query = "MERGE(folder:" + folder.Type + "{path:$path}) " +
            "WITH folder " +
            "MATCH(lastfolder:" + Types.Folder + " {path:$lastfolder}) " +
            "MERGE (folder) -[:" + Types.BlocksInheritanceFrom + "]->(lastfolder) " +
            "RETURN folder.path ";

            IStatementResult result = this._session.WriteTransaction(tx => tx.Run(query, new { path = folder.Path, lastfolder = folder.PermParent }));
            return result.Summary.Counters.NodesCreated;
        }

        public int SendPermissions(List<Permission> permissions)
        {
            string query = "UNWIND $perms as p " +
            "MERGE(folder:" + Types.Folder + "{path:p.Path}) " +
            "WITH folder,p " +
            "MERGE(n {id:p.ID})  " +
            "ON CREATE SET n:" + CommonTypes.Orphaned + " " +
            //"MERGE (folder) -[:" + CommonTypes.Uses + "]->(n) " +
            "MERGE (n) -[r:" + CommonTypes.GivesAccessTo + "]->(folder) " +
            "SET r.right=p.Right " +
            "RETURN folder.path ";

            IStatementResult result = this._session.WriteTransaction(tx => tx.Run(query, new { perms = permissions }));
            return result.Summary.Counters.RelationshipsCreated;
        }

        public int SendDatastore(DataStore ds)
        {
            string query = "MERGE(n:" + Types.Datastore + " {name:$Name}) " +           
            "SET n.comment=$Comment " +
            "SET n.host=$Host " +
            "MERGE(host:" + CommonTypes.Device + " {name:$Host}) " +
            "MERGE (n)-[r:" + CommonTypes.ConnectedTo + "]->(host) " +
            "RETURN n ";

            IStatementResult result = this._session.WriteTransaction(tx => tx.Run(query, ds));
            return result.Summary.Counters.RelationshipsCreated;
        }

        public int AttachRootToDataStore(DataStore ds, string rootpath)
        {
            string query = "MERGE(datastore:" + Types.Datastore + " {name:$dsname}) " +
            "MERGE(root:" + Types.Folder + " {path:$rootpath}) " +
            "MERGE (root)-[r:" + Types.HostedOn + "]->(datastore) " +
            "RETURN * ";

            IStatementResult result = this._session.WriteTransaction(tx => tx.Run(query, new { dsname=ds.Name, rootpath=rootpath?.ToLower()}));
            return result.Summary.Counters.RelationshipsCreated;
        }
    }
}
