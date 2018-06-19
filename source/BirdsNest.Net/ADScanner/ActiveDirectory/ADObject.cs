using System.Collections.Generic;
using System.DirectoryServices;
using ADScanner.Neo4j;

namespace ADScanner.ActiveDirectory
{
    public abstract class ADObject:INode
    {
        public string Name { get; protected set; }
        public virtual string Type { get { return "AD_Object"; } }
        public string ID { get; protected set; }
        public string Path { get; private set; }
        public string CN { get; private set; }
        public string DN { get; private set; }
        public Dictionary<string, object> Properties { get; private set; }

        public ADObject(SearchResult result)
        {
            this.ID = ADSearchResultConverter.GetSidAsString(result);
            this.Path = ADSearchResultConverter.GetSinglestringValue(result, "distinguishedname");
            this.DN = this.Path;
            this.CN = ADSearchResultConverter.GetSinglestringValue(result, "cn");
            this.Name = ADSearchResultConverter.GetSinglestringValue(result, "Name");

            this.Properties = new Dictionary<string, object>
            {
                {"name", this.Name},
                {"id", this.ID},
                {"path", this.Path},
                {"dn", this.DN},
                {"cn", this.CN }
            };
        }
    }
}
