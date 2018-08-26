using Microsoft.ConfigurationManagement.ManagementProvider;

namespace CMScanner.Sccm
{
    public class SccmConfigurationBaseline : SccmDeployableItem
    {
        public override SccmItemType Type { get { return SccmItemType.ConfigurationBaseline; } }
        public SccmConfigurationBaseline(): base() { }
        public SccmConfigurationBaseline(IResultObject resource) : base(resource) { }
    }
}
