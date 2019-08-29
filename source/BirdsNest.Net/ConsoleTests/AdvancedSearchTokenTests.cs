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
            ExpectedResult = "(token1.tpropn1 = $props.val1 AND (token2.tpropn2 STARTS WITH $props.val2 OR token3.tpropr1 = $props.val3))")]
        public string GenerateSearchTokenStringTest(string json)
        {
            Search s = JsonConvert.DeserializeObject<Search>(json);
            s.Tokenize();
            return s.Condition.ToTokenizedSearchString();
        }
    }
}
