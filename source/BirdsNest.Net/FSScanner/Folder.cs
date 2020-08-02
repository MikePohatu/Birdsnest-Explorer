#region license
// Copyright (c) 2019-2020 "20Road"
// 20Road Limited [https://20road.com]
//
// This file is part of BirdsNest.
//
// BirdsNest is free software: you can redistribute it and/or modify
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
using System.Collections.Generic;
using System.Security.AccessControl;
using Newtonsoft.Json;
using common;
using System.Security.Principal;

namespace FSScanner
{
    public class Folder
    {
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

        private string _path;
        [JsonProperty("path")]
        public string Path
        {
            get { return this._path; }
            set { this._path = value.ToLower(); }
        }
        public List<Permission> DomainPermissions { get; } = new List<Permission>();
        public List<Permission> BuiltinPermissions { get; } = new List<Permission>();

        public int PermissionCount { get { return this.DomainPermissions.Count + this.BuiltinPermissions.Count; } }

        public Folder() { }

        public Folder(string name, string path, string permparent, AuthorizationRuleCollection rules, bool isinheritancedisabled)
        {
            if (string.IsNullOrEmpty(name)) { this.Name = path; }
            else  { this.Name = name; }

            //this.ScanId = scanid;
            this.InheritanceDisabled = isinheritancedisabled;
            this.Path = path;
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
                        Path = this._path,
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
