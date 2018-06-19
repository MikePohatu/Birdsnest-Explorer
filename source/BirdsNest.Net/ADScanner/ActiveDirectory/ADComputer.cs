using System.DirectoryServices;
using System.Collections.Generic;

namespace ADScanner.ActiveDirectory
{
    public class ADComputer: ADEntity
    {
        public override string Type { get { return "AD_COMPUTER"; } }
        public string OperatingSystem { get; private set; }
        public string OperatingSystemVersion { get; private set; }
        public string State { get; private set; }

        public ADComputer(SearchResult result):base(result)
        {
            this.OperatingSystem = ADSearchResultConverter.GetSinglestringValue(result, "operatingsystem");
            this.OperatingSystemVersion = ADSearchResultConverter.GetSinglestringValue(result, "operatingsystemversion");

            //find if the computer is enabled
            int istate = ADSearchResultConverter.GetIntSingleValue(result, "useraccountcontrol");
            this.State = (istate == 4098) ? "disabled" : "enabled";

            this.Properties.Add("state", this.State);
            this.Properties.Add("operatingsystem", this.OperatingSystem);
            this.Properties.Add("operatingsystemversion", this.OperatingSystemVersion);
            this.Properties.Add("type", this.Type);
        }
    }
}
