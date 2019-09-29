using Microsoft.ConfigurationManagement.ManagementProvider;
using System.Collections.Generic;

namespace CMScanner.Sccm
{
    public class SccmDevice: SccmResource
    {
        public override SccmItemType Type { get { return SccmItemType.Device; } }
        public string SID { get; private set; }
        public string DN { get; private set; }
        public string OU { get; private set; }

        public static SccmDevice GetSccmDevice(IResultObject result)
        {
            SccmDevice dev = new SccmDevice();

            dev.ID = ResultObjectHandler.GetString(result, "ResourceId");
            dev.SID = ResultObjectHandler.GetString(result, "SID");
            dev.DN = ResultObjectHandler.GetString(result, "DistinguishedName");
            //dev.OU = ResultObjectHandler.GetString(result, "SystemOUName");
            dev.Name = ResultObjectHandler.GetString(result, "Name");
            //dev.CollectionIDs.Add(result["CollectionID"].StringValue);
            return dev;
        }

        public override object GetObject()
        {
            Dictionary<string, object> o = base.GetObject() as Dictionary<string,object>;
            o.Add("SID", this.SID);
            o.Add("DN", this.DN);
            return o;
        }
    }
}
