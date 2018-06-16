using System.DirectoryServices;
using System.Collections.Generic;

namespace ADScanner.ActiveDirectory
{
    public class ADUser : ADEntity
    {
        public override string Label { get { return "AD_USER"; } }

        public ADUser(SearchResult result) : base(result)
        {
            this.Properties.Add(new KeyValuePair<string, object>("displayName", ADSearchResultConverter.GetSinglestringValue(result, "displayName")));

            //find if the user is enabled
            int istate = ADSearchResultConverter.GetIntSingleValue(result, "userAccountControl");
            string state = ((istate == 512) || (istate == 66050)) ? "disabled" : "enabled";
            this.Properties.Add(new KeyValuePair<string, object>("state", state));
        }
    }
}
