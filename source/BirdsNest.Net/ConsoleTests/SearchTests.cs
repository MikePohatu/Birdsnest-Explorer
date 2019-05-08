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
