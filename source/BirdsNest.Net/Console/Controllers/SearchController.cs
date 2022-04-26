#region license
// Copyright (c) 2019-2020 "20Road"
// 20Road Limited [https://www.20road.com]
//
// This file is part of Birdsnest Explorer.
//
// Birdsnest Explorer is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, version 3 of the License.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.
#endregion
using Console.neo4jProxy;
using Console.neo4jProxy.AdvancedSearch;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Console.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class SearchController : ControllerBase
    {
        private readonly Neo4jService _service;
        private readonly ILogger<SearchController> _logger;

        public SearchController(ILogger<SearchController> logger, Neo4jService service)
        {
            this._service = service;
            this._logger = logger;
        }

        [ValidateAntiForgeryToken]
        [HttpPost("advancedsearch")]
        public async Task<ResultSet> AdvancedSearch(Search search)
        {
            this._logger.LogInformation("{0} executed search: {1}", User.FindFirst(ClaimTypes.Name).Value, search.ToSearchString());
            return await this._service.AdvancedSearch(search);
        }

        [ValidateAntiForgeryToken]
        [HttpPost("advancedsearchcypher")]
        public object AdvancedSearchCypher(Search search)
        {
            return search.ToSharableSearchString();
        }

        public async Task<ResultSet> SimpleSearch(string searchterm)
        {
            return await this._service.SimpleSearch(searchterm);
        }
    }
}