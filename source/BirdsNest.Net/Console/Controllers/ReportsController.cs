using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Console.neo4jProxy;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Console.ActiveDirectory;

namespace Console.Controllers
{
    [Authorize]
    public class ReportsController : Controller
    {
        private readonly Neo4jService _service;
        private readonly ILogger _logger;

        public ReportsController(ILogger<ReportsController> logger, Neo4jService service)
        {
            this._logger = logger;
            this._service = service;
        }

        public IActionResult Index()
        {
            return View();
        }

        // GET reports/emptygroups
        public IActionResult EmptyGroups()
        {
            string query = ADQueries.EmptyGroups;
            
            ResultSet results = _service.GetResultSetFromQuery(query);
            results.AddPropertyFilter("name");
            results.AddPropertyFilter("dn");
            results.AddPropertyFilter("description");
            //results.AddPropertyFilter("scope");

            ViewData["Query"] = query;
            ViewData["Title"] = "Empty Groups";
            return View("TableView", results);
        }

        // GET reports/grouploops
        public IActionResult GroupLoops()
        {
            string query = ADQueries.GroupLoops;
            ResultSet results = _service.GetResultSetFromQuery(query);
            results.AddPropertyFilter("name");
            results.AddPropertyFilter("dn");
            results.AddPropertyFilter("description");
            results.AddPropertyFilter("scope");

            ViewData["Query"] = query;
            ViewData["Title"] = "Group Loops";
            return View("TableView", results);
        }

    }
}