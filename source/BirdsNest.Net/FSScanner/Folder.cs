using System;
using System.Collections.Generic;
using System.Security.AccessControl;
using Newtonsoft.Json;
using common;

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

        [JsonProperty("scanid")]
        public string ScanId { get; set; }

        public virtual string Type { get { return Types.Folder; } }
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
        public List<Permission> Permissions { get; } = new List<Permission>();

        public Folder() { }

        public Folder(string name, string path, string permparent, AuthorizationRuleCollection rules, bool isinheritancedisabled, string scanid)
        {
            if (string.IsNullOrEmpty(name)) { this.Name = path; }
            else  { this.Name = name; }

            this.ScanId = scanid;
            this.InheritanceDisabled = isinheritancedisabled;
            this.Path = path;
            this.PermParent = permparent;
            if (rules?.Count > 0)
            {
                ConsoleWriter.WriteInfo("Permissions set: " + path);
                foreach (FileSystemAccessRule rule in rules)
                {
                    this.Permissions.Add(new Permission() {
                        ID = rule.IdentityReference.Value,
                        Path = this._path,
                        Right = rule.FileSystemRights.ToString(),
                        ScanId = scanid
                    });
                    //Console.WriteLine("{0} | Account: {1} | Permission: {2}", path, rule.IdentityReference.Value, rule.FileSystemRights.ToString());
                }
            }
        }
    }
}
