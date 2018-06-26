using System;
using System.Collections.Generic;
using System.Security.AccessControl;

namespace FSScanner
{
    public class Folder
    {
        public bool InheritanceDisabled { get; private set; }

        public virtual string Type { get { return Types.Folder; } }
        private string _permparent = string.Empty;
        public string PermParent
        {
            get { return this._permparent; }
            set { this._permparent = value?.ToLower(); }
        } 

        private string _path;
        public string Path
        {
            get { return this._path; }
            set { this._path = value.ToLower(); }
        }
        public List<Permission> Permissions { get; } = new List<Permission>();

        public Folder(string path, string permparent, AuthorizationRuleCollection rules, bool isinheritancedisabled)
        {
            this.InheritanceDisabled = isinheritancedisabled;
            this.Path = path;
            this.PermParent = permparent;
            if (rules?.Count > 0)
            {
                foreach (FileSystemAccessRule rule in rules)
                {
                    this.Permissions.Add( new Permission(){
                        ID = rule.IdentityReference.Value,
                        Path =this._path,
                        Right =rule.FileSystemRights.ToString() });
                    Console.WriteLine("{0} | Account: {1} | Permission: {2}", path, rule.IdentityReference.Value, rule.FileSystemRights.ToString());
                }
            }
        }
    }
}
