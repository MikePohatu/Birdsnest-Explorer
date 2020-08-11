#region license
// Copyright (c) 2019-2020 "20Road"
// 20Road Limited [https://20road.com]
//
// This file is part of BirdsNest.
//
// BirdsNest is free software: you can redistribute it and/or modify
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
using System.Reflection;
using Console.neo4jProxy.Indexes;


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


        //nodes bits
        //****************
        // GET api/stats/db
        [HttpGet]
        public async Task<object> Get()
        {
            var dbStats = await this._getter.GatherDbStats();
            var assemblyInfo = Assembly.GetEntryAssembly().GetName().Version.ToString();
            var indexes = await IndexGetter.GatherDbIndexes(this._neoservice);
            return new { serverVersion = assemblyInfo, dbStats, indexes };
        }
    }
}