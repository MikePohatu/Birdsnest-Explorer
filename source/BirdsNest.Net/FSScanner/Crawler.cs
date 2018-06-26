using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Diagnostics;
using Neo4j.Driver.V1;

namespace FSScanner
{
    public class Crawler
    {
        private Stopwatch timer = new Stopwatch();
        private Writer writer;

        public Crawler(ISession session)
        {
            this.writer = new Writer(session);
        }


        public void Crawl(DataStore ds, string rootpath, NetworkCredential cred, ISession session)
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
            this.Crawl(ds, rootpath, session);
        }


        public void Crawl(DataStore ds, string rootpath, ISession session)
        {
            Console.WriteLine(rootpath);

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
                QueryFolder(rootpath, string.Empty, true);
                CrawlChildren(rootpath, null);
                writer.FlushFolderQueue();
                timer.Stop();
                Console.WriteLine("Crawled file system " + rootpath + " in " + timer.Elapsed);
                //Console.WriteLine("Crawled file system " + rootpath + " in " + timer.Elapsed);
            }
            catch (Exception e)
            {
                writer.FlushFolderQueue();
                timer.Stop();
                Console.WriteLine("Error crawling file system " + rootpath + ": " + e.Message);
                return;
            }

            Console.WriteLine("Found " + writer.FolderCount + " folders with permissions applied");
        }

        /// <summary>
        /// Crawl subdirectories recursively. permparent is the last parent folder that had new permissions set
        /// </summary>
        /// <param name="path"></param>
        /// <param name="permparent"></param>
        private void CrawlChildren(string path, string permparent)
        {
            string newpermparent = permparent;
            try
            {
                if (QueryFolder(path, permparent, false)==true) { newpermparent = path; }
            }
            catch (Exception e)
            {
                BlockedFolder f = new BlockedFolder(path, newpermparent);
                writer.QueueFolder(f);
                Console.WriteLine("Unable to connect to " + path, e.Message);
                return;
            }

            foreach (string subdirpath in Directory.EnumerateDirectories(path))
            {
                this.CrawlChildren(subdirpath, newpermparent);
            }
        }

        private bool QueryFolder(string path, string permroot, bool isroot)
        {
            //Console.WriteLine("Query path: " + path);
            DirectorySecurity dirsec = Directory.GetAccessControl(path);
            AuthorizationRuleCollection directrules = dirsec.GetAccessRules(true, isroot, typeof(SecurityIdentifier));
            if (directrules.Count > 0)
            {
                Folder f = new Folder(path, permroot, directrules, dirsec.AreAccessRulesProtected);
                writer.QueueFolder(f);
                return true;
            }
            else
            { return false; }           
        }
    }
}
