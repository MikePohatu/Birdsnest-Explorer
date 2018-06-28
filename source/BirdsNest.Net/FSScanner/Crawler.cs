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
        private Dictionary<string, Folder> existingfolders;
        private Stopwatch timer = new Stopwatch();
        private Writer writer;
        private int counter = 0;

        public Crawler(ISession session)
        {
            this.writer = new Writer(session);
        }


        public void CrawlRoot(DataStore ds, string rootpath, NetworkCredential cred, IDriver driver)
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
            this.CrawlRoot(ds, rootpath, driver);
        }


        public void CrawlRoot(DataStore ds, string rootpath, IDriver driver)
        {
            Console.WriteLine(rootpath);

            //get the existing folders for comparison
            try
            {
                using (ISession session = driver.Session())
                {
                    TransactionResult<Dictionary<string, Folder>> existfolderstx = Reader.GetAllFoldersAsDict(rootpath, session);
                    this.existingfolders = existfolderstx.Result;
                    Console.WriteLine("Found " + this.existingfolders.Count + " folders in database in " + existfolderstx.ElapsedMilliseconds.TotalMilliseconds + "ms");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Error reading existing folders from database " + rootpath + ": " + e.Message);
                return;
            }

            //created the datastore node
            try
            {
                timer.Start();
                writer.SendDatastore(ds);
                writer.AttachRootToDataStore(ds, rootpath);
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
                writer.FlushFolderQueue();
                timer.Stop();
                Console.WriteLine("Crawled file system " + rootpath + " in " + timer.Elapsed);
                Console.WriteLine();
                //Console.WriteLine("Crawled file system " + rootpath + " in " + timer.Elapsed);
            }
            catch (Exception e)
            {
                writer.FlushFolderQueue();
                timer.Stop();
                Console.WriteLine("Error crawling file system " + rootpath + ": " + e.Message);
                Console.WriteLine();
                return;
            }

            Console.WriteLine("Found " + writer.FolderCount + " folders with permissions applied");
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
                    writer.QueueFolder(f);
                }
            }
            catch (Exception e)
            {
                Folder f = new Folder() { Blocked = true, Path = path, Name = path, PermParent = permparent, InheritanceDisabled=true };
                writer.QueueFolder(f);
                Console.WriteLine("Unable to connect to " + path, e.Message);
                return;
            }

            foreach (string subdirpath in Directory.EnumerateDirectories(path))
            {
                this.Crawl(subdirpath, newpermparent, false);
            }
        }

        private Folder QueryFolder(DirectoryInfo directory, string permroot, bool isroot)
        {
            this.counter++;
            if (counter == 100 )
            {
                Console.WriteLine("Progress: " + directory.FullName);
                counter = 0;
            }
            
            DirectorySecurity dirsec = directory.GetAccessControl();
            AuthorizationRuleCollection directrules = dirsec.GetAccessRules(true, isroot, typeof(SecurityIdentifier));

            Folder f = new Folder(directory.Name, directory.FullName, permroot, directrules, dirsec.AreAccessRulesProtected);
            return f;          
        }
    }
}
