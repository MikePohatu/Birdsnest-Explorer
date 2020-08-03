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
                "SET folder:" + Types.Folder + ", " +
                "folder.name=$name, " +
                "folder.lastpermission=$lastfolder, " +
                "folder.inheritancedisabled=$inheritancedisabled, " +
                "folder.depth=$depth, " +
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
                    depth = folder.Depth,
                    scanid = this.ScanID,
                    fsid = this.FsID
                }));
                nodescreated = result.Summary.Counters.NodesCreated;
            }

            if (string.IsNullOrEmpty(folder.PermParent) == false)
            {
                ConnectFolderToParent(folder, driver);
            }

            //send the perms
            if (folder.PermissionCount > 0)
            {
                this.SendFolderPermissions(folder, driver);
            }

            return nodescreated;
        }

        public int ConnectFolderToParent(Folder folder, IDriver driver)
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("MERGE(folder:" + Types.Folder + " {path:$path}) ");
            builder.AppendLine("WITH folder ");
            builder.AppendLine("MERGE(lastfolder:" + Types.Folder + " {path:$lastfolder}) ");

            if (folder.InheritanceDisabled) { builder.AppendLine("MERGE (lastfolder) -[r:" + Types.BlockedInheritance + "]->(folder) "); }
            else { builder.AppendLine("MERGE (lastfolder) -[r:" + Types.AppliesInhertitanceTo + "]->(folder) "); }

            builder.AppendLine("SET r.lastscan = $scanid ");
            builder.AppendLine("SET r.fsid = $fsid ");
            builder.AppendLine("WITH folder ");
            builder.AppendLine($"MATCH (:{Types.Folder})-[r]->(folder) ");
            builder.AppendLine($"WHERE (type(r)='{Types.AppliesInhertitanceTo}' OR type(r)='{Types.BlockedInheritance}') AND r.lastscan <> $scanid ");
            builder.AppendLine("DELETE r ");
            builder.AppendLine("RETURN folder.path");

            int relcreated = 0;
            using (ISession session = driver.Session())
            {
                IStatementResult result = session.WriteTransaction(tx => tx.Run(builder.ToString(), new
                {
                    path = folder.Path,
                    lastfolder = folder.PermParent,
                    scanid = this.ScanID,
                    fsid = this.FsID
                }));
                relcreated = result.Summary.Counters.RelationshipsCreated;
            }
            return relcreated;
        }

        public int SendFolderPermissions(Folder folder, IDriver driver)
        {
            string query = 
                
            "UNWIND $domainpermissions as domainperm" +
            " MERGE(folderDom:" + Types.Folder + " {path:$path})" +
            " WITH folderDom,domainperm" +
            " MERGE(ndom:" + Types.ADObject + " {id:domainperm.ID})" +
            " ON CREATE SET ndom:" + Types.Orphaned + ",ndom.lastscan = $scanid" +
            " MERGE (ndom)-[r:" + Types.AppliesPermissionTo + "]->(folderDom)" +
            " SET r.right=domainperm.Right" +
            " SET r.accesstype=domainperm.AccessType" +
            " SET r.lastscan=$scanid" +
            " SET r.fsid=$fsid" +

            " WITH folderDom" +
            " UNWIND $builtinperms as builtinperm" +
            " MERGE(folderBuiltin:" + Types.Folder + " {path:$path})" +
            " MERGE(nbuiltin:" + Types.BuiltinObject + " {id:builtinperm.ID})" +
            " MERGE (nbuiltin)-[r:" + Types.AppliesPermissionTo + "]->(folderBuiltin)" +
            " SET r.right=builtinperm.Right" +
            " SET r.accesstype=builtinperm.AccessType" +
            " SET r.lastscan=$scanid" +
            " SET r.fsid=$fsid" +

            " WITH folderDom, folderBuiltin" +
            " MATCH ()-[rdom:" + Types.AppliesPermissionTo + "]->(folderDom)" +
            " WHERE rdom.lastscan <> $scanid" +
            " DELETE rdom" +

            " WITH folderDom, folderBuiltin" +
            " MATCH ()-[rbuiltin:" + Types.AppliesPermissionTo + "]->(folderBuiltin)" +
            " WHERE rbuiltin.lastscan <> $scanid" +
            " DELETE rbuiltin" +

            " RETURN $path";

            int relcreated = 0;
            using (ISession session = driver.Session())
            {
                IStatementResult result = session.WriteTransaction(tx => tx.Run(query, new 
                { 
                    fsid = this.FsID, 
                    scanid = this.ScanID, 
                    domainpermissions = folder.DomainPermissions, 
                    builtinperms = folder.BuiltinPermissions,
                    path = folder.Path
                }));
                relcreated = result.Summary.Counters.RelationshipsCreated;
            }
            return relcreated;
        }

        public int SendDatastore(DataStore ds, IDriver driver)
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

        public int AttachRootToDataStore(DataStore ds, string rootpath, IDriver driver)
        {
            string query = "MERGE(datastore:" + Types.Datastore + " {name:$dsname}) " +
            "MERGE(root:" + Types.Folder + " {path:$rootpath}) " +
            "MERGE (datastore)-[r:" + Types.Hosts + "]->(root) " +
            "RETURN * ";

            int relcreated = 0;
            using (ISession session = driver.Session())
            {
                IStatementResult result = session.WriteTransaction(tx => tx.Run(query, new { 
                    dsname = ds.Name, 
                    rootpath = rootpath?.ToLower(), 
                    scanid = this.ScanID}));
                relcreated = result.Summary.Counters.RelationshipsCreated;
            }

            return relcreated;
        }

        public int CleanupChangedFolders(string rootpath, IDriver driver)
        {
            string query = "MATCH(f:" + Types.Folder + ") " +
            "WHERE f.fsid = $fsid AND f.lastscan<>$scanid " +
            "DETACH DELETE f ";

            int nodesdeleted = 0;
            using (ISession session = driver.Session())
            {
                IStatementResult result = session.WriteTransaction(tx => tx.Run(query, new {
                    rootpath,
                    scanid = this.ScanID,
                    fsid = this.FsID
                }));
                nodesdeleted = result.Summary.Counters.NodesDeleted;
            }

            return nodesdeleted;
        }

        public int CleanupConnections(string rootpath, IDriver driver)
        {
            string query = "MATCH (:" + Types.Folder + " {fsid: $fsid})-[r]-()"+
                " WHERE r.fsid = $fsid AND r.lastscan <> $scanid" +
                " DELETE r ";

            int nodesdeleted = 0;
            using (ISession session = driver.Session())
            {
                IStatementResult result = session.WriteTransaction(tx => tx.Run(query, new {
                    rootpath = rootpath,
                    scanid = this.ScanID,
                    fsid = this.FsID }));
                nodesdeleted = result.Summary.Counters.NodesDeleted;
            }

            return nodesdeleted;
        }
    }
}
