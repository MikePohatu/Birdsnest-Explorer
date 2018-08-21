using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Console.neo4jProxy;
using Microsoft.AspNetCore.Authorization;

namespace Console.Controllers
{
    [Authorize]
    [Produces("application/json")]
    [Route("api/Edges")]
    public class EdgesController : Controller
    {
        private readonly Neo4jService _service;
        public EdgesController(Neo4jService service)
        {
            this._service = service;
        }

        // POST: api/edges
        [HttpGet()]
        public object GetEdgesForNode([FromQuery] long nodeid)
        {
            List<long> idlist = new List<long>();
            idlist.Add(nodeid);
            return this._service.GetRelationships(idlist);
        }

        [HttpPost()]
        public async Task<object> GetEdgesForNodes([FromBody]List<long> value)
        {
            var d = await this._service.GetRelationshipsAsync(value);
            return d;
        }

        // POST api/edges/direct
        [HttpPost("direct")]
        public async Task<object> GetDirectEdgesForNodes([FromBody]List<long> value)
        {
            var d = await this._service.GetDirectRelationshipsAsync(value);
            return d;
        }

        // GET api/edges/labels
        [HttpGet("labels")]
        public IEnumerable<string> GetLabels()
        {
            return this._service.GetEdgeLabels();
        }
    }
}
