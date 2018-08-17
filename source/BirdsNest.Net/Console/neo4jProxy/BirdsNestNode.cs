using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Neo4j.Driver.V1;
using Newtonsoft.Json;

namespace Console.neo4jProxy
{
    public class BirdsNestNode
    {
        [JsonProperty("db_id")]
        public long DbId { get; private set; } = 0;

        [JsonProperty("name")]
        public string Name { get; private set; } = string.Empty;

        [JsonProperty("label")]
        public string Label { get; private set; } = string.Empty;

        [JsonProperty("scope")]
        public long Scope { get; private set; } = 1;

        [JsonProperty("properties")]
        public IReadOnlyDictionary<string,object> Properties { get; private set; }

        public static BirdsNestNode GetNode(INode noderecord)
        {
            BirdsNestNode newnode = new BirdsNestNode();

            newnode.DbId = noderecord.Id;
            newnode.Properties = noderecord.Properties;

            object o;
            if (newnode.Properties.TryGetValue("name", out o))
            {
                newnode.Name = o.ToString();
            }

            if (newnode.Properties.TryGetValue("scope", out o))
            {
                newnode.Scope = (long)o;
            }

            foreach (string s in noderecord.Labels)
            {
                newnode.Label = s;
                break;
            }

            return newnode;
        }
    }
}
