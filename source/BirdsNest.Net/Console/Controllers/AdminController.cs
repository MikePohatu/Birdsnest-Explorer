using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Console.Plugins;
using Console.neo4jProxy;
using Newtonsoft.Json;

namespace Console.Controllers
{
    [Authorize(Policy = Console.Auth.Types.BirdsNestAdminsPolicy)]
    public class AdminController : Controller
    {
        private readonly PluginManager _pluginmanager;
        private readonly Neo4jService _service;

        public AdminController(PluginManager plugman, Neo4jService service)
        {
            this._pluginmanager = plugman;
            this._service = service;
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

        // GET /admin/updateproperties?label=AD_Object
        public async Task<object> UpdateProperties([FromQuery]string label)
        {
            Dictionary<string, object> data = new Dictionary<string, object>();
            try
            {
                var ret = await this._service.UpdateMetadataAsync(label);
                data.Add("status", 200);
                data.Add("message", "OK");
            }
            catch (Exception e)
            {
                data.Add("status", 500);
                data.Add("message", "There was an error updating metadata for " +label+ ": " + e.Message);
            }

            return data;
        }
    }
}
