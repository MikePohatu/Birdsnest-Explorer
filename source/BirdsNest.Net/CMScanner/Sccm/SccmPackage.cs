using System.Collections.Generic;

namespace CMScanner.Sccm
{
    public class SccmPackage: ISccmObject
    {
        public string ID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public PackageType PackageType { get; set; }

        public bool IsHighlighted { get; set; }
        public SccmItemType Type { get { return SccmItemType.Package; } }
    }
}
