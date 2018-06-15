using System.Collections.Generic;
using System.DirectoryServices;
using neo4jlink;

namespace ADScanner.ActiveDirectory
{
    internal class ADUser:INode
    {
        public string Name { get; private set; }
        public string Label { get { return "AD_Object"; } }
        public string SubLabel { get { return "AD_User"; } }
        public string ID { get; private set; }
        public string Path { get; private set; }
        public List<KeyValuePair<string, object>> Properties { get; private set; }
        public List<string> MemberOfDNs { get; private set; }

        public ADUser(SearchResult result)
        {
            this.Name = ADSearchResultConverter.GetSinglestringValue(result, "samaccountname");
            this.ID = ADSearchResultConverter.GetSidAsString(result);
            this.Path = ADSearchResultConverter.GetSinglestringValue(result, "distinguishedName");
            this.MemberOfDNs = ADSearchResultConverter.GetStringList(result, "memberOf");

            this.Properties = new List<KeyValuePair<string, object>>();
            this.Properties.Add(new KeyValuePair<string, object>("distinguishedName", ADSearchResultConverter.GetSinglestringValue(result, "distinguishedName")));
        }
    }
}
