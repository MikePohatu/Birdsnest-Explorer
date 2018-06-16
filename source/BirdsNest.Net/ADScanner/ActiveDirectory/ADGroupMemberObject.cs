using System.Collections.Generic;
using System.DirectoryServices;
using ADScanner.Neo4j;

namespace ADScanner.ActiveDirectory
{
    public abstract class ADGroupMemberObject:INode
    {
        public string Name { get; private set; }
        public virtual string Label { get { return "AD_Object"; } }
        public string ID { get; private set; }
        public string Path { get; private set; }
        

        public List<KeyValuePair<string, object>> Properties { get; protected set; }
        public List<string> MemberOfDNs { get; protected set; }

        public ADGroupMemberObject(SearchResult result)
        {
            this.Name = ADSearchResultConverter.GetSinglestringValue(result, "name");
            this.ID = ADSearchResultConverter.GetSidAsString(result);
            this.Path = ADSearchResultConverter.GetSinglestringValue(result, "distinguishedName");
            this.MemberOfDNs = ADSearchResultConverter.GetStringList(result, "memberOf");

            this.Properties = new List<KeyValuePair<string, object>>();
            this.Properties.Add(new KeyValuePair<string, object>("distinguishedName", this.Path));
            this.Properties.Add(new KeyValuePair<string, object>("sAMAccountName", ADSearchResultConverter.GetSinglestringValue(result, "sAMAccountName")));
            this.Properties.Add(new KeyValuePair<string, object>("cn", ADSearchResultConverter.GetSinglestringValue(result, "cn")));
            
        }
    }
}
