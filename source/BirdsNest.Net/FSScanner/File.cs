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
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Security.AccessControl;
using System.Security.Principal;

namespace FSScanner
{
    public class File
    {
        public virtual string Type { get; protected set; } = Types.File;

        [JsonProperty("inheritancedisabled")]
        public bool InheritanceDisabled { get; set; } = false;

        [JsonProperty("blocked")]
        public bool Blocked { get; set; } = false;

        [JsonProperty("name")]
        public string Name { get; set; } = string.Empty;

        private string _permparent = string.Empty;
        public string PermParent
        {
            get { return this._permparent; }
            set { this._permparent = value?.ToLower(); }
        }

        private string _path = string.Empty;
        [JsonProperty("path")]
        public string Path
        { get { return this._path; } set { this._path = value.ToLower(); } }

        /// <summary>
        /// Path of the file/folder as reported by the file system
        /// </summary>
        public string FullName { get; set; }

        public List<Permission> DomainPermissions { get; } = new List<Permission>();
        public List<Permission> BuiltinPermissions { get; } = new List<Permission>();

        public int Depth { get; set; } = 0;

        public int PermissionCount { get { return this.DomainPermissions.Count + this.BuiltinPermissions.Count; } }

        public File() { }

        public File(string name, string path, string permparent, AuthorizationRuleCollection rules, bool isinheritancedisabled)
        {
            if (string.IsNullOrEmpty(name)) { this.Name = path; }
            else { this.Name = name; }

            //this.ScanId = scanid;
            this.InheritanceDisabled = isinheritancedisabled;
            this.Path = path;
            this.FullName = path;
            this.PermParent = permparent;
            if (rules?.Count > 0)
            {
                ConsoleWriter.WriteInfo("Permissions set: " + path);
                foreach (FileSystemAccessRule rule in rules)
                {
                    SecurityIdentifier identifier = rule.IdentityReference as SecurityIdentifier;

                    Permission perm = new Permission()
                    {
                        ID = rule.IdentityReference.Value,
                        Path = this.Path,
                        Right = rule.FileSystemRights.ToString(),
                        AccessType = rule.AccessControlType.ToString()
                    };

                    if (identifier != null && identifier.AccountDomainSid == null)
                    {
                        this.BuiltinPermissions.Add(perm);
                    }
                    else
                    {
                        this.DomainPermissions.Add(perm);
                    }

                    //Console.WriteLine("{0} | Account: {1} | Permission: {2}", path, rule.IdentityReference.Value, rule.FileSystemRights.ToString());
                }
            }
        }
    }
}
