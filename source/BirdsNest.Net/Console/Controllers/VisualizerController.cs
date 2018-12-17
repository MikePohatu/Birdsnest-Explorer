using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Console.neo4jProxy;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Console.ViewModels;
using Microsoft.Extensions.Logging;

namespace Console.Controllers
{
    [Authorize]
    public class VisualizerController : Controller
    {
        private readonly Neo4jService _service;
        private readonly ILogger _logger;

        public VisualizerController(ILogger<VisualizerController> logger, Neo4jService service)
        {
            this._logger = logger;
            this._service = service;
        }

        public IActionResult Index()
        {
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
            [FromQuery]string tartype, [FromQuery]string tarprop, [FromQuery]string tarval)
        {
            ResultSet results = this._service.SearchPath(sourcetype, sourceprop, sourceval, relationship, relmin, relmax, dir, tartype, tarprop, tarval);
            return PartialView("SearchResultsDetail", results);
        }
    }
}