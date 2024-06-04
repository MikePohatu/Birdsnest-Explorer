#region license
// Copyright (c) 2019-2023 "20Road"
// 20Road Limited [https://www.20road.com]
//
// This file is part of Birdsnest Explorer.
//
// Birdsnest Explorer is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, version 3 of the License.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.
#endregion
using Console.neo4jProxy;
using Console.Plugins;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Console.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class GraphController : ControllerBase
    {
        private readonly Neo4jService _service;
        private readonly PluginManager _pluginmanager;
        private readonly ILogger _logger;

        public GraphController(Neo4jService service, PluginManager pluginmanager, ILogger<GraphController> logger)
        {
            this._service = service;
            this._pluginmanager = pluginmanager;
            this._logger = logger;
        }


        //nodes bits
        //****************
        // GET api/graph/node/<id>
        [HttpGet("node/{id}")]
        public async Task<BirdsNestNode> Get(long id)
        {
            ResultSet rs = await this._service.GetNodeAsync(id);
            return rs.Nodes.First();
        }

        // GET api/graph/node/<id>/related
        [HttpGet("node/{id}/related")]
        public async Task<List<BirdsNestNode>> GetRelatedNodesAsync(long id)
        {
            ResultSet rs = await this._service.GetRelatedNodesAsync(id);
            return rs.Nodes;
        }


        // GET api/graph/node/related?id=1&id=2
        [HttpGet("node/related")]
        public async Task<List<RelatedDetail>> GetRelatedAsync([FromQuery(Name = "id")] List<long> ids)
        {
            List<RelatedDetail> details = new List<RelatedDetail>();
            try
            {
                foreach (long id in ids)
                {
                    ResultSet nodeResultSet = await this._service.GetNodeAsync(id);
                    ResultSet rs = await this._service.GetAllRelatedAsync(id);

                    RelatedDetail detail = new RelatedDetail(nodeResultSet.Nodes.First(), rs);
                    details.Add(detail);
                }

            }
            catch (Exception e)
            {
                this._logger.LogError("Error getting related details for node id {0}: {1}", String.Join(" ,", ids), e.Message);
            }

            return details;
        }

        // GET api/graph/nodes?id=1&id=2
        [HttpGet("nodes")]
        public async Task<ResultSet> GetNodes([FromQuery(Name = "id")] List<long> ids)
        {
            return await this._service.GetNodesAsync(ids);
        }

        // POST api/graph/nodes
        [HttpPost("nodes")]
        public async Task<ResultSet> GetNodesPost([FromBody] List<long> ids)
        {
            return await this._service.GetNodesAsync(ids);
        }

        // GET: api/graph/node/values
        [HttpGet("node/values")]
        public async Task<IEnumerable<string>> GetNodeValues([FromQuery] string type, [FromQuery] string property, [FromQuery] string searchterm)
        {
            return await this._service.SearchNodePropertyValuesAsync(type, property, searchterm);
        }

        // GET: api/graph/node/values
        [HttpGet("node/namevalues")]
        public async Task<IEnumerable<string>> GetNodeNameValues([FromQuery] string searchterm)
        {
            return await this._service.SearchNodePropertyValuesAsync(null, "name", searchterm);
        }


        //edges stuff
        //****************
        // POST: api/graph/edges
        [HttpGet("edges")]
        public async Task<ResultSet> GetEdgesForNode([FromQuery] List<long> nodeid)
        {
            return await this._service.GetRelationshipsAsync(nodeid);
        }

        [ValidateAntiForgeryToken]
        [HttpPost("edges")]
        public async Task<ResultSet> GetEdgesForNodes([FromBody] List<long> value)
        {
            return await this._service.GetRelationshipsAsync(value);
        }

        // POST api/graph/directloops
        [ValidateAntiForgeryToken]
        [HttpPost("directloops")]
        public async Task<object> GetDirectLoopsForNodes([FromBody] List<long> value)
        {
            var d = await this._service.GetDirectLoopsAsync(value);
            return d;
        }

        // POST api/graph/edges/direct
        [HttpPost("edges/direct")]
        public async Task<ResultSet> GetDirectEdgesForNodes([FromBody] List<long> nodeids)
        {
            var d = await this._service.GetDirectRelationshipsAsync(nodeids);
            return d;
        }

        // GET: api/graph/edge/values
        [HttpGet("edge/values")]
        public async Task<IEnumerable<string>> GetEdgeValues([FromQuery] string type, [FromQuery] string property, [FromQuery] string searchterm)
        {
            return await this._service.SearchEdgePropertyValuesAsync(type, property, searchterm);
        }


        // GET visualizer/edgedetails
        public async Task<ResultSet> EdgeDetailsAsync(long id)
        {
            return await this._service.GetEdgeAsync(id);
        }
    }
}