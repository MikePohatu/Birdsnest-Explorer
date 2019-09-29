using Microsoft.ConfigurationManagement.ManagementProvider;
using System.Collections.Generic;

namespace CMScanner.Sccm
{
    public class SccmUser : SccmResource
    {
        public string FullUserName { get; set; }
        public override SccmItemType Type { get { return SccmItemType.User; } }

        public string SID { get; private set; }
        public string DN { get; private set; }
        public string OU { get; private set; }

        public static SccmUser GetSccmUser(IResultObject result)
        {
            SccmUser user = new SccmUser();

            user.ID = ResultObjectHandler.GetString(result, "ResourceId");
            user.SID = ResultObjectHandler.GetString(result, "SID");
            user.DN = ResultObjectHandler.GetString(result, "DistinguishedName");
            //dev.OU = ResultObjectHandler.GetString(result, "SystemOUName");
            user.Name = ResultObjectHandler.GetString(result, "Name");
            //dev.CollectionIDs.Add(result["CollectionID"].StringValue);
            return user;
        }

        public override object GetObject()
        {
            Dictionary<string, object> o = base.GetObject() as Dictionary<string, object>;
            o.Add("SID", this.SID);
            o.Add("DN", this.DN);
            return o;
        }
    }
}
