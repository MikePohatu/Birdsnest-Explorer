using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Console.Controllers
{
    [Authorize(Policy = "IsBirdsNestUser")]
    public class VisualizerController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}