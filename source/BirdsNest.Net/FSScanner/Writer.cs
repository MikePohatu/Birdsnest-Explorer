#region license
// Copyright (c) 2019-2020 "20Road"
// 20Road Limited [https://20road.com]
//
// This file is part of BirdsNest.
//
// BirdsNest is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, version 3 of the License.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.
#endregion
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
        public string FsID { get; private set; }
        public string ScanID { get; private set; }

        public Writer(string fsid, string scanid)
        {
            this.FsID = fsid;
            this.ScanID = scanid;
        }

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
                "folder.fsid=$fsid, " +
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
                    blocked = folder.Blocked,
                    name = folder.Name,
                    scanid = this.ScanID,
                    fsid = this.FsID
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
                    scanid = this.ScanID
                }));
                relcreated = result.Summary.Counters.RelationshipsCreated;
            }
            return relcreated;
        }

        public int SendPermissions(List<Permission> permissions, IDriver driver)
        {
            string query = "UNWIND $perms as p " +
            "MERGE(folder:"+ Types.Folder +" {path:p.Path}) " +
            "WITH folder,p " +
            "MERGE(n {id:p.ID})  " + // generic because could be ad_object or builtin
            "ON CREATE SET n:" + Types.Orphaned + ",n.lastscan = p.ScanId " +
            "MERGE (n) -[r:" + Types.GivesAccessTo + "]->(folder) " +
            "SET r.right=p.Right " +
            "SET r.lastscan=$scanid " +
            "SET r.fsid=$fsid " +
            "RETURN folder.path ";

            int relcreated = 0;
            using (ISession session = driver.Session())
            {
                IStatementResult result = session.WriteTransaction(tx => tx.Run(query, new { fsid = this.FsID, scanid = this.ScanID, perms = permissions }));
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
            "WHERE f.fsid = $fsid AND f.lastscan<>$scanid " +
            "DETACH DELETE f ";

            int nodesdeleted = 0;
            using (ISession session = driver.Session())
            {
                IStatementResult result = session.WriteTransaction(tx => tx.Run(query, new {
                    rootpath = rootpath,
                    scanid = scanid,
                    fsid = this.FsID
                }));
                nodesdeleted = result.Summary.Counters.NodesDeleted;
            }

            return nodesdeleted;
        }

        public int CleanupConnections(string rootpath, string scanid, IDriver driver)
        {
            string query = "MATCH (:" + Types.Folder + " {fsid: $fsid})-[r]-() WHERE r.lastscan <> $scanid " +
                "DELETE r ";

            int nodesdeleted = 0;
            using (ISession session = driver.Session())
            {
                IStatementResult result = session.WriteTransaction(tx => tx.Run(query, new {
                    rootpath = rootpath,
                    scanid = scanid,
                    fsid = this.FsID }));
                nodesdeleted = result.Summary.Counters.NodesDeleted;
            }

            return nodesdeleted;
        }
    }
}
