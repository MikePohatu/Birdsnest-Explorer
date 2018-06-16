using System.Collections.Generic;
using System.DirectoryServices;
using ADScanner.Neo4j;

namespace ADScanner.ActiveDirectory
{
    public abstract class ADObject:INode
    {
        public string Name { get; protected set; }
        public virtual string Label { get { return "AD_Object"; } }
        public string ID { get; protected set; }
        public string Path { get; private set; }


        public List<KeyValuePair<string, object>> Properties { get; private set; }

        public ADObject(SearchResult result)
        {
            this.ID = ADSearchResultConverter.GetSidAsString(result);
            this.Path = ADSearchResultConverter.GetSinglestringValue(result, "distinguishedname");

            this.Properties = new List<KeyValuePair<string, object>>();
            this.Properties.Add(new KeyValuePair<string, object>("distinguishedname", this.Path));
            this.Properties.Add(new KeyValuePair<string, object>("cn", ADSearchResultConverter.GetSinglestringValue(result, "cn")));
            this.Properties.Add(new KeyValuePair<string, object>("isdeleted", ADSearchResultConverter.GetSinglestringValue(result, "isdeleted")));
            this.Properties.Add(new KeyValuePair<string, object>("isrecycled", ADSearchResultConverter.GetSinglestringValue(result, "isrecycled")));
        }
    }
}
