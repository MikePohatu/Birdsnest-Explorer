using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Console.neo4jProxy;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Console.ViewModels;

namespace Console.Controllers
{
    [Authorize]
    public class VisualizerController : Controller
    {
        private readonly Neo4jService _service;
        public VisualizerController(Neo4jService service)
        {
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
            RelatedDetailViewModel model = new RelatedDetailViewModel(node);

            set = this._service.GetAllRelated(id);
            foreach (BirdsNestNode newnode in set.Nodes)
            {
                model.AddRelatedNode(newnode);
            }
            foreach (BirdsNestRelationship newrel in set.Edges)
            {
                model.AddDirectEdge(newrel);
            }

            ViewBag.ID = id;
            return PartialView("RelatedDetail",model);
        }
    }
}