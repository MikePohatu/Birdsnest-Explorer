using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Console.Plugins;
using Newtonsoft.Json;

namespace Console.Controllers
{
    [Authorize(Policy = Console.Auth.Types.BirdsNestAdminsPolicy)]
    public class AdminController : Controller
    {
        private readonly PluginManager _pluginmanager;

        public AdminController(PluginManager plugman)
        {
            this._pluginmanager = plugman;
        }

        // GET: api/Admin
        public IActionResult Index()
        {
            return View();
        }

        public object ReloadPlugins()
        {
            Dictionary<string,object> data = new Dictionary<string, object>();
            if (this._pluginmanager.Reload())
            {
                data.Add("status",200);
                data.Add("message", "Plugins updated OK");
            }
            else
            {
                data.Add("status", 500);
                data.Add("message", "There was an error reloading plugins. Please check the server logs");
            }
            return data;
        }
    }
}
