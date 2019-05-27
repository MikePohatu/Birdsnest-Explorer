using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Console.neo4jProxy;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Console.ViewModels;
using Microsoft.Extensions.Logging;
using Console.Plugins;

namespace Console.Controllers
{
    [Authorize]
    public class VisualizerController : Controller
    {
        private readonly Neo4jService _service;
        private readonly ILogger _logger;
        private readonly PluginManager _plugingmanager;

        public VisualizerController(ILogger<VisualizerController> logger, Neo4jService service, PluginManager plugman)
        {
            this._logger = logger;
            this._service = service;
            this._plugingmanager = plugman;
        }

        // GET visualizer/id=1&id=2&usestored=false
        public IActionResult Index([FromQuery(Name = "id")]List<long> ids, bool usestored)
        {
            ViewData["LoadIDs"] = ids;
            ViewData["UseStored"] = usestored;
            return View();
        }

        // GET visualizer/details
        public IActionResult Details(long id)
        {
            ResultSet set = this._service.GetNode(id);
            BirdsNestNode node = set.Nodes.First();
            ResultSet resultset = this._service.GetAllRelated(id);
            RelatedDetailViewModel model = new RelatedDetailViewModel(node, resultset);

            ViewBag.ID = id;
            return PartialView("RelatedDetail",model);
        }

        // GET visualizer/search
        public IActionResult Search([FromQuery]string sourcetype, [FromQuery]string sourceprop, [FromQuery]string sourceval,
            [FromQuery]string relationship, [FromQuery]int relmin, [FromQuery]int relmax, [FromQuery]char dir,
            [FromQuery]string targettype, [FromQuery]string targetprop, [FromQuery]string targetval)
        {
            ResultSet results = this._service.SearchPath(sourcetype, sourceprop, sourceval, relationship, relmin, relmax, dir, targettype, targetprop, targetval);
            return PartialView("SearchResultsDetail", results);
        }

        // GET visualizer/friendlysearch
        public IActionResult FriendlySearch([FromQuery]string sourcetype, [FromQuery]string sourceprop, [FromQuery]string sourceval,
            [FromQuery]string relationship, [FromQuery]int relmin, [FromQuery]int relmax, [FromQuery]char dir,
            [FromQuery]string targettype, [FromQuery]string targetprop, [FromQuery]string targetval)
        {
            ResultSet results = this._service.SearchPath(sourcetype, sourceprop, sourceval, relationship, relmin, relmax, dir, targettype, targetprop, targetval);
            return PartialView("SearchResultsDetail", results);
        }

        public object Icons()
        {
            return this._plugingmanager.Icons;
        }

        public object SubTypeProperties()
        {
            return this._plugingmanager.SubTypeProperties;
        }
    }
}