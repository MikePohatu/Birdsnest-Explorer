using Microsoft.ConfigurationManagement.ManagementProvider;

namespace CMScanner.Sccm
{
    public class SccmSoftwareUpdateGroup : SccmDeployableItem
    {
        public override SccmItemType Type { get { return SccmItemType.SoftwareUpdateGroup; } }
        public SccmSoftwareUpdateGroup(): base() { }
        public SccmSoftwareUpdateGroup(IResultObject resource) : base(resource) { }
    }
}
