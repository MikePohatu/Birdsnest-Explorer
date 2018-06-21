using System.Collections.Generic;

namespace common
{
    public interface IBirdsNestNode
    {
        string Type { get; }
        string ID { get; }
        string Path { get; }
        string Name { get; }
        Dictionary<string, object> Properties { get; }
    }
}
