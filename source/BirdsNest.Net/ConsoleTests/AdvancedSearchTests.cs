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
        [TestCase(@"{ ""condition"":{ ""type"":""AND"", ""left"":{ ""type"":""STRING"", ""name"":""n1"", ""property"":""tpropn1"", ""value"":""testn1"", ""comparator"":""EQUALS"" }, ""right"":{ ""type"":""OR"", ""left"":{ ""type"":""STRING"", ""name"":""n2"", ""property"":""tpropn2"", ""value"":""testn2"", ""comparator"":""STARTSWITH"" }, ""right"":{ ""type"":""STRING"", ""name"":""r1"", ""property"":""tpropr1"", ""value"":""testr1"", ""comparator"":""EQUALS"" } } } }", 
            ExpectedResult = "(n1.tpropn1 = 'testn1' AND (n2.tpropn2 STARTS WITH 'testn2' OR r1.tpropr1 = 'testr1'))")]
        [TestCase(@"{ ""condition"": { ""type"": ""AND"", ""left"": { ""type"": ""MATH"", ""name"": ""n1"", ""property"": ""tpropn1"", ""value"": 1, ""operator"": ""="" }, ""right"": { ""type"": ""OR"", ""left"": { ""type"": ""STRING"", ""name"": ""n2"", ""property"": ""tpropn2"", ""value"": ""testn2"", ""comparator"": ""STARTSWITH"" }, ""right"": { ""type"": ""STRING"", ""name"": ""r1"", ""property"": ""tpropr1"", ""value"": ""testr1"", ""comparator"": ""EQUALS"" } } } }",
            ExpectedResult = "(n1.tpropn1 = 1 AND (n2.tpropn2 STARTS WITH 'testn2' OR r1.tpropr1 = 'testr1'))")]
        [TestCase(@"{ ""condition"": { ""type"": ""AND"", ""left"": { ""type"": ""MATH"", ""name"": ""n1"", ""property"": ""tpropn1"", ""value"": 1, ""operator"": ""<"" }, ""right"": { ""type"": ""OR"", ""left"": { ""type"": ""STRING"", ""name"": ""n2"", ""property"": ""tpropn2"", ""value"": ""testn2"", ""comparator"": ""STARTSWITH"" }, ""right"": { ""type"": ""STRING"", ""name"": ""r1"", ""property"": ""tpropr1"", ""value"": ""testr1"", ""comparator"": ""EQUALS"" } } } }",
            ExpectedResult = "(n1.tpropn1 < 1 AND (n2.tpropn2 STARTS WITH 'testn2' OR r1.tpropr1 = 'testr1'))")]
        public string GenerateSearchStringTest(string json)
        {
            Search s = JsonConvert.DeserializeObject<Search>(json);
            return s.Condition.ToSearchString();
        }
    }
}
