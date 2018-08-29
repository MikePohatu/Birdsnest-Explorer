using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Diagnostics;
using Neo4j.Driver.V1;
using common;

namespace FSScanner
{
    public class Crawler
    {
        private Dictionary<string, Folder> _existingfolders;
        private Stopwatch _timer = new Stopwatch();
        private Writer _writer;
        private string _scanid = ShortGuid.NewGuid().ToString();
        private readonly IDriver _driver;

        public int FolderCount { get; private set; }

        public Crawler(IDriver driver)
        {
            this._driver = driver;
            this._writer = new Writer();
        }

        /// <summary>
        /// Start a crawl from a root folder, authenticating as another user
        /// </summary>
        /// <param name="ds"></param>
        /// <param name="rootpath"></param>
        /// <param name="cred"></param>
        /// <param name="driver"></param>
        public void CrawlRoot(DataStore ds, string rootpath, NetworkCredential cred)
        {

            //create a connection to the root path using other credentials
            try
            {
                NetworkConnection netcred = new NetworkConnection(rootpath, cred);
            }
            catch (Exception e)
            {
                ConsoleWriter.WriteError("Error connecting to " + rootpath + " with " + cred.UserName + ": " + e.Message);
                return;
            }

            //now initiate the crawl
            this.CrawlRoot(ds, rootpath);
        }

        /// <summary>
        /// Start a crawl from a root folder, authenticating as the process user
        /// </summary>
        /// <param name="ds"></param>
        /// <param name="rootpath"></param>
        /// <param name="driver"></param>
        public void CrawlRoot(DataStore ds, string rootpath)
        {
            ConsoleWriter.WriteInfo("Crawling " + rootpath);

            //get the existing folders for comparison
            try
            {
                using (ISession session = this._driver.Session())
                {
                    TransactionResult<Dictionary<string, Folder>> existfolderstx = Reader.GetAllFoldersAsDict(rootpath, session);
                    this._existingfolders = existfolderstx.Result;
                    ConsoleWriter.WriteInfo("Found " + this._existingfolders.Count + " folders in database in " + existfolderstx.ElapsedMilliseconds.TotalMilliseconds + "ms");
                }
            }
            catch (Exception e)
            {
                ConsoleWriter.WriteError("Error reading existing folders from database " + rootpath + ": " + e.Message);
                return;
            }

            //create the datastore node
            try
            {
                _timer.Start();
                using (ISession session = this._driver.Session())
                {
                    _writer.SendDatastore(ds, this._scanid, session);
                    _writer.AttachRootToDataStore(ds, rootpath.ToLower(), this._scanid, session);
                }
                    
            }
            catch (Exception e)
            {
                ConsoleWriter.WriteError("Error adding datastore " + ds.Name + ": " + e.Message);
                return;
            }

            //start at root and recurse down
            try
            {
                Crawl(rootpath, null, true);
                using (ISession session = this._driver.Session())
                {
                    _writer.FlushFolderQueue(session);
                }
                _timer.Stop();
                ConsoleWriter.ClearProgress();
                ConsoleWriter.WriteInfo("Crawled file system " + rootpath + " in " + _timer.Elapsed);
                ConsoleWriter.WriteLine();
            }
            catch (Exception e)
            {
                using (ISession session = this._driver.Session())
                {
                    _writer.FlushFolderQueue(session);
                }
                _timer.Stop();
                ConsoleWriter.WriteError("Error crawling file system " + rootpath + ": " + e.Message);
                ConsoleWriter.WriteLine();
                return;
            }

            //cleanup folders that have changed
            try
            {
                using (ISession session = this._driver.Session())
                {
                    _writer.CleanupChangedFolders(rootpath, this._scanid, session);
                    _writer.CleanupInheritanceMappings(rootpath, this._scanid, session);
                }
            }
            catch (Exception e)
            {
                ConsoleWriter.ClearProgress();
                ConsoleWriter.WriteError("Error cleaning up folders " + rootpath + ": " + e.Message);
                ConsoleWriter.WriteLine();
            }

            ConsoleWriter.WriteInfo("Found " + _writer.FolderCount + " folders with permissions applied");
        }

        /// <summary>
        /// Crawl subdirectories recursively. permparent is the last parent folder that had new permissions set
        /// </summary>
        /// <param name="path"></param>
        /// <param name="permparent"></param>
        private void Crawl(string path, string permparent, bool isroot)
        {
            string newpermparent = permparent;
            try
            {
                Folder f = QueryFolder(new DirectoryInfo(path), permparent, isroot);
                if ((f.Permissions.Count > 0) || isroot) {
                    newpermparent = path;
                    using (ISession session = this._driver.Session())
                    {
                        _writer.UpdateFolder(f, session);
                    }
                }
            }
            catch (Exception e)
            {
                ConsoleWriter.WriteError("Error connecting to " + path + ": " + e.Message);
                Folder f = new Folder() { Blocked = true, Path = path, Name = path, PermParent = permparent, InheritanceDisabled=true, ScanId = this._scanid };
                using (ISession session = this._driver.Session())
                {
                    _writer.UpdateFolder(f, session);
                }
                return;
            }

            try
            {
                foreach (string subdirpath in Directory.EnumerateDirectories(path))
                {
                    this.Crawl(subdirpath, newpermparent, false);
                }
            }
            catch (Exception e)
            {
                ConsoleWriter.WriteError("Error encountered while enumerating directories in " + path + ": " + e.Message);
            }
            
        }

        /// <summary>
        /// Query a folder in the file system and return a Folder object with the details
        /// </summary>
        /// <param name="directory"></param>
        /// <param name="permroot"></param>
        /// <param name="isroot"></param>
        /// <returns></returns>
        private Folder QueryFolder(DirectoryInfo directory, string permroot, bool isroot)
        {
            this.FolderCount++;
            if (this.FolderCount % 10 == 0 )
            {
                ConsoleWriter.WriteProgress(directory.FullName);
            }

            DirectorySecurity dirsec = directory.GetAccessControl();
            AuthorizationRuleCollection directrules = dirsec.GetAccessRules(true, isroot, typeof(SecurityIdentifier));

            Folder f = new Folder(directory.Name, directory.FullName, permroot, directrules, dirsec.AreAccessRulesProtected, this._scanid);
            return f;          
        }
    }
}
