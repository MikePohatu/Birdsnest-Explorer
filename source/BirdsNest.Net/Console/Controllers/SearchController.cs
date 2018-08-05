using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Console.neo4jProxy;
using Microsoft.AspNetCore.Authorization;

namespace Console.Controllers
{
    [Authorize]
    [Produces("application/json")]
    [Route("api/Search")]
    public class SearchController : Controller
    {
        private readonly Neo4jService _service;
        public SearchController(Neo4jService service)
        {
            this._service = service;
        }
        // GET: api/search/path
        [HttpGet()]
        public string GetNodeValues()
        {
            return "";
        }

        // GET: api/search/path
        [HttpGet("path")]
        public ResultSet GetNodeValues([FromQuery]string sourcetype, [FromQuery]string sourceprop, [FromQuery]string sourceval,
            [FromQuery]string relationship, [FromQuery]int relmin, [FromQuery]int relmax, [FromQuery]char dir,
            [FromQuery]string tartype, [FromQuery]string tarprop, [FromQuery]string tarval)
        {
            return this._service.SearchPath(sourcetype, sourceprop, sourceval, relationship, relmin, relmax, dir, tartype, tarprop, tarval);
        }
    }
}
