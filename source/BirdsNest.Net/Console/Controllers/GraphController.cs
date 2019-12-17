using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Console.neo4jProxy;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Console.Plugins;

namespace Console.Controllers
{
    [Authorize]
    [Produces("application/json")]
    [Route("api/[controller]")]
    [ApiController]
    public class GraphController : ControllerBase
    {
        private readonly Neo4jService _service;
        private readonly ILogger _logger;
        private readonly PluginManager _pluginmanager;

        public GraphController(ILogger<GraphController> logger, Neo4jService service, PluginManager pluginmanager)
        {
            this._logger = logger;
            this._service = service;
            this._pluginmanager = pluginmanager;
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
        public object GetNodes([FromQuery(Name = "id")]List<long> ids)
        {
            return this._service.GetNodes(ids);
        }


        // GET api/graph/node/labels
        [HttpGet("node/labels")]
        public IEnumerable<string> GetLabels()
        {
            return this._pluginmanager.NodeLabels;
        }

        // GET: api/graph/node/properties
        [HttpGet("node/properties")]
        public SortedDictionary<string, List<string>> GetNodeProperties()
        {
            //return this._service.GetNodeProperties();
            return this._pluginmanager.NodeProperties;
        }

        // GET: api/graph/node/values
        [HttpGet("node/values")]
        public IEnumerable<string> GetNodeValues([FromQuery]string type, [FromQuery]string property, [FromQuery]string searchterm)
        {
            return this._service.SearchNodePropertyValues(type, property, searchterm);
        }

		// GET: api/graph/node/values
		[HttpGet("node/namevalues")]
		public IEnumerable<string> GetNodeNameValues([FromQuery]string searchterm)
		{
			return this._service.SearchNodePropertyValues(null, "name", searchterm);
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
            return this._pluginmanager.EdgeLabels;
        }

        // GET: api/graph/edge/properties
        [HttpGet("edge/properties")]
        public SortedDictionary<string, List<string>> GetEdgeProperties()
        {
            return this._pluginmanager.EdgeProperties;
            //return this._service.GetEdgeProperties();
        }

        // GET: api/graph/edge/values
        [HttpGet("edge/values")]
        public IEnumerable<string> GetEdgeValues([FromQuery]string type, [FromQuery]string property, [FromQuery]string searchterm)
        {
            return this._service.SearchEdgePropertyValues(type, property, searchterm);
        }
    }
}