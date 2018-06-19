using System.Collections.Generic;
using System.DirectoryServices;

namespace ADScanner.ActiveDirectory
{
    public abstract class ADGroupMemberObject:ADObject
    {
        public List<string> MemberOfDNs { get; protected set; }
        public string SamAccountName { get; protected set; }

        public ADGroupMemberObject(SearchResult result):base(result)
        {
            this.Name = ADSearchResultConverter.GetSinglestringValue(result, "name");
            this.ID = ADSearchResultConverter.GetSidAsString(result);
            this.MemberOfDNs = ADSearchResultConverter.GetStringList(result, "memberof");
            this.SamAccountName = ADSearchResultConverter.GetSinglestringValue(result, "samaccountname");

            this.Properties.Add("samaccountname", this.SamAccountName);
        }
    }
}
