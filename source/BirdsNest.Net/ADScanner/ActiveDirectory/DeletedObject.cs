using System.Collections.Generic;
using System.DirectoryServices;
using ADScanner.Neo4j;

namespace ADScanner.ActiveDirectory
{
    public class DeletedObject: ADObject
    {
        public override string Label { get { return Labels.Deleted; } }

        public DeletedObject(SearchResult result) : base(result)
        {
            this.Properties.Add(new KeyValuePair<string, object>("isdeleted", ADSearchResultConverter.GetSinglestringValue(result, "isdeleted")));
            this.Properties.Add(new KeyValuePair<string, object>("isrecycled", ADSearchResultConverter.GetSinglestringValue(result, "isrecycled")));
            this.Properties.Add(new KeyValuePair<string, object>("lastKnownParent", ADSearchResultConverter.GetSinglestringValue(result, "lastKnownParent")));
        }
    }
}
