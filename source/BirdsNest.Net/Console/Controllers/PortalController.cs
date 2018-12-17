using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;

namespace Console.Controllers
{
    [Authorize]
    public class PortalController : Controller
    {
        private readonly ILogger _logger;
        public PortalController(ILogger<PortalController> logger)
        {
            this._logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }
    }
}