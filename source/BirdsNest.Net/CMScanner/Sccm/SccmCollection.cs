using System.Collections.Generic;
using Microsoft.ConfigurationManagement.ManagementProvider;

namespace CMScanner.Sccm
{
    public class SccmCollection: ISccmObject
    {
        public SccmItemType Type { get { return SccmItemType.Collection; } }
        public string LimitingCollectionID { get; set; }
        public CollectionType CollectionType { get; set; }
        public int IncludeExcludeCollectionCount { get; set; }

        public string Comment { get; set; }
        public string Name { get; set; }
        public string ID { get; set; }

        public SccmCollection() { }

        public SccmCollection(string id, string name, string limitingcollectionid)
        {
            this.Name = name;
            this.ID = id;
            this.LimitingCollectionID = limitingcollectionid;
        }
    }
}
