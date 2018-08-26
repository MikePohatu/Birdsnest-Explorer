using Microsoft.ConfigurationManagement.ManagementProvider;

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
    }
}
