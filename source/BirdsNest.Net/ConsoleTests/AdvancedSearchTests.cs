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
using Newtonsoft.Json;
using Console.neo4jProxy.AdvancedSearch.Conditions;
using Console.neo4jProxy.AdvancedSearch;

namespace ConsoleTests
{
    [TestFixture]
    public class AdvancedSearchTests
    {
        [Test]
        [TestCase(@"{
    ""Condition"": {
        ""Type"": ""AND"",
        ""Conditions"": [
            {
                ""Type"": ""STRING"",
                ""Name"": ""n1"",
                ""Property"": ""tpropn1"",
                ""Value"": ""testn1"",
                ""Operator"": ""EQUALS""
            },
            {
                ""Type"": ""OR"",
                ""Conditions"": [
                    {
                        ""Type"": ""STRING"",
                        ""Name"": ""n2"",
                        ""Property"": ""tpropn2"",
                        ""Value"": ""testn2"",
                        ""Operator"": ""STARTSWITH""
                    },
                    {
                        ""Type"": ""STRING"",
                        ""Name"": ""r1"",
                        ""Property"": ""tpropr1"",
                        ""Value"": ""testr1"",
                        ""Operator"": ""EQUALS""
                    }
                ]
            }
        ]
    }
}", 
            ExpectedResult = "(n1.tpropn1 = 'testn1' AND (n2.tpropn2 STARTS WITH 'testn2' OR r1.tpropr1 = 'testr1'))")]
        [TestCase(@"{
    ""Condition"": {
        ""Type"": ""AND"",
        ""Conditions"": [{
            ""Type"": ""MATH"",
            ""Name"": ""n1"",
            ""Property"": ""tpropn1"",
            ""Value"": 1,
            ""Operator"": ""=""
        },
        {
            ""Type"": ""OR"",
            ""Conditions"": [{
                ""Type"": ""STRING"",
                ""Name"": ""n2"",
                ""Property"": ""tpropn2"",
                ""Value"": ""testn2"",
                ""Operator"": ""STARTSWITH""
            },
            {
                ""Type"": ""STRING"",
                ""Name"": ""r1"",
                ""Property"": ""tpropr1"",
                ""Value"": ""testr1"",
                ""Operator"": ""EQUALS""
            }]
        }]
    }
}",
            ExpectedResult = "(n1.tpropn1 = 1 AND (n2.tpropn2 STARTS WITH 'testn2' OR r1.tpropr1 = 'testr1'))")]
        [TestCase(@"{
    ""Condition"": {
        ""Type"": ""AND"",
        ""Conditions"": [{
            ""Type"": ""MATH"",
            ""Name"": ""n1"",
            ""Property"": ""tpropn1"",
            ""Value"": 1,
            ""Operator"": ""<""
        },
        {
            ""Type"": ""OR"",
            ""Conditions"": [{
                ""Type"": ""STRING"",
                ""Name"": ""n2"",
                ""Property"": ""tpropn2"",
                ""Value"": ""testn2"",
                ""Operator"": ""STARTSWITH""
            },
            {
                ""Type"": ""STRING"",
                ""Name"": ""r1"",
                ""Property"": ""tpropr1"",
                ""Value"": ""testr1"",
                ""Operator"": ""EQUALS""
            }]
        }]
    }
}",
            ExpectedResult = "(n1.tpropn1 < 1 AND (n2.tpropn2 STARTS WITH 'testn2' OR r1.tpropr1 = 'testr1'))")]
        [TestCase(@"{
    ""Condition"": {
        ""Type"": ""AND"",
        ""Conditions"": [{
            ""Type"": ""MATH"",
            ""Name"": ""n1"",
            ""Property"": ""tpropn1"",
            ""Value"": 1,
            ""Operator"": ""<""
        },
        {
            ""Type"": ""OR"",
            ""Conditions"": [{
                ""Type"": ""STRING"",
                ""Name"": ""n2"",
                ""Property"": ""tpropn2"",
                ""Value"": ""testn2"",
                ""Operator"": ""STARTSWITH""
            },
            {
                ""Type"": ""STRING"",
                ""Name"": ""r1"",
                ""Property"": ""tpropr1"",
                ""Value"": ""testr1"",
                ""Operator"": ""EQUALS""
            },
            {
                ""Type"": ""STRING"",
                ""Name"": ""r2"",
                ""Property"": ""tpropr2"",
                ""Value"": ""testr2"",
                ""Operator"": ""EQUALS""
            }]
        }]
    }
}",
            ExpectedResult = "(n1.tpropn1 < 1 AND (n2.tpropn2 STARTS WITH 'testn2' OR r1.tpropr1 = 'testr1' OR r2.tpropr2 = 'testr2'))")]
        public string GenerateSearchStringTest(string json)
        {
            Search s = JsonConvert.DeserializeObject<Search>(json);
            return s.Condition.ToSearchString();
        }
    }
}
