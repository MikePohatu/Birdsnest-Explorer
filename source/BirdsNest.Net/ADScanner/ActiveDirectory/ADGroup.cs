using System.Collections.Generic;
using System.DirectoryServices;
using ADScanner.Neo4j;

namespace ADScanner.ActiveDirectory
{
    public class ADGroup: ADGroupMemberObject
    {
        const string SCOPE_GLOBAL = "Global";
        const string SCOPE_UNIVERSAL = "Universal";
        const string SCOPE_DOMAIN_LOCAL = "DomainLocal";
        const string TYPE_SECURITY = "Security";
        const string TYPE_DISTRIBUTION = "Distribution";

        public override string Label { get { return "AD_GROUP"; } }
        public string GroupType { get; private set; }
        public List<string> MemberDNs { get; private set; }

        public ADGroup(SearchResult result): base (result)
        {
            this.MemberDNs = ADSearchResultConverter.GetStringList(result,"member");

            this.SetTypeAndScope(ADSearchResultConverter.GetSinglestringValue(result, "groupType"));
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
