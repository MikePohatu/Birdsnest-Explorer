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
using Console.neo4jProxy.AdvancedSearch;
using Newtonsoft.Json;
using NUnit.Framework;

namespace ConsoleTests
{
    [TestFixture]
    public class SearchTests
    {
        [Test]
        [TestCase(@"
            {
                ""nodes"": [
                    {
                        ""label"": ""AD_User"",
                        ""name"": ""n1""
                    },
                    {
                        ""label"": ""AD_Group"",
                        ""name"": ""n2""
                    }
                ],
                ""edges"": [
                    {
                        ""label"": ""AD_MEMBER_OF"",
                        ""name"": ""e1""
                    }
                ]
            }
        ",
            ExpectedResult = "MATCH p=(n1:AD_User)-[:AD_MEMBER_OF*]->(n2:AD_Group) UNWIND nodes(p) as bnest_nodes RETURN DISTINCT bnest_nodes ORDER BY LOWER(bnest_nodes.name)")]
        [TestCase(@"
            {
                ""nodes"": [
                    {
                        ""label"": ""AD_User"",
                        ""name"": ""n1""
                    },
                    {
                        ""label"": ""AD_Group"",
                        ""name"": ""n2""
                    }
                ],
                ""edges"": [
                    {
                        ""label"": ""AD_MEMBER_OF"",
                        ""name"": ""e1"",
                        ""min"": 0,
                        ""max"": 1
                    }
                ]
            }
        ",
            ExpectedResult = "MATCH p=(n1:AD_User)-[e1:AD_MEMBER_OF*0..1]->(n2:AD_Group) UNWIND nodes(p) as bnest_nodes RETURN DISTINCT bnest_nodes ORDER BY LOWER(bnest_nodes.name)")]
        [TestCase(@"
            {
                ""nodes"": [
                    {
                        ""label"": """",
                        ""name"": ""n1""
                    },
                    {
                        ""label"": ""AD_Group"",
                        ""name"": ""n2""
                    }
                ],
                ""edges"": [
                    {
                        ""label"": ""AD_MEMBER_OF"",
                        ""name"": ""e1"",
                        ""min"": 0,
                        ""max"": 1
                    }
                ]
            }
        ",
        ExpectedResult = "MATCH p=()-[e1:AD_MEMBER_OF*0..1]->(n2:AD_Group) UNWIND nodes(p) as bnest_nodes RETURN DISTINCT bnest_nodes ORDER BY LOWER(bnest_nodes.name)")]
        [TestCase(@"
            {
                ""nodes"": [
                    {
                        ""label"": """",
                        ""name"": ""node1""
                    },
                    {
                        ""label"": ""AD_Group"",
                        ""name"": ""node2""
                    }
                ],
                ""edges"": [
                    {
                        ""label"": """",
                        ""name"": ""hop1"",
                        ""min"": 1,
                        ""max"": 7
                    }
                ]
            }
        ",
        ExpectedResult = "MATCH p=()-[*1..7]->(node2:AD_Group) UNWIND nodes(p) as bnest_nodes RETURN DISTINCT bnest_nodes ORDER BY LOWER(bnest_nodes.name)")]
        public string GenerateSearchStringTest(string json)
        {
            Search s = JsonConvert.DeserializeObject<Search>(json);
            return s.ToSearchString();
        }
    }
}
