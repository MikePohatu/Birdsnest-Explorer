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

        // GET visualizer/id=1&id=2&usestored=false&sharedsearch=b64_encoded_json
        public IActionResult Index([FromQuery(Name = "id")]List<long> ids, [FromQuery]string sharedsearch)
        {
            ViewData["LoadIDs"] = ids;
            ViewData["SharedSearchString"] = sharedsearch;
            return View();
        }

        // GET visualizer/ASearchTest
        public IActionResult ASearchTest()
        {
            return View("ASearchTest");
        }

        // GET visualizer/details
        public IActionResult NodeDetails(long id)
        {
            ResultSet set = this._service.GetNode(id);
            BirdsNestNode node = set.Nodes.First();
            ResultSet resultset = this._service.GetAllRelated(id);
            NodeRelatedDetailViewModel model = new NodeRelatedDetailViewModel(node, resultset);

            ViewBag.ID = id;
            return PartialView("NodeRelatedDetail",model);
        }


        // GET visualizer/edgedetails
        public IActionResult EdgeDetails(long id)
        {
            ResultSet set = this._service.GetEdge(id);
            BirdsNestRelationship edge = set.Edges.First();

            ViewBag.ID = id;
            return PartialView("EdgeDetail", edge);
        }


        public object Icons()
        {
            return this._plugingmanager.Icons;
        }

        public object SubTypeProperties()
        {
            return this._plugingmanager.SubTypeProperties;
        }

        public object NodePropertyDetails()
        {
            return this._plugingmanager.NodePropertyDetails;
        }

        [HttpPost]
        public IActionResult AdvancedSearch([FromBody] neo4jProxy.AdvancedSearch.Search search)
        {
            ResultSet results = this._service.AdvancedSearch(search);
            return PartialView("SearchResultsDetail", results);
        }

        [HttpPost]
        public object AdvancedSearchCypher([FromBody] neo4jProxy.AdvancedSearch.Search search)
        {
            return search.ToSharableSearchString();
        }

        public IActionResult SimpleSearch(string searchterm)
		{
			ResultSet results = this._service.SimpleSearch(searchterm);
			return PartialView("SearchResultsDetail", results);
		}
	}
}