using System.DirectoryServices;
using System.Collections.Generic;

namespace ADScanner.ActiveDirectory
{
    public class ADComputer: ADEntity
    {
        public override string Label { get { return "AD_COMPUTER"; } }

        public ADComputer(SearchResult result):base(result)
        {
            this.Properties.Add(new KeyValuePair<string, object>("operatingsystem", ADSearchResultConverter.GetSinglestringValue(result, "operatingsystem")));
            this.Properties.Add(new KeyValuePair<string, object>("operatingsystemversion", ADSearchResultConverter.GetSinglestringValue(result, "operatingsystemversion")));

            //find if the computer is enabled
            int istate = ADSearchResultConverter.GetIntSingleValue(result, "useraccountcontrol");
            string state = (istate == 4098) ? "disabled" : "enabled";
            this.Properties.Add(new KeyValuePair<string, object>("state", state));
        }
    }
}
