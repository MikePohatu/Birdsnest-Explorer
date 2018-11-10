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
    public class CrawlerThreadWrapper
    {
        private Crawler _parent;

        public bool IsRoot { get; set; } = false;
        public int ThreadNumber { get; set; } = 1;
        public string PermParent { get; set; } = null;
        public string Path { get; set; } = null;
        public bool IsNewThread { get; set; } = false;

        public CrawlerThreadWrapper(Crawler parent)
        {
            this._parent = parent;
        }

        public void Crawl(object state)
        { this.Crawl(); }

        /// <summary>
        /// Crawl subdirectories recursively. permparent is the last parent folder that had new permissions set
        /// </summary>
        /// <param name="path"></param>
        /// <param name="permparent"></param>
        public void Crawl()
        {
            string newpermparent = this.PermParent;
            try
            {
                Folder f = this.QueryFolder(new DirectoryInfo(this.Path), this.PermParent, this.IsRoot);
                if ((f.Permissions.Count > 0) || this.IsRoot)
                {
                    newpermparent = this.Path;
                    this._parent.Writer.UpdateFolder(f, this._parent.Driver);
                }
            }
            catch (Exception e)
            {
                ConsoleWriter.WriteError("Error connecting to " + this.Path + ": " + e.Message);
                Folder f = new Folder() { Blocked = true, Path = this.Path, Name = this.Path, PermParent = this.PermParent, InheritanceDisabled = true, ScanId = this._parent.ScanId };
                this._parent.Writer.UpdateFolder(f, this._parent.Driver);
                return;
            }

            try
            {
                foreach (string subdirpath in Directory.EnumerateDirectories(this.Path))
                {
                    CrawlerThreadWrapper subwrapper = new CrawlerThreadWrapper(this._parent);
                    subwrapper.Path = subdirpath;
                    subwrapper.PermParent = newpermparent;
                    subwrapper.IsRoot = false;

                    int threadnum = ThreadCounter.RequestThread();

                    if (threadnum != -1)
                    {
                        subwrapper.ThreadNumber = threadnum;
                        subwrapper.IsNewThread = true;
                        ThreadPool.QueueUserWorkItem(subwrapper.Crawl);
                    }
                    else
                    {
                        subwrapper.ThreadNumber = this.ThreadNumber;
                        subwrapper.Crawl();
                    }
                }
            }
            catch (Exception e)
            {
                ConsoleWriter.WriteError("Error encountered while enumerating directories in " + this.Path + ": " + e.Message);
            }
            if (this.IsNewThread == true) { ThreadCounter.Decrement(); }
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
            this._parent.FolderCount++;
            ConsoleWriter.WriteProgress(this.ThreadNumber + " | " + directory.FullName, this.ThreadNumber);


            DirectorySecurity dirsec = directory.GetAccessControl();
            AuthorizationRuleCollection directrules = dirsec.GetAccessRules(true, isroot, typeof(SecurityIdentifier));

            Folder f = new Folder(directory.Name, directory.FullName, permroot, directrules, dirsec.AreAccessRulesProtected, this._parent.ScanId);
            return f;
        }
    }
}
