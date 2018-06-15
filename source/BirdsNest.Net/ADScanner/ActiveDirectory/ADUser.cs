using System.DirectoryServices;
using System.Collections.Generic;

namespace ADScanner.ActiveDirectory
{
    public class ADUser : ADGroupMemberObject
    {
        public override string SubLabel { get { return "User"; } }

        public ADUser(SearchResult result) : base(result)
        {
            this.Properties.Add(new KeyValuePair<string, object>("displayName", ADSearchResultConverter.GetSinglestringValue(result, "displayName")));
        }
    }
}
