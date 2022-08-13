#region license
// Copyright (c) 2019-2020 "20Road"
// 20Road Limited [https://www.20road.com]
//
// This file is part of Birdsnest Explorer.
//
// Birdsnest Explorer is free software: you can redistribute it and/or modify
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
using common;
using Neo4j.Driver;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace FSScanner
{
    public class Writer
    {
        private ConcurrentBag<Folder> _queuedfolders = new ConcurrentBag<Folder>();

        public int FolderCount { get; set; } = 0;
        public int PermissionCount { get; set; } = 0;
        public string FsID { get; private set; }

        public Writer(string fsid)
        {
            this.FsID = fsid;
        }

        public async Task UpdateFolderAsync(Folder newfolder, IDriver driver)
        {
            //try to send the folder first, queue it if not
            try
            {
                this.FolderCount++;
                await this.SendFolderAsync(newfolder, driver);
            }
            catch (Exception e)
            {
                ConsoleWriter.WriteError("Failed to send folder: " + newfolder.Path + ": " + e.Message);
                this._queuedfolders.Add(newfolder);
            }
        }

        public async Task FlushFolderQueueAsync(IDriver driver)
        {
            foreach (Folder f in this._queuedfolders)
            {
                try
                {
                    int nodecount = await SendFolderAsync(f, driver);
                    this.FolderCount++;
                }
                catch (Exception e)
                { ConsoleWriter.WriteError("Failed to send queued folder: " + f.Path + ": " + e.Message); }
            }
        }

        public async Task<int> SendFolderAsync(Folder folder, IDriver driver)
        {
            string query = "UNWIND $Properties AS prop" +
                " MERGE (folder:" + Types.Folder + " {path:prop.path})" +
                " SET folder.name=prop.name," +
                " folder.lastpermission=prop.lastfolder," +
                " folder.inheritancedisabled=prop.inheritancedisabled," +
                " folder.depth=prop.depth," +
                " folder.lastscan=$ScanID," +
                " folder.fsid=prop.fsid," +
                " folder.blocked=prop.blocked" +
                " RETURN folder";

            NeoQueryData querydata = new NeoQueryData();
            querydata.Properties = new List<object>();
            querydata.Properties.Add(new
            {
                path = folder.Path,
                lastfolder = folder.PermParent,
                inheritancedisabled = folder.InheritanceDisabled,
                blocked = folder.Blocked,
                name = folder.Name,
                depth = folder.Depth,
                fsid = this.FsID
            });
            var summaryList = await NeoWriter.RunQueryAsync(query, querydata, driver);
            TransactionResult<List<string>> result = new TransactionResult<List<string>>(summaryList);

            if (string.IsNullOrEmpty(folder.PermParent) == false)
            {
                await this.ConnectFolderToParentAsync(folder, driver);
            }

            //send the perms
            if (folder.PermissionCount > 0)
            {
                await this.SendFolderPermissionsAsync(folder, driver);
            }

            return result.CreatedNodeCount;
        }

        public async Task<int> ConnectFolderToParentAsync(Folder folder, IDriver driver)
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("UNWIND $Properties AS prop");
            builder.AppendLine(" MERGE (folder:" + Types.Folder + " {path:prop.path})");
            builder.AppendLine(" WITH folder, prop");
            builder.AppendLine(" MERGE (lastfolder:" + Types.Folder + " {path:prop.lastfolder})");

            if (folder.InheritanceDisabled) { builder.AppendLine(" MERGE (lastfolder) -[r:" + Types.BlockedInheritance + "]->(folder)"); }
            else { builder.AppendLine(" MERGE (lastfolder) -[r:" + Types.AppliesInhertitanceTo + "]->(folder)"); }

            builder.AppendLine(" SET r.lastscan = $ScanID");
            builder.AppendLine(" SET r.fsid = prop.fsid");
            builder.AppendLine(" SET r.layout='mesh'");
            builder.AppendLine(" WITH folder");
            builder.AppendLine($" MATCH (:{Types.Folder})-[r]->(folder)");
            builder.AppendLine($" WHERE (type(r)='{Types.AppliesInhertitanceTo}' OR type(r)='{Types.BlockedInheritance}') AND r.lastscan <> $ScanID");
            builder.AppendLine(" DELETE r");
            builder.AppendLine(" RETURN folder.path");

            string query = builder.ToString();
            NeoQueryData querydata = new NeoQueryData();
            querydata.Properties = new List<object>();
            querydata.Properties.Add(new
            {
                path = folder.Path,
                lastfolder = folder.PermParent,
                fsid = this.FsID
            });

            TransactionResult<List<string>> result = new TransactionResult<List<string>>(await NeoWriter.RunQueryAsync(query, querydata, driver));

            return result.CreatedEdgeCount;
        }

        public async Task<int> SendFolderPermissionsAsync(Folder folder, IDriver driver)
        {
            string query =

            "UNWIND $Properties AS prop" +
            " MERGE (folderDom:" + Types.Folder + " {path:prop.path})" +
            " WITH folderDom, prop" +
            " UNWIND prop.domainpermissions as domainperm" +
            " WITH folderDom,domainperm, prop" +
            " MERGE (ndom:" + Types.ADObject + " {id:domainperm.ID})" +
            " ON CREATE SET ndom:" + Types.Orphaned + ",ndom.lastscan = $ScanID" +
            " MERGE (ndom)-[r:" + Types.AppliesPermissionTo + "]->(folderDom)" +
            " SET r.right=domainperm.Right" +
            " SET r.accesstype=domainperm.AccessType" +
            " SET r.lastscan=$ScanID" +
            " SET r.fsid=prop.fsid" +
            " SET r.layout='mesh' " +

            " WITH folderDom, prop" +
            " UNWIND prop.builtinperms as builtinperm" +
            " MERGE (folderBuiltin:" + Types.Folder + " {path:prop.path})" +
            " MERGE (nbuiltin:" + Types.BuiltinObject + " {id:builtinperm.ID})" +
            " MERGE (nbuiltin)-[r:" + Types.AppliesPermissionTo + "]->(folderBuiltin)" +
            " SET r.right=builtinperm.Right" +
            " SET r.accesstype=builtinperm.AccessType" +
            " SET r.lastscan=$ScanID" +
            " SET r.fsid=prop.fsid" +
            " SET r.layout='mesh' " +

            " WITH folderDom, folderBuiltin, prop" +
            " MATCH ()-[rdom:" + Types.AppliesPermissionTo + "]->(folderDom)" +
            " WHERE rdom.lastscan <> $ScanID" +
            " DELETE rdom" +

            " WITH folderDom, folderBuiltin, prop" +
            " MATCH ()-[rbuiltin:" + Types.AppliesPermissionTo + "]->(folderBuiltin)" +
            " WHERE rbuiltin.lastscan <> $ScanID" +
            " DELETE rbuiltin" +

            " RETURN prop.path";


            NeoQueryData querydata = new NeoQueryData();
            querydata.Properties = new List<object>();
            querydata.Properties.Add(new
            {
                fsid = this.FsID,
                domainpermissions = folder.DomainPermissions,
                builtinperms = folder.BuiltinPermissions,
                path = folder.Path
            });

            TransactionResult<List<string>> result = new TransactionResult<List<string>>(await NeoWriter.RunQueryAsync(query, querydata, driver));

            return result.CreatedEdgeCount;
        }

        public async Task<int> SendDatastoreAsync(DataStore ds, IDriver driver)
        {
            string query = "UNWIND $Properties AS prop" +
                " MERGE (n:" + Types.Datastore + " {name:prop.Name})" +
                " SET n.comment=prop.Comment" +
                " SET n.host=prop.Host" +
                " MERGE(host:" + Types.Device + " {name:prop.Host})" +
                " MERGE (n)-[r:" + Types.ConnectedTo + "]->(host)" +
                " SET r.lastscan=$ScanID" +
                " SET r.layout='tree' " +
                " RETURN n";

            NeoQueryData querydata = new NeoQueryData();
            querydata.Properties = new List<object>();
            querydata.Properties.Add(ds);

            TransactionResult<List<string>> result = new TransactionResult<List<string>>(await NeoWriter.RunQueryAsync(query, querydata, driver));

            return result.CreatedEdgeCount;
        }

        public async Task<int> AttachRootToDataStoreAsync(DataStore ds, string rootpath, IDriver driver)
        {
            string query = "UNWIND $Properties AS prop" +
                " MERGE (datastore:" + Types.Datastore + " {name:prop.dsname})" +
                " MERGE (root:" + Types.Folder + " {path:prop.rootpath})" +
                " MERGE (datastore)-[r:" + Types.Hosts + "]->(root)" +
                " SET r.lastscan=$ScanID" +
                " SET r.layout='tree' " +
                " RETURN *";

            NeoQueryData querydata = new NeoQueryData();
            querydata.Properties = new List<object>();
            querydata.Properties.Add(new
            {
                dsname = ds.Name,
                rootpath = rootpath?.ToLower(),
            });

            TransactionResult<List<string>> result = new TransactionResult<List<string>>(await NeoWriter.RunQueryAsync(query, querydata, driver));
            return result.CreatedEdgeCount;
        }

        public async Task<int> CleanupChangedFoldersAsync(string rootpath, IDriver driver)
        {
            string query = "UNWIND $Properties AS prop" +
                " MATCH (f:" + Types.Folder + ")" +
                " WHERE f.fsid = prop.fsid AND f.lastscan<>$ScanID" +
                " DETACH DELETE f";

            NeoQueryData querydata = new NeoQueryData();
            querydata.Properties = new List<object>();
            querydata.Properties.Add(new
            {
                rootpath,
                fsid = this.FsID
            });

            TransactionResult<List<string>> result = new TransactionResult<List<string>>(await NeoWriter.RunQueryAsync(query, querydata, driver));
            return result.DeletedNodeCount;
        }

        public async Task<int> CleanupConnectionsAsync(string rootpath, IDriver driver)
        {
            string query = "UNWIND $Properties AS prop" +
                " MATCH (:" + Types.Folder + " {fsid: prop.fsid})-[r]-()" +
                " WHERE r.fsid = prop.fsid AND r.lastscan <> $ScanID" +
                " DELETE r ";

            NeoQueryData querydata = new NeoQueryData();
            querydata.Properties = new List<object>();
            querydata.Properties.Add(new
            {
                rootpath = rootpath,
                fsid = this.FsID
            });

            TransactionResult<List<string>> result = new TransactionResult<List<string>>(await NeoWriter.RunQueryAsync(query, querydata, driver));
            return result.DeletedNodeCount;
        }
    }
}
