﻿using System;
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

        // GET reports/query?query
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
    }
}