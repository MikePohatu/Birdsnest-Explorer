using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace Console.Controllers
{
    [Authorize]
    public class PortalController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}