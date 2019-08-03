using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Console.neo4jProxy.AdvancedSearch;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace Console.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class AdvancedSearchController : ControllerBase
    {
        //// GET: api/AdvancedSearch
        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new string[] { "test", "data" };
        }

        //// GET: api/AdvancedSearch/5
        //[HttpGet("{id}", Name = "Get")]
        //public string Get(int id)
        //{
        //    return "value";
        //}

        // POST: api/AdvancedSearch
        [HttpPost]
        public object Post([FromBody] Search search)
        {
            //Search s = JsonConvert.DeserializeObject<Search>(json);
            return search.ToSearchString();
        }

        //// PUT: api/AdvancedSearch/5
        //[HttpPut("{id}")]
        //public void Put(int id, [FromBody] string value)
        //{
        //}

        //// DELETE: api/ApiWithActions/5
        //[HttpDelete("{id}")]
        //public void Delete(int id)
        //{
        //}
    }
}
