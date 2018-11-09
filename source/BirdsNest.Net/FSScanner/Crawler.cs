using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Security.AccessControl;
using System.Security.Principal;
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

        public int ThreadCount { get; set; } = 0;
        public int FolderCount { get; set; } = 0;
        public IDriver Driver { get; private set; }
        public Writer Writer { get; private set; }

        public bool IsThreadAvailable { get { return this.ThreadCount < this.MaxThreads ? true : false; } }

        private int _maxthreads = Environment.ProcessorCount * 2;
        public int MaxThreads {
            get
            {
                return this._maxthreads;
            }
            set
            {
                if ((value >= Environment.ProcessorCount) && (value <=20))
                {
                    this._maxthreads = value;
                }
            }
        }

        public Crawler(IDriver driver)
        {
            this.Driver = driver;
            this.Writer = new Writer();
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
            ThreadPool.SetMaxThreads(this.MaxThreads, this.MaxThreads);
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
                using (ISession session = this.Driver.Session())
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
                using (ISession session = this.Driver.Session())
                {
                    Writer.SendDatastore(ds, this.ScanId, session);
                    Writer.AttachRootToDataStore(ds, rootpath.ToLower(), this.ScanId, session);
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
                //Crawl(rootpath, null, true);
                CrawlerThreadWrapper threadwrapper = new CrawlerThreadWrapper(this);
                threadwrapper.IsRoot = true;
                threadwrapper.Path = rootpath;
                threadwrapper.PermParent = null;

                using (var countdownEvent = new CountdownEvent(this.MaxThreads))
                {
                    threadwrapper.Crawl();
                    while (true)
                    {
                        Thread.Sleep(1000);
                        if (this.ThreadCount == 0 ) { break; }
                    }
                }

                using (ISession session = this.Driver.Session())
                {
                    Writer.FlushFolderQueue(session);
                }
                _timer.Stop();
                ConsoleWriter.ClearProgress();
                ConsoleWriter.WriteInfo("Crawled file system " + rootpath + " in " + _timer.Elapsed);
                ConsoleWriter.WriteLine();
            }
            catch (Exception e)
            {
                using (ISession session = this.Driver.Session())
                {
                    Writer.FlushFolderQueue(session);
                }
                _timer.Stop();
                ConsoleWriter.WriteError("Error crawling file system " + rootpath + ": " + e.Message);
                ConsoleWriter.WriteLine();
                return;
            }

            //cleanup folders that have changed
            try
            {
                using (ISession session = this.Driver.Session())
                {
                    Writer.CleanupChangedFolders(rootpath, this.ScanId, session);
                    Writer.CleanupInheritanceMappings(rootpath, this.ScanId, session);
                }
            }
            catch (Exception e)
            {
                ConsoleWriter.ClearProgress();
                ConsoleWriter.WriteError("Error cleaning up folders " + rootpath + ": " + e.Message);
                ConsoleWriter.WriteLine();
            }

            ConsoleWriter.WriteInfo("Found " + Writer.FolderCount + " folders with permissions applied");
        }
    }
}
