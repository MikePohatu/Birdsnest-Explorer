using System.DirectoryServices;

namespace ADScanner.ActiveDirectory
{
    public class ADComputer: ADGroupMemberObject
    {
        public override string SubLabel { get { return "Computer"; } }

        public ADComputer(SearchResult result):base(result) { }
    }
}
