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
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;
using Console.neo4jProxy.AdvancedSearch.Conditions;
using Console.neo4jProxy.AdvancedSearch;
using Newtonsoft.Json;

namespace ConsoleTests
{
    [TestFixture]
    public class AdvancedSearchTests
    {
        [Test]
        [TestCase(@"{
    ""condition"": {
        ""type"": ""AND"",
        ""conditions"": [
            {
                ""type"": ""string"",
                ""name"": ""n1"",
                ""property"": ""tpropn1"",
                ""value"": ""testn1"",
                ""operator"": ""=""
            },
            {
                ""type"": ""OR"",
                ""conditions"": [
                    {
                        ""type"": ""string"",
                        ""name"": ""n2"",
                        ""property"": ""tpropn2"",
                        ""value"": ""testn2"",
                        ""operator"": ""StartsWith""
                    },
                    {
                        ""type"": ""string"",
                        ""name"": ""r1"",
                        ""property"": ""tpropr1"",
                        ""value"": ""testr1"",
                        ""operator"": ""=""
                    }
                ]
            }
        ]
    }
}", 
            ExpectedResult = "(n1.tpropn1 = 'testn1' AND (n2.tpropn2 STARTS WITH 'testn2' OR r1.tpropr1 = 'testr1'))")]
        [TestCase(@"{
    ""condition"": {
        ""type"": ""AND"",
        ""conditions"": [{
            ""type"": ""number"",
            ""name"": ""n1"",
            ""property"": ""tpropn1"",
            ""value"": 1,
            ""operator"": ""=""
        },
        {
            ""type"": ""OR"",
            ""conditions"": [{
                ""type"": ""string"",
                ""name"": ""n2"",
                ""property"": ""tpropn2"",
                ""value"": ""testn2"",
                ""operator"": ""StartsWith""
            },
            {
                ""type"": ""string"",
                ""name"": ""r1"",
                ""property"": ""tpropr1"",
                ""value"": ""testr1"",
                ""operator"": ""=""
            }]
        }]
    }
}",
            ExpectedResult = "(n1.tpropn1 = 1 AND (n2.tpropn2 STARTS WITH 'testn2' OR r1.tpropr1 = 'testr1'))")]
        [TestCase(@"{
    ""condition"": {
        ""type"": ""AND"",
        ""conditions"": [{
            ""type"": ""number"",
            ""name"": ""n1"",
            ""property"": ""tpropn1"",
            ""value"": 1,
            ""operator"": ""<""
        },
        {
            ""type"": ""OR"",
            ""conditions"": [{
                ""type"": ""string"",
                ""name"": ""n2"",
                ""property"": ""tpropn2"",
                ""value"": ""testn2"",
                ""operator"": ""StartsWith""
            },
            {
                ""type"": ""string"",
                ""name"": ""r1"",
                ""property"": ""tpropr1"",
                ""value"": ""testr1"",
                ""operator"": ""=""
            }]
        }]
    }
}",
            ExpectedResult = "(n1.tpropn1 < 1 AND (n2.tpropn2 STARTS WITH 'testn2' OR r1.tpropr1 = 'testr1'))")]
        [TestCase(@"{
    ""condition"": {
        ""type"": ""AND"",
        ""conditions"": [{
            ""type"": ""number"",
            ""name"": ""n1"",
            ""property"": ""tpropn1"",
            ""value"": 1,
            ""operator"": ""<""
        },
        {
            ""type"": ""OR"",
            ""conditions"": [{
                ""type"": ""string"",
                ""name"": ""n2"",
                ""property"": ""tpropn2"",
                ""value"": ""testn2"",
                ""operator"": ""StartsWith""
            },
            {
                ""type"": ""string"",
                ""name"": ""r1"",
                ""property"": ""tpropr1"",
                ""value"": ""testr1"",
                ""operator"": ""=""
            },
            {
                ""type"": ""string"",
                ""name"": ""r2"",
                ""property"": ""tpropr2"",
                ""value"": ""testr2"",
                ""operator"": ""=""
            }]
        }]
    }
}",
            ExpectedResult = "(n1.tpropn1 < 1 AND (n2.tpropn2 STARTS WITH 'testn2' OR r1.tpropr1 = 'testr1' OR r2.tpropr2 = 'testr2'))")]
        public string GenerateSearchConditionStringTest(string json)
        {
            Search s = JsonConvert.DeserializeObject<Search>(json);
            return s.Condition.ToSearchString();
        }
    }
}
