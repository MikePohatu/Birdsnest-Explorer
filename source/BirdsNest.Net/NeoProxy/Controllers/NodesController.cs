using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace NeoProxy.Controllers
{
    [Produces("application/json")]
    [Route("api/Nodes")]
    public class NodesController : Controller
    {
        private readonly Neo4jService _service;
        public NodesController(Neo4jService service)
        {
            this._service = service;
        }

        [HttpGet("node/{id}")]
        public object Get(long id)
        {
            return this._service.GetNode(id);
        }

        [HttpGet()]
        public object GetRelatedNodes([FromQuery] long nodeid)
        {
            return this._service.GetRelatedNodes(nodeid);
        }
    }
}
