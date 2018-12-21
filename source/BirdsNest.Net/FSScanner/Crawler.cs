using System;
using System.Collections.Generic;
using System.Net;
using System.Diagnostics;
using Neo4j.Driver.V1;
using common;
using System.Threading;

namespace FSScanner
{
    public class Crawler
    {
        private Dictionary<string, Folder> _existingfolders;
        private Stopwatch _timer = new Stopwatch();
        public string ScanId { get; private set; } = ShortGuid.NewGuid().ToString();
        public string FsID { get; set; }

        public int FolderCount { get; set; } = 0;
        public IDriver Driver { get; private set; }
        public Writer Writer { get; private set; }

        public Crawler(IDriver driver, string fsid)
        {
            this.FsID = fsid;
            this.Driver = driver;
            this.Writer = new Writer(fsid);
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
                TransactionResult<Dictionary<string, Folder>> existfolderstx = Reader.GetAllFoldersAsDict(rootpath, this.Driver);
                this._existingfolders = existfolderstx.Result;
                ConsoleWriter.WriteInfo("Found " + this._existingfolders.Count + " folders in database in " + existfolderstx.ElapsedMilliseconds.TotalMilliseconds + "ms");
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
                Writer.SendDatastore(ds, this.ScanId, this.Driver);
                Writer.AttachRootToDataStore(ds, rootpath.ToLower(), this.ScanId, this.Driver);                    
            }
            catch (Exception e)
            {
                ConsoleWriter.WriteError("Error adding datastore " + ds.Name + ": " + e.Message);
                return;
            }

            //start at root and recurse down
            try
            {
                CrawlerThreadWrapper threadwrapper = new CrawlerThreadWrapper(this);
                threadwrapper.IsRoot = true;
                threadwrapper.Path = rootpath;
                threadwrapper.PermParent = null;

                //start a new thread for the crawl
                int threadnum = ThreadCounter.RequestThread();
                threadwrapper.ThreadNumber = threadnum;
                threadwrapper.IsNewThread = true;
                ThreadPool.QueueUserWorkItem(threadwrapper.Crawl);

                //wait for threads to finish
                while (true)
                {
                    Thread.Sleep(5000);
                    if (ThreadCounter.ActiveThreadCount == 0 ) { break; }
                }

                Writer.FlushFolderQueue(this.Driver);
                _timer.Stop();
                ConsoleWriter.ClearProgress();
                ConsoleWriter.WriteInfo("Crawled file system " + rootpath + " in " + _timer.Elapsed);
                ConsoleWriter.WriteLine();
            }
            catch (Exception e)
            {
                Writer.FlushFolderQueue(this.Driver);
                _timer.Stop();
                ConsoleWriter.WriteError("Error crawling file system " + rootpath + ": " + e.Message);
                ConsoleWriter.WriteLine();
                return;
            }

            //cleanup folders that have changed
            try
            {
                _timer.Restart();
                ConsoleWriter.WriteInfo("Cleaning up ");
                Writer.CleanupChangedFolders(rootpath, this.ScanId, this.Driver);
                Writer.CleanupInheritanceMappings(rootpath, this.ScanId, this.Driver);
                _timer.Stop();
                ConsoleWriter.WriteInfo("Clean finished in " + _timer.Elapsed);
                ConsoleWriter.WriteLine();
            }
            catch (Exception e)
            {
                _timer.Stop();
                ConsoleWriter.ClearProgress();
                ConsoleWriter.WriteError("Error cleaning up folders " + rootpath + ": " + e.Message);
                ConsoleWriter.WriteLine();
            }

            ConsoleWriter.WriteInfo("Found " + Writer.FolderCount + " folders with permissions applied");
        }
    }
}
