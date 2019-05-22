using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Console.neo4jProxy;
using common;

using Microsoft.Extensions.Logging;

namespace Console.ActiveDirectory
{
    public class Queries
    {
        private readonly Neo4jService _service;

        public Queries(Neo4jService service)
        {
            this._service = service;
        }

        public ResultSet GetEmptyGroups()
        {
            string query = "MATCH (n:" + Types.Group + ") where n.scope=0 RETURN n ORDER BY n.name";
            return _service.GetResultSetFromQuery(query);
        }

        public ResultSet GetGroupLoops()
        {
            string query = "MATCH p=(n:" + Types.Group + ")-[:" + Types.MemberOf + "*]->(n) RETURN p";
            return _service.GetResultSetFromQuery(query);
        }
    }
}
