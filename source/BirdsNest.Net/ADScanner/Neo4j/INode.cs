using System.Collections.Generic;

namespace ADScanner.Neo4j
{
    public interface INode
    {
        string Label { get; }
        string ID { get; }
        string Path { get; }
        string Name { get; }
        List<KeyValuePair<string,object>> Properties { get; }
    }
}
