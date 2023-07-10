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
using Console.Helpers;
using Console.neo4jProxy;
using Console.neo4jProxy.Indexes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Reflection;
using System.Threading.Tasks;

namespace Console.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class ServerInfoController : ControllerBase
    {
        private readonly StatsGetter _getter;
        private readonly ILogger _logger;
        private readonly Neo4jService _neoservice;

        public ServerInfoController(Neo4jService neoservice, ILogger<ServerInfoController> logger)
        {
            this._neoservice = neoservice;
            this._getter = new StatsGetter(neoservice);
            this._logger = logger;
        }

        [HttpGet("install")]
        [LocalOnly]
        [AllowAnonymous]
        public InstallInfo GetInstallInfo()
        {
            if (HttpHelpers.IsLocal(HttpContext.Request))
            {
                return InstallInfo.Instance;
            }
            else
            {
                return null;
            }
        }

        //nodes bits
        //****************
        // GET api/stats/db
        [HttpGet]
        public async Task<object> Get()
        {
            var dbStats = await this._getter.GatherDbStats();
            var assemblyInfo = Assembly.GetEntryAssembly().GetName().Version.ToString();
            var indexes = await IndexGetter.GatherDbIndexes4Async(this._neoservice);
            return new { serverVersion = assemblyInfo, dbStats, indexes };
        }
    }
}