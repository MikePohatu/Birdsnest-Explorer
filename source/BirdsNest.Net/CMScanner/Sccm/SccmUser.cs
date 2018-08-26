namespace CMScanner.Sccm
{
    public class SccmUser : SccmResource
    {
        public string FullUserName { get; set; }
        public override SccmItemType Type { get { return SccmItemType.User; } }
    }
}
