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
//
#endregion
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Console.Plugins;
using Console.neo4jProxy;
using Newtonsoft.Json;

namespace Console.Controllers
{
    [Authorize(Policy = Console.Auth.Types.BirdsNestAdminsPolicy)]
    public class AdminController : Controller
    {
        private readonly PluginManager _pluginmanager;
        private readonly Neo4jService _service;

        public AdminController(PluginManager plugman, Neo4jService service)
        {
            this._pluginmanager = plugman;
            this._service = service;
        }

        // GET: api/Admin
        public IActionResult Index()
        {
            return View();
        }

        public object ReloadPlugins()
        {
            Dictionary<string,object> data = new Dictionary<string, object>();
            if (this._pluginmanager.Reload())
            {
                data.Add("status",200);
                data.Add("message", "Plugins updated OK");
            }
            else
            {
                data.Add("status", 500);
                data.Add("message", "There was an error reloading plugins. Please check the server logs");
            }
            return data;
        }

        // GET /admin/updateproperties?label=AD_Object
        public async Task<object> UpdateProperties([FromQuery]string label)
        {
            Dictionary<string, object> data = new Dictionary<string, object>();
            try
            {
                var ret = await this._service.UpdateMetadataAsync(label);
                data.Add("status", 200);
                data.Add("message", "OK");
            }
            catch (Exception e)
            {
                data.Add("status", 500);
                data.Add("message", "There was an error updating metadata for " +label+ ": " + e.Message);
            }

            return data;
        }
    }
}
