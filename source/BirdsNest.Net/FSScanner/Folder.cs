using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;

namespace FSScanner
{
    public class Folder
    {
        public virtual string Type { get { return Types.Folder; } }
        private string _permparent = string.Empty;
        public string PermParent
        {
            get { return this._permparent; }
            set { this._permparent = value.ToLower(); }
        } 

        private string _path;
        public string Path
        {
            get { return this._path; }
            set { this._path = value.ToLower(); }
        }
        public List<Permission> Permissions { get; } = new List<Permission>();

        public Folder(string path, string permparent, AuthorizationRuleCollection rules)
        {
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
