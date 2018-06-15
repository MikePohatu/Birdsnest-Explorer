using System.Collections.Generic;
using System.DirectoryServices;
using ADScanner.Neo4j;

namespace ADScanner.ActiveDirectory
{
    public class ADUser : ADGroupMember
    {
        public override string SubLabel { get { return "User"; } }

        public ADUser(SearchResult result) : base(result) { }
    }
}
