using Microsoft.ConfigurationManagement.ManagementProvider;
using System.Collections.Generic;

namespace CMScanner.Sccm
{
    public abstract class SccmDeployableItem : ISccmObject
    {
        public string ID { get; set; }
        public string Name { get; set; }

        public abstract SccmItemType Type { get; }

        public SccmDeployableItem() { }
        public SccmDeployableItem(IResultObject resource)
        {
            this.ID = ResultObjectHandler.GetString(resource, "CI_ID");
            this.Name = ResultObjectHandler.GetString(resource, "LocalizedDisplayName");
        }

        public virtual object GetObject()
        {
            Dictionary<string, object> props = new Dictionary<string, object>();
            props.Add("ID", this.ID);
            props.Add("Name", this.Name);

            return props;
        }
    }
}
