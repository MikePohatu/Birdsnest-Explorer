using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Console.neo4jProxy;
using Microsoft.AspNetCore.Authorization;

namespace Console.Controllers
{
    [Authorize(Policy = "IsBirdsNestUser")]
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

        // GET api/nodes/labels
        [HttpGet("labels")]
        public IEnumerable<string> GetLabels()
        {
            return this._service.GetNodeLabels();
        }

        // GET: api/nodes/properties
        [HttpGet("properties")]
        public IEnumerable<string> GetNodeProperties([FromQuery]string type)
        {
            return this._service.GetNodeProperties(type);
        }

        // GET: api/nodes/properties
        [HttpGet("values")]
        public IEnumerable<string> GetNodeValues([FromQuery]string type, [FromQuery]string property, [FromQuery]string searchterm)
        {
            return this._service.SearchNodePropertyValues(type, property, searchterm);
        }
    }
}
