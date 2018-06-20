using System.Collections.Generic;
using System.DirectoryServices;
using ADScanner.Neo4j;

namespace ADScanner.ActiveDirectory
{
    public class DeletedObject: ADObject
    {
        public override string Type { get { return Types.Deleted; } }
        public bool IsDeleted { get; private set; }
        public bool IsRecycled { get; private set; }
        public string LastKnownParent { get; private set; }

        public DeletedObject(SearchResult result, string scanid) : base(result, scanid)
        {
            this.IsDeleted = ADSearchResultConverter.GetBoolSingleValue(result, "isdeleted");
            this.IsRecycled = ADSearchResultConverter.GetBoolSingleValue(result, "isrecycled");
            this.LastKnownParent = ADSearchResultConverter.GetSinglestringValue(result, "lastKnownParent");

            this.Properties.Add("isdeleted", this.IsDeleted);
            this.Properties.Add("isrecycled", this.IsRecycled);
            this.Properties.Add("lastknownparent", this.LastKnownParent);
            this.Properties.Add("type", this.Type);
        }
    }
}
