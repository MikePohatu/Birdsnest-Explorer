using System.Collections.Generic;
using System.DirectoryServices;
using ADScanner.Neo4j;

namespace ADScanner.ActiveDirectory
{
    public class ADGroup: ADGroupMemberObject
    {
        const string SCOPE_GLOBAL = "global";
        const string SCOPE_UNIVERSAL = "universal";
        const string SCOPE_DOMAIN_LOCAL = "domainlocal";
        const string TYPE_SECURITY = "security";
        const string TYPE_DISTRIBUTION = "distribution";

        public override string Type { get { return Types.Group; } }
        public string GroupType { get; private set; }
        public string Rid { get; private set; }
        public List<string> MemberDNs { get; private set; }

        public ADGroup(SearchResult result, string scanid) : base (result, scanid)
        {
            this.MemberDNs = ADSearchResultConverter.GetStringList(result,"member");

            this.SetTypeAndScope(ADSearchResultConverter.GetSinglestringValue(result, "grouptype"));
            this.Rid = ADSearchResultConverter.GetRidFromSid(this.ID);

            this.Properties.Add("rid", this.Rid);
            this.Properties.Add("grouptype", this.GroupType);
            this.Properties.Add("type", this.Type);
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
