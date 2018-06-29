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
        private int _counter = 0;
        private string _scanid = ShortGuid.NewGuid().ToString();
        private readonly IDriver _driver;

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
                Console.WriteLine("Error connecting to " + rootpath + " with " + cred.UserName + ": " + e.Message);
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
            Console.WriteLine(rootpath);

            //get the existing folders for comparison
            try
            {
                using (ISession session = this._driver.Session())
                {
                    TransactionResult<Dictionary<string, Folder>> existfolderstx = Reader.GetAllFoldersAsDict(rootpath, session);
                    this._existingfolders = existfolderstx.Result;
                    Console.WriteLine("Found " + this._existingfolders.Count + " folders in database in " + existfolderstx.ElapsedMilliseconds.TotalMilliseconds + "ms");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Error reading existing folders from database " + rootpath + ": " + e.Message);
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
                Console.WriteLine("Error adding datastore " + ds.Name + ": " + e.Message);
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
                Console.WriteLine("Crawled file system " + rootpath + " in " + _timer.Elapsed);
                Console.WriteLine();
            }
            catch (Exception e)
            {
                using (ISession session = this._driver.Session())
                {
                    _writer.FlushFolderQueue(session);
                }
                _timer.Stop();
                Console.WriteLine("Error crawling file system " + rootpath + ": " + e.Message);
                Console.WriteLine();
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
                Console.WriteLine("Error cleaning up folders " + rootpath + ": " + e.Message);
                Console.WriteLine();
            }

            Console.WriteLine("Found " + _writer.FolderCount + " folders with permissions applied");
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
                Folder f = new Folder() { Blocked = true, Path = path, Name = path, PermParent = permparent, InheritanceDisabled=true, ScanId = this._scanid };
                using (ISession session = this._driver.Session())
                {
                    _writer.UpdateFolder(f, session);
                }
                Console.WriteLine("Unable to connect to " + path, e.Message);
                return;
            }

            foreach (string subdirpath in Directory.EnumerateDirectories(path))
            {
                this.Crawl(subdirpath, newpermparent, false);
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
            this._counter++;
            if (_counter == 100 )
            {
                Console.WriteLine("  Progress: " + directory.FullName);
                _counter = 0;
            }

            DirectorySecurity dirsec = directory.GetAccessControl();
            AuthorizationRuleCollection directrules = dirsec.GetAccessRules(true, isroot, typeof(SecurityIdentifier));

            Folder f = new Folder(directory.Name, directory.FullName, permroot, directrules, dirsec.AreAccessRulesProtected, this._scanid);
            return f;          
        }
    }
}
