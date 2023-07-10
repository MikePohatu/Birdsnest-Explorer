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
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Console.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class ReportsController : ControllerBase
    {
        private readonly PluginManager _pluginmanager;
        private readonly Neo4jService _service;

        public ReportsController(PluginManager plugman, Neo4jService service)
        {
            this._pluginmanager = plugman;
            this._service = service;
        }

        // GET reports/report?pluginname=***&reportname=****
        [HttpGet("report")]
        public async Task<ResultSet> Report([FromQuery] string pluginname, [FromQuery] string reportname)
        {
            Plugin plugin;
            this._pluginmanager.Plugins.TryGetValue(pluginname, out plugin);
            Report rep;
            plugin.Reports.TryGetValue(reportname, out rep);

            ResultSet results = await _service.GetResultSetFromQueryAsync(rep.Query, null);
            foreach (string filter in rep.PropertyFilters)
            { results.AddPropertyFilter(filter); }

            return results;
        }


        public class NodeList
        {
            public List<long> ids { get; set; }
            public List<string> propertyfilters { get; set; }
        }

        // GET reports/nodesquery?id=1&id=2
        [ValidateAntiForgeryToken]
        [HttpPost("nodes")]
        public async Task<ResultSet> NodesQuery([FromBody] NodeList nodelist)
        {
            ResultSet results = await this._service.GetNodesAsync(nodelist.ids);
            foreach (string filter in nodelist.propertyfilters)
            { results.AddPropertyFilter(filter); }

            return results;
        }
    }
}