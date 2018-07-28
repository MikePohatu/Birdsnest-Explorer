using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NeoProxy;

namespace NeoProxy.Controllers
{
    [Produces("application/json")]
    [Route("api/Search")]
    public class SearchController : Controller
    {
        private readonly Neo4jService _service;
        public SearchController(Neo4jService service)
        {
            this._service = service;
        }

        // GET: api/NodeNames
        [HttpGet("NodeNames")]
        public IEnumerable<string> SearchNodes([FromQuery]string term, [FromQuery]int limit)
        {
            return this._service.SearchNodeNames(term, limit);
        }

        // GET: api/Search
        //[HttpGet]
        //public IEnumerable<string> Get()
        //{
        //    return new string[] { "value1", "value2" };
        //}

        // GET: api/Search
        [HttpGet]
        public object Get()
        {
            return this._service.GetAll();
        }

        // POST: api/Search/5
        //[HttpPost("{id}", Name = "Get")]
        //public string Get(int id)
        //{
        //    return "value";
        //}

        // GET: api/Search/node/5
        [HttpGet("node/{id}")]
        public object Get(long id)
        {
            return this._service.GetNode(id);
        }

        //// GET: api/Search/relatednodes/5
        //[HttpGet("relatednodes/{id}")]
        //public object Get(long id)
        //{
        //    return this._service.GetNode(id);
        //}

        //// Get all edges for a node
        //// POST: api/Search/edges
        //[HttpGet("edges/{id}")]
        //public object Get(long id)
        //{
        //    List<long> idlist = new List<long>();
        //    idlist.Add(id);
        //    return this._service.GetRelationships(idlist);
        //}

        // POST: api/Search/edges
        [HttpGet("edges")]
        public object Get([FromBody]List<long> value)
        {
            return this._service.GetRelationships(value);
        }

        // POST: api/Search
        //[HttpPost]
        //public void Post("{relationships}", [FromBody]string value)
        //{
        //}

        // PUT: api/Search/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody]string value)
        {
        }
        
        // DELETE: api/ApiWithActions/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
