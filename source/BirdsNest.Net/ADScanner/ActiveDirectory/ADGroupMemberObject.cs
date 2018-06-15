using System.Collections.Generic;
using System.DirectoryServices;
using ADScanner.Neo4j;

namespace ADScanner.ActiveDirectory
{
    /// <summary>
    /// ADGroupMemberObject is the base class for User and Computer classes
    /// </summary>
    public abstract class ADGroupMemberObject:INode
    {
        public string Name { get; private set; }
        public string Label { get { return "AD_Object"; } }
        public virtual string SubLabel { get { return "Base_Object"; } }
        public string ID { get; private set; }
        public string Path { get; private set; }
        public string PrimaryGroupID { get; private set; }

        public List<KeyValuePair<string, object>> Properties { get; private set; }
        public List<string> MemberOfDNs { get; private set; }

        public ADGroupMemberObject(SearchResult result)
        {
            this.Name = ADSearchResultConverter.GetSinglestringValue(result, "name");
            this.ID = ADSearchResultConverter.GetSidAsString(result);
            this.Path = ADSearchResultConverter.GetSinglestringValue(result, "distinguishedName");
            this.MemberOfDNs = ADSearchResultConverter.GetStringList(result, "memberOf");
            this.PrimaryGroupID = ADSearchResultConverter.GetSinglestringValue(result, "primaryGroupID");

            this.Properties = new List<KeyValuePair<string, object>>();
            this.Properties.Add(new KeyValuePair<string, object>("distinguishedName", this.Path));
            this.Properties.Add(new KeyValuePair<string, object>("sAMAccountName", ADSearchResultConverter.GetSinglestringValue(result, "sAMAccountName")));
            this.Properties.Add(new KeyValuePair<string, object>("cn", ADSearchResultConverter.GetSinglestringValue(result, "cn")));
            this.Properties.Add(new KeyValuePair<string, object>("primaryGroupID", this.PrimaryGroupID));
        }
    }
}
