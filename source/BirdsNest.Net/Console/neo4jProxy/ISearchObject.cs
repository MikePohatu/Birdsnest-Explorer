using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Console.neo4jProxy
{
    public interface ISearchObject
    {
        string Identifier { get; }
        string GetPathString();
        string GetWhereString();
    }
}
