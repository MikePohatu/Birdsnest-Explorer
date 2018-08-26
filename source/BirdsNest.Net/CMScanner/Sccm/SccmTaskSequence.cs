namespace CMScanner.Sccm
{
    public class SccmTaskSequence : SccmDeployableItem
    {
        public override SccmItemType Type { get { return SccmItemType.TaskSequence; } }
        public TaskSequenceType TaskSequenceType { get; set; }

        public new string ToString()
        {
            return this.Name;
        }
    }
}
