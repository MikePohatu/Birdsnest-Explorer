namespace CMScanner.Sccm
{
    public class SccmApplication: SccmDeployableItem
    {
        public override SccmItemType Type { get { return SccmItemType.Application; } }
        public bool IsDeployed { get; set; }
        public bool IsEnabled { get; set; }
        public bool IsSuperseded { get; set; }
        public bool IsSuperseding { get; set; }
        public bool IsLatest { get; set; }

        public new string ToString()
        {
            return this._name;
        }
    }
}
