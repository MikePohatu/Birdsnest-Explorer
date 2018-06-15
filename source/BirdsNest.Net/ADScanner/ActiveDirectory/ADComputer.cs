using System.Collections.Generic;
using System.DirectoryServices;
using ADScanner.Neo4j;

namespace ADScanner.ActiveDirectory
{
    public class ADComputer: ADGroupMember
    {
        public override string SubLabel { get { return "Computer"; } }

        public ADComputer(SearchResult result):base(result) { }
    }
}
