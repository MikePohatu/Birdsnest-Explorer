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
using Console.neo4jProxy;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Console.Plugins;

namespace Console.Controllers
{
    [Authorize]
    public class ReportsController : Controller
    {
        private readonly Neo4jService _service;
        private readonly ILogger _logger;
        private readonly PluginManager _pluginmanager;

        public ReportsController(ILogger<ReportsController> logger, Neo4jService service, PluginManager plugman)
        {
            this._logger = logger;
            this._service = service;
            this._pluginmanager = plugman;
        }

        public IActionResult Index()
        {
            return View();
        }

        // GET reports/pluginquery?pluginname=***&reportname=****
        public IActionResult PluginQuery(string pluginname, string reportname)
        {
            Plugin plugin;
            this._pluginmanager.Plugins.TryGetValue(pluginname, out plugin);
            Report rep;
            plugin.Reports.TryGetValue(reportname, out rep);

            ResultSet results = _service.GetResultSetFromQuery(rep.Query);
            foreach (string filter in rep.PropertyFilters)
            { results.AddPropertyFilter(filter); }

            ViewData["Query"] = rep.Query;
            return View("TableView", results);
        }

        // GET reports/nodesquery?id=1&id=2
        public IActionResult NodesQuery([FromQuery(Name = "id")]List<long> ids, [FromQuery(Name = "property")]List<string> propertyfilters)
        {
            ResultSet results = this._service.GetNodes(ids);
            foreach (string filter in propertyfilters)
            { results.AddPropertyFilter(filter); }

            ViewData["Query"] = "";
            return View("TableView", results);
        }
    }
}