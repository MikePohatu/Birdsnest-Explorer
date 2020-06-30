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
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Console.Plugins;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace Console.Controllers
{
    [Authorize(Policy = Auth.Types.BirdsNestAdminsPolicy)]
    [Produces("application/json")]
    [Route("api/[controller]")]
    [ApiController]
    public class AdminController : Controller
    {
        private readonly PluginManager _pluginmanager;
        private readonly ILogger _logger;

        public AdminController(ILogger<AdminController> logger, PluginManager plugman)
        {
            this._pluginmanager = plugman;
            this._logger = logger;
        }

        [ValidateAntiForgeryToken]
        [HttpPost("reloadplugins")]
        public async Task<object> ReloadPlugins()
        {
            this._logger.LogInformation("Reload plugins initiated by {0}", User.FindFirst(ClaimTypes.Name)?.Value);
            Dictionary<string,object> data = new Dictionary<string, object>();
            bool ret = await this._pluginmanager.ReloadAsync();
            if (ret)
            {
                data.Add("status",200);
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
