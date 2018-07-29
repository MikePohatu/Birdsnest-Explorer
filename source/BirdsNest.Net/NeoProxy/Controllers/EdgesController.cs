using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace NeoProxy.Controllers
{
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
        public object GetEdgesForNodes([FromBody]List<long> value)
        {
            return this._service.GetRelationships(value);
        }

        // GET api/nodes/labels
        [HttpGet("labels")]
        public IEnumerable<string> GetLabels()
        {
            return this._service.GetEdgeLabels();
        }
    }
}
