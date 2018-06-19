using System.DirectoryServices;
using System.Collections.Generic;

namespace ADScanner.ActiveDirectory
{
    public class ADUser : ADEntity
    {
        public override string Type { get { return Types.User; } }
        public string State { get; private set; }
        public string DisplayName { get; private set; }

        public ADUser(SearchResult result) : base(result)
        {
            this.DisplayName = ADSearchResultConverter.GetSinglestringValue(result, "displayname");

            //find if the user is enabled
            int istate = ADSearchResultConverter.GetIntSingleValue(result, "useraccountcontrol");
            this.State = ((istate == 512) || (istate == 66050)) ? "disabled" : "enabled";

            this.Properties.Add("state", this.State);
            this.Properties.Add("displayname", this.DisplayName);
            this.Properties.Add("type", this.Type);
        }
    }
}
