using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Console.neo4jProxy;
using common;

using Microsoft.Extensions.Logging;

namespace Console.ActiveDirectory
{
    public static class ADQueries
    {
        public static string EmptyGroups { get { return "MATCH (n:" + Types.Group + ") where n.scope=0 RETURN n ORDER BY n.name"; } }
        public static string GroupLoops { get { return "MATCH p=(n:" + Types.Group + ")-[:" + Types.MemberOf + "*]->(n) RETURN p"; } }
    }
}
