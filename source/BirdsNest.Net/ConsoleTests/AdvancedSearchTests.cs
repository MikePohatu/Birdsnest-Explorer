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
                ""Comparator"": ""EQUALS""
            },
            {
                ""Type"": ""OR"",
                ""Conditions"": [
                    {
                        ""Type"": ""STRING"",
                        ""Name"": ""n2"",
                        ""Property"": ""tpropn2"",
                        ""Value"": ""testn2"",
                        ""Comparator"": ""STARTSWITH""
                    },
                    {
                        ""Type"": ""STRING"",
                        ""Name"": ""r1"",
                        ""Property"": ""tpropr1"",
                        ""Value"": ""testr1"",
                        ""Comparator"": ""EQUALS""
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
                ""Comparator"": ""STARTSWITH""
            },
            {
                ""Type"": ""STRING"",
                ""Name"": ""r1"",
                ""Property"": ""tpropr1"",
                ""Value"": ""testr1"",
                ""Comparator"": ""EQUALS""
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
                ""Comparator"": ""STARTSWITH""
            },
            {
                ""Type"": ""STRING"",
                ""Name"": ""r1"",
                ""Property"": ""tpropr1"",
                ""Value"": ""testr1"",
                ""Comparator"": ""EQUALS""
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
                ""Comparator"": ""STARTSWITH""
            },
            {
                ""Type"": ""STRING"",
                ""Name"": ""r1"",
                ""Property"": ""tpropr1"",
                ""Value"": ""testr1"",
                ""Comparator"": ""EQUALS""
            },
            {
                ""Type"": ""STRING"",
                ""Name"": ""r2"",
                ""Property"": ""tpropr2"",
                ""Value"": ""testr2"",
                ""Comparator"": ""EQUALS""
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
