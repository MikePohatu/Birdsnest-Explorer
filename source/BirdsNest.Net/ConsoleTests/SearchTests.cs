#region license
// Copyright (c) 2019-2020 "20Road"
// 20Road Limited [https://20road.com]
//
// This file is part of BirdsNest.
//
// BirdsNest is free software: you can redistribute it and/or modify
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
//
#endregion
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;
using Console.neo4jProxy;
using Newtonsoft.Json;

namespace ConsoleTests
{
    [TestFixture]
    public class SearchTests
    {
        [Test]
        [TestCase("{\"nodes\": [{ \"type\": \"AD_User\",\"identifier\": \"n1\"},{ \"type\": \"AD_Group\", \"identifier\": \"n2\"}],\"edges\": [{\"type\": \"AD_MEMBER_OF\",\"identifier\": \"e1\"}]}", 
            ExpectedResult = "MATCH p=(n1:AD_User)-[e1:AD_MEMBER_OF*0..1]->(n2:AD_Group) UNWIND nodes(p) as n RETURN DISTINCT n LIMIT 1000 ORDER BY LOWER(n.name)")]
        [TestCase("{\"nodes\": [{ \"type\": \"AD_User\",\"identifier\": \"n1\"},{ \"type\": \"AD_Group\", \"identifier\": \"n2\"}],\"edges\": [{\"type\": \"AD_MEMBER_OF\",\"identifier\": \"e1\"}]}",
            ExpectedResult = "MATCH p=(n1:AD_User)-[e1:AD_MEMBER_OF*0..1]->(n2:AD_Group) UNWIND nodes(p) as n RETURN DISTINCT n LIMIT 1000 ORDER BY LOWER(n.name)")]
        public string GenerateSearchStringTest(string json)
        {
            Search s = JsonConvert.DeserializeObject<Search>(json);
            return s.GetSearchString();
        }
    }
}
