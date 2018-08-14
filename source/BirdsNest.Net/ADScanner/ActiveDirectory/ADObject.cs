using System.Collections.Generic;
using System.DirectoryServices;
using common;

namespace ADScanner.ActiveDirectory
{
    public abstract class ADObject: IBirdsNestNode
    {
        public string Name { get; protected set; }
        public virtual string Type { get { return "AD_Object"; } }
        public string ID { get; protected set; }
        public string Path { get; private set; }
        public string CN { get; private set; }
        public string DN { get; private set; }
        public string Description { get; private set; }
        public string Info { get; private set; }
        public Dictionary<string, object> Properties { get; private set; }
        public string ScanId;

        public ADObject(SearchResult result, string scanid)
        {
            this.ScanId = scanid;
            this.Path = ADSearchResultConverter.GetSinglestringValue(result, "distinguishedname");
            this.ID = this.Path;
            this.DN = this.Path;
            this.CN = ADSearchResultConverter.GetSinglestringValue(result, "cn");
            this.Name = ADSearchResultConverter.GetSinglestringValue(result, "Name");
            this.Info = ADSearchResultConverter.GetSinglestringValue(result, "Info");
            this.Description = ADSearchResultConverter.GetSinglestringValue(result, "Description");

            this.Properties = new Dictionary<string, object>
            {
                {"name", this.Name},
                {"id", this.ID},
                {"path", this.Path},
                {"dn", this.DN},
                {"cn", this.CN },
                {"description", this.Description },
                {"info", this.Info },
                {"scanid",this.ScanId }
            };
        }
    }
}
