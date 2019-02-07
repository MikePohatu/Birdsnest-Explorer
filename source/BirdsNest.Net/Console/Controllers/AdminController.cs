using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Console.Controllers
{
    [Authorize(Policy = Console.Auth.Types.BirdsNestAdminsPolicy)]
    public class AdminController : Controller
    {
        // GET: api/Admin
        public IActionResult Index()
        {
            return View();
        }
    }
}
