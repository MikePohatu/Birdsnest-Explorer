using System.Collections.Generic;
using System.DirectoryServices;
using ADScanner.Neo4j;

namespace ADScanner.ActiveDirectory
{
    public class ADComputer:INode
    {
        public string Name { get; private set; }
        public string Label { get { return "AD_Object"; } }
        public string SubLabel { get { return "Computer"; } }
        public string ID { get; private set; }
        public string Path { get; private set; }
        public List<KeyValuePair<string, object>> Properties { get; private set; }
        public List<string> MemberOfDNs { get; private set; }

        public ADComputer(SearchResult result)
        {
            this.Name = ADSearchResultConverter.GetSinglestringValue(result, "name");
            this.ID = ADSearchResultConverter.GetSidAsString(result);
            this.Path = ADSearchResultConverter.GetSinglestringValue(result, "distinguishedName");
            this.MemberOfDNs = ADSearchResultConverter.GetStringList(result, "memberOf");

            this.Properties = new List<KeyValuePair<string, object>>();
            this.Properties.Add(new KeyValuePair<string, object>("distinguishedName", this.Path));
            this.Properties.Add(new KeyValuePair<string, object>("operatingSystem", ADSearchResultConverter.GetSinglestringValue(result, "operatingSystem")));
            this.Properties.Add(new KeyValuePair<string, object>("operatingSystemVersion", ADSearchResultConverter.GetSinglestringValue(result, "operatingSystemVersion")));
            this.Properties.Add(new KeyValuePair<string, object>("sAMAccountName", ADSearchResultConverter.GetSinglestringValue(result, "sAMAccountName")));
            this.Properties.Add(new KeyValuePair<string, object>("cn", ADSearchResultConverter.GetSinglestringValue(result, "cn")));
            this.Properties.Add(new KeyValuePair<string, object>("primaryGroupID", ADSearchResultConverter.GetSinglestringValue(result, "primaryGroupID")));
        }
    }
}
