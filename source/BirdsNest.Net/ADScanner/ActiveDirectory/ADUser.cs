using System.DirectoryServices;
using System.Collections.Generic;

namespace ADScanner.ActiveDirectory
{
    public class ADUser : ADEntity
    {
        public override string Label { get { return Labels.User; } }

        public ADUser(SearchResult result) : base(result)
        {
            this.Properties.Add(new KeyValuePair<string, object>("displayName", ADSearchResultConverter.GetSinglestringValue(result, "displayname")));

            //find if the user is enabled
            int istate = ADSearchResultConverter.GetIntSingleValue(result, "useraccountcontrol");
            string state = ((istate == 512) || (istate == 66050)) ? "disabled" : "enabled";
            this.Properties.Add(new KeyValuePair<string, object>("state", state));
        }
    }
}
