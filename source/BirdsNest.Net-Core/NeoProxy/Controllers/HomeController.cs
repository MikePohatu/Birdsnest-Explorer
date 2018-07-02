using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.FileProviders;
using System.IO;

namespace NeoProxy.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            var file = Path.Combine(Directory.GetCurrentDirectory(),
                            "static", "index.htm");

            return PhysicalFile(file, "text/html");
            //return File("index.html", "text/html");
            //return View();
        }

        // GET /
        [HttpGet("{path}")]
        public IActionResult Get(string path)
        {
            var file = Path.Combine(Directory.GetCurrentDirectory(),
                            "static", path);

            return PhysicalFile(file, "text/html");
        }
    }
}