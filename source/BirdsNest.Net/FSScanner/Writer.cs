using System.Collections.Generic;
using Neo4j.Driver.V1;
using System;
using common;

namespace FSScanner
{
    public class Writer
    {
        private List<Folder> _queuedfolders = new List<Folder>();
        private List<Permission> _permissions = new List<Permission>();
        private ISession _session;
        private DateTime _lastqueuedsend;

        public int FolderCount { get; set; } = 0;
        public int PermissionCount { get; set; } = 0;

        public Writer (ISession session)
        {
            this._session = session;
        }

        public void QueueFolder(Folder newfolder)
        {
            foreach (var rule in newfolder.Permissions)
            {
                this._permissions.AddRange(newfolder.Permissions);
            }
            //try to send the folder first, queue it if not
            try
            {
                SendFolder(newfolder);
                this.FolderCount++;
            }
            catch (Exception e)
            {
                Console.WriteLine("Failed to send folder: " + newfolder.Path + ": " + e.Message);
                this._queuedfolders.Add(newfolder);
            }

            if ((this._queuedfolders.Count > 0 ) && (DateTime.Now - _lastqueuedsend) > TimeSpan.FromSeconds(30))
            {
                this._lastqueuedsend = DateTime.Now;
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
            string query = "MERGE(folder:" + folder.Type + "{path:$path}) " +
            "WITH folder " +
            "MATCH(lastfolder:" + Types.Folder + " {path:$lastfolder}) " +
            "MERGE (folder) -[:" + Types.InheritsFrom + "]->(lastfolder) " +
            "RETURN folder.path ";

            IStatementResult result = this._session.WriteTransaction(tx => tx.Run(query, new { path = folder.Path, lastfolder = folder.PermParent }));
            return result.Summary.Counters.NodesCreated;
        }

        public int SendPermissions()
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

            IStatementResult result = this._session.WriteTransaction(tx => tx.Run(query, new { perms = this._permissions }));
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
