using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.DirectoryServices;

namespace ADScanner.ActiveDirectory
{
    internal class ADGroup
    {
        public string Name { get; set; }
        public string DN { get; set; }
        public string SID { get; set; }
        public string CanonicalName { get; set; }

        public ADGroup(SearchResult result)
        {
            this.Name = ADSearchResultConverter.GetSinglestringValue(result,"name");
            this.SID = ADSearchResultConverter.GetSidAsString(result);
        }
    }
}
