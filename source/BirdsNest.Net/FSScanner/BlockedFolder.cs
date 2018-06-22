namespace FSScanner
{
    public class BlockedFolder: Folder
    {
        public override string Type { get { return Types.BlockedFolder; } }

        public BlockedFolder(string path, string permparent): base(path, permparent, null)
        { }
    }
}
