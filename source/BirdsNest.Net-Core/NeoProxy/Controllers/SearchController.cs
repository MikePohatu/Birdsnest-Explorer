using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using common;
using Neo4j.Driver.V1;
using Newtonsoft.Json;

namespace NeoProxy.Controllers
{
    [Produces("application/json")]
    [Route("api/Search")]
    public class SearchController : Controller
    {
        private readonly NeoService _service;
        public SearchController(NeoService service)
        {
            this._service = service;
        }

        // GET: api/Search
        [HttpGet]
        public string Get()
        {
            return this._service.GetAll();
        }

        // GET: api/Search/5
        [HttpGet("{id}", Name = "Get")]
        public string Get(int id)
        {
            return "value";
        }
    }
}
