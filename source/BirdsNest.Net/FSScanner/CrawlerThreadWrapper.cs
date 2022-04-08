﻿#region license
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
using System;
using System.IO;
using System.Security.AccessControl;
using System.Security.Principal;
using common;
using System.Threading;
using System.Threading.Tasks;

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

        public int Depth { get; private set; } = 0;

        public CrawlerThreadWrapper(Crawler parent, int depth)
        {
            this._parent = parent;
            this.Depth = depth;
        }

        public async Task CrawlAsync(object state)
        { await this.CrawlAsync(); }

        public void Crawl(object state)
        { this.CrawlAsync().Wait(); }

        /// <summary>
        /// Crawl subdirectories recursively. permparent is the last parent folder that had new permissions set
        /// </summary>
        /// <param name="path"></param>
        /// <param name="permparent"></param>
        public async Task CrawlAsync()
        {
            bool connected = false;
            string newpermparent = this.PermParent;
            try
            {
                Folder f = this.QueryFolder(new DirectoryInfo(this.Path), this.PermParent, this.IsRoot);
                if ((f.PermissionCount > 0) || this.IsRoot)
                {
                    newpermparent = this.Path;
                    await this._parent.Writer.UpdateFolderAsync(f, this._parent.Driver);
                }

                connected = true;
            }
            catch (Exception e)
            {
                ConsoleWriter.WriteError("Error connecting to " + this.Path + ": " + e.Message);
                Folder f = new Folder() {
                    Blocked = true,
                    Path = this.Path,
                    Name = this.Path,
                    PermParent = this.PermParent,
                    InheritanceDisabled = true,
                    Depth = this.Depth
                };
                await this._parent.Writer.UpdateFolderAsync(f, this._parent.Driver);
            }

            if (connected == true)
            {
                try
                {
                    foreach (string subdirpath in Directory.EnumerateDirectories(this.Path))
                    {
                        CrawlerThreadWrapper subwrapper = new CrawlerThreadWrapper(this._parent, this.Depth + 1);
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
                            await subwrapper.CrawlAsync();
                        }
                    }
                }
                catch (Exception e)
                {
                    ConsoleWriter.WriteError("Error encountered while enumerating directories in " + this.Path + ": " + e.Message);
                }
            }

            if (this.IsNewThread == true)
            {
                ThreadCounter.Release(this.ThreadNumber);
                ConsoleWriter.WriteProgress(this.ThreadNumber + " | " + "Released thread", this.ThreadNumber);
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
            this._parent.FolderCount++;
            ConsoleWriter.WriteProgress(this.ThreadNumber + " | " + directory.FullName, this.ThreadNumber);


            DirectorySecurity dirsec = directory.GetAccessControl();
            AuthorizationRuleCollection directrules = dirsec.GetAccessRules(true, isroot, typeof(SecurityIdentifier));

            Folder f = new Folder(directory.Name, directory.FullName, permroot, directrules, dirsec.AreAccessRulesProtected);
            f.Depth = this.Depth;
            return f;
        }
    }
}
