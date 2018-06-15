using System.DirectoryServices;

namespace ADScanner.ActiveDirectory
{
    public class ADUser : ADGroupMemberObject
    {
        public override string SubLabel { get { return "User"; } }

        public ADUser(SearchResult result) : base(result) { }
    }
}
