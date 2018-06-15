using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ADScanner.Neo4j
{
    public interface INode
    {
        string Label { get; }
        string ID { get; }
        string Path { get; }
        string Name { get; }
        string SubLabel { get; }
        List<KeyValuePair<string,object>> Properties { get; }
    }
}
