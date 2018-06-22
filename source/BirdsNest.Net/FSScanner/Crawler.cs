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

        public void Crawl(string rootpath, NetworkCredential cred, ISession session)
        {
            Console.WriteLine(rootpath);

            //start at root
            try
            {
                timer.Start();
                NetworkConnection netcred = new NetworkConnection(rootpath, cred);
                QueryFolder(rootpath, string.Empty, true);
                
            }
            catch(Exception e)
            {
                Console.WriteLine("Error connecting to " + rootpath + " with " + cred.UserName + ": " + e.Message);
                return;
            }

            //recurse down
            try
            {
                CrawlChildren(rootpath, rootpath);
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

            //now send the perm mappings
            try
            {
                timer.Restart();
                writer.SendPermissions();
                timer.Stop();
                Console.WriteLine("Sent permission mappings for " + rootpath + " in " + timer.ElapsedMilliseconds + "ms");
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
            AuthorizationRuleCollection rules = dirsec.GetAccessRules(true, isroot, typeof(SecurityIdentifier));
            if (rules.Count > 0)
            {
                Folder f = new Folder(path, permroot, rules);
                writer.QueueFolder(f);
                return true;
            }
            else
            { return false; }           
        }
    }
}
