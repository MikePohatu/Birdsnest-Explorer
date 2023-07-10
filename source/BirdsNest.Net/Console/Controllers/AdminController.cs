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
using Console.neo4jProxy.Indexes;
using Console.Plugins;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Console.Controllers
{
    [Authorize(Policy = Auth.Types.BirdsNestAdminsPolicy)]
    [Produces("application/json")]
    [Route("api/[controller]")]
    [ApiController]
    public class AdminController : Controller
    {
        private readonly PluginManager _pluginmanager;
        private readonly Neo4jService _neoservice;
        private readonly ILogger _logger;

        public AdminController(ILogger<AdminController> logger, PluginManager plugman, Neo4jService service)
        {
            this._neoservice = service;
            this._pluginmanager = plugman;
            this._logger = logger;
        }

        [ValidateAntiForgeryToken]
        [HttpPost("indexes/drop")]
        public async Task<int> DropIndex([FromBody] Console.neo4jProxy.Indexes.Index index)
        {
            this._logger.LogWarning($"Index drop for {index.Label}:{index.PropertyName} initiated by {User.FindFirst(ClaimTypes.Name)?.Value}");
            if (this._pluginmanager.NodeDataTypes.ContainsKey(index.Label))
            {
                DataType dt = this._pluginmanager.NodeDataTypes[index.Label];
                if (dt.Properties.ContainsKey(index.PropertyName))
                {
                    Property prop = dt.Properties[index.PropertyName];
                    if (prop.IndexEnforced == false) { return await this._neoservice.DropIndexNameAsync(index.IndexName); }
                }
            }

            this._logger.LogError($"Invalid index drop request: {index.IndexName}");
            return -1;


        }

        [ValidateAntiForgeryToken]
        [HttpPost("indexes/create")]
        public async Task<int> CreateIndex([FromBody] NewIndex newIndex)
        {
            this._logger.LogWarning($"Index creation for {newIndex.Label}:{newIndex.Property} initiated by {User.FindFirst(ClaimTypes.Name)?.Value}");
            if (this._pluginmanager.NodeDataTypes.ContainsKey(newIndex.Label))
            {
                DataType dt = this._pluginmanager.NodeDataTypes[newIndex.Label];
                if (dt.Properties.ContainsKey(newIndex.Property))
                {
                    return await this._neoservice.CreateIndexAsync(newIndex);
                }
            }

            this._logger.LogError($"Invalid index creation request: {newIndex.Label}:{newIndex.Property}");
            return -1;
        }

        [ValidateAntiForgeryToken]
        [HttpPost("reloadplugins")]
        public async Task<object> ReloadPlugins()
        {
            this._logger.LogInformation("Reload plugins initiated by {0}", User.FindFirst(ClaimTypes.Name)?.Value);
            Dictionary<string, object> data = new Dictionary<string, object>();
            bool ret = await this._pluginmanager.ReloadAsync();
            if (ret)
            {
                data.Add("status", 200);
                data.Add("message", "Plugins updated OK");
            }
            else
            {
                data.Add("status", 500);
                data.Add("message", "There was an error reloading plugins. Please check the server logs");
            }
            return Json(data);
        }
    }
}
