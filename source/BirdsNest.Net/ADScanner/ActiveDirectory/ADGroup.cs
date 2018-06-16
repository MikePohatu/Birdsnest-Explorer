using System.Collections.Generic;
using System.DirectoryServices;
using ADScanner.Neo4j;

namespace ADScanner.ActiveDirectory
{
    public class ADGroup: INode
    {
        const string SCOPE_GLOBAL = "Global";
        const string SCOPE_UNIVERSAL = "Universal";
        const string SCOPE_DOMAIN_LOCAL = "DomainLocal";
        const string TYPE_SECURITY = "Security";
        const string TYPE_DISTRIBUTION = "Distribution";

        public string Name { get; private set; }
        public string Label { get { return "AD_GROUP"; } }
        public string GroupType { get; private set; }
        public string ID { get; private set; }
        public string Path { get; private set; }
        public List<KeyValuePair<string, object>> Properties { get; private set; }
        public List<string> MemberDNs { get; private set; }
        public List<string> MemberOfDNs { get; private set; }

        public ADGroup(SearchResult result)
        {
            this.Name = ADSearchResultConverter.GetSinglestringValue(result, "samaccountname");
            this.ID = ADSearchResultConverter.GetSidAsString(result);
            this.Properties = new List<KeyValuePair<string, object>>();
            this.Path = ADSearchResultConverter.GetSinglestringValue(result, "distinguishedName");
            this.MemberDNs = ADSearchResultConverter.GetStringList(result,"member");
            this.MemberOfDNs = ADSearchResultConverter.GetStringList(result, "memberOf");

            this.SetTypeAndScope(ADSearchResultConverter.GetSinglestringValue(result, "groupType"));
            this.Properties.Add(new KeyValuePair<string, object>("distinguishedName", ADSearchResultConverter.GetSinglestringValue(result, "distinguishedName")));
            this.Properties.Add(new KeyValuePair<string, object>("rid", ADSearchResultConverter.GetRidFromSid(this.ID)));
        }

        private void SetTypeAndScope(string grouptype)
        {
            switch (grouptype)
            {
                case "-2147483646":
                    this.GroupType = TYPE_SECURITY + "_" + SCOPE_GLOBAL;
                    break;
                case "-2147483644":
                    this.GroupType = TYPE_SECURITY + "_" + SCOPE_DOMAIN_LOCAL;
                    break;
                case "-2147483643":
                    this.GroupType = TYPE_SECURITY + "_" + SCOPE_DOMAIN_LOCAL;
                    break;
                case "-2147483645":
                    this.GroupType = TYPE_SECURITY + "_" + SCOPE_DOMAIN_LOCAL;
                    break;
                case "-2147483640":
                    this.GroupType = TYPE_SECURITY + "_" + SCOPE_UNIVERSAL;
                    break;
                case "2":
                    this.GroupType = TYPE_DISTRIBUTION + "_" + SCOPE_GLOBAL;
                    break;
                case "4":
                    this.GroupType = TYPE_DISTRIBUTION + "_" + SCOPE_DOMAIN_LOCAL;
                    break;
                case "8":
                    this.GroupType = TYPE_DISTRIBUTION + "_" + SCOPE_UNIVERSAL;
                    break;
                default:
                    break;
            }
        }
    }
}
