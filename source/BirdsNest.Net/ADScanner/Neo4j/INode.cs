using System.Collections.Generic;

namespace ADScanner.Neo4j
{
    public interface INode
    {
        string Type { get; }
        string ID { get; }
        string Path { get; }
        string Name { get; }
        Dictionary<string, object> Properties { get; }
    }
}
