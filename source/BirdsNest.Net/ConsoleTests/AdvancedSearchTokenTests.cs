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
    public class AdvancedSearchTokenTests
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
            ExpectedResult = "(token1.tpropn1 = $val1 AND (token2.tpropn2 STARTS WITH $val2 OR token3.tpropr1 = $val3))")]
        [TestCase(@"{
    ""Condition"": {
        ""Type"": ""AND"",
        ""Conditions"": [
            {
                ""Type"": ""STRING"",
                ""Name"": ""n1"",
                ""Property"": ""tpropn1"",
                ""Value"": ""testn1"",
                ""Operator"": ""EQUALS"",
                ""CaseSensitive"":false
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
            ExpectedResult = "(token1.tpropn1 =~ $val1 AND (token2.tpropn2 STARTS WITH $val2 OR token3.tpropr1 = $val3))")]
        public string GenerateSearchTokenStringTest(string json)
        {
            Search s = JsonConvert.DeserializeObject<Search>(json);
            s.Tokenize();
            return s.Condition.ToTokenizedSearchString();
        }
    }
}
