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
        private readonly Queries _adqueries;

        public ReportsController(ILogger<ReportsController> logger, Neo4jService service)
        {
            this._logger = logger;
            this._service = service;
            this._adqueries = new Queries(this._service);
        }

        public IActionResult Index()
        {
            return View();
        }

        // GET reports/emptygroups
        public IActionResult EmptyGroups()
        {
            
            ResultSet results = this._adqueries.GetEmptyGroups();
            results.AddPropertyFilter("name");
            results.AddPropertyFilter("dn");
            //results.AddPropertyFilter("scope");

            ViewData["Title"] = "Empty Groups";
            return View("TableView", results);
        }

        // GET reports/grouploops
        public IActionResult GroupLoops()
        {

            ResultSet results = this._adqueries.GetGroupLoops();
            results.AddPropertyFilter("name");
            results.AddPropertyFilter("dn");
            //results.AddPropertyFilter("scope");

            ViewData["Title"] = "Group Loops";
            return View("TableView", results);
        }

    }
}