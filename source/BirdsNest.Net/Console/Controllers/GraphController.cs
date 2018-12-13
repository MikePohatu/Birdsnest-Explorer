using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Console.neo4jProxy;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Console.Controllers
{
    [Authorize]
    [Produces("application/json")]
    [Route("api/[controller]")]
    [ApiController]
    public class GraphController : ControllerBase
    {
        private readonly Neo4jService _service;
        public GraphController(Neo4jService service)
        {
            this._service = service;
        }


        //nodes bits
        //****************
        // GET api/graph/node/<id>
        [HttpGet("node/{id}")]
        public object Get(long id)
        {
            return this._service.GetNode(id);
        }

        // GET api/graph/nodes/<id>
        [HttpGet("nodes/{id}")]
        public object GetRelatedNodes(long id)
        {
            return this._service.GetRelatedNodes(id);
        }

        // GET api/graph/nodes?id=1&id=2
        [HttpGet("nodes")]
        public object GetNodes([FromQuery(Name = "id")]List<long> id)
        {
            return this._service.GetNodes(id);
        }


        // GET api/graph/node/labels
        [HttpGet("node/labels")]
        public IEnumerable<string> GetLabels()
        {
            return this._service.GetNodeLabels();
        }

        // GET: api/graph/node/properties
        [HttpGet("node/properties")]
        public IEnumerable<string> GetNodeProperties([FromQuery]string type)
        {
            return this._service.GetNodeProperties(type);
        }

        // GET: api/graph/node/details
        [HttpGet("node/details")]
        public SortedDictionary<string, List<string>> GetNodeDetails()
        {
            return this._service.GetNodeDetails();
        }

        // GET: api/graph/node/values
        [HttpGet("node/values")]
        public IEnumerable<string> GetNodeValues([FromQuery]string type, [FromQuery]string property, [FromQuery]string searchterm)
        {
            return this._service.SearchNodePropertyValues(type, property, searchterm);
        }


        //edges stuff
        //****************
        // POST: api/graph/edges
        [HttpGet("edges")]
        public object GetEdgesForNode([FromQuery] long nodeid)
        {
            List<long> idlist = new List<long>();
            idlist.Add(nodeid);
            return this._service.GetRelationships(idlist);
        }

        [HttpPost("edges")]
        public async Task<object> GetEdgesForNodes([FromBody]List<long> value)
        {
            var d = await this._service.GetRelationshipsAsync(value);
            return d;
        }

        // POST api/graph/edges/direct
        [HttpPost("edges/direct")]
        public async Task<object> GetDirectEdgesForNodes([FromBody]List<long> value)
        {
            var d = await this._service.GetDirectRelationshipsAsync(value);
            return d;
        }

        // GET api/edges/labels
        [HttpGet("edges/labels")]
        public IEnumerable<string> GetEdgeLabels()
        {
            return this._service.GetEdgeLabels();
        }


        //search stuff
        //*******************
        // GET: api/graph/search/path
        [HttpGet("search/path")]
        public ResultSet GetNodeValues([FromQuery]string sourcetype, [FromQuery]string sourceprop, [FromQuery]string sourceval,
            [FromQuery]string relationship, [FromQuery]int relmin, [FromQuery]int relmax, [FromQuery]char dir,
            [FromQuery]string tartype, [FromQuery]string tarprop, [FromQuery]string tarval)
        {
            return this._service.SearchPath(sourcetype, sourceprop, sourceval, relationship, relmin, relmax, dir, tartype, tarprop, tarval);
        }
    }
}