using System.ComponentModel;

namespace CMScanner.Sccm
{
    public interface ISccmObject
    {
        SccmItemType Type { get; }
        string ID { get; }
        string Name { get; }
    }
}
