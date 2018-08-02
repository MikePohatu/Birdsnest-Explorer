using Neo4j.Driver.V1;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Console.neo4jProxy
{
    public class BirdsNestRelationship
    {
        [JsonProperty("db_id")]
        public long DbId { get; private set; } = 0;

        [JsonProperty("source")]
        public long Source { get; private set; } = 0;

        [JsonProperty("target")]
        public long Target { get; private set; } = 0;

        [JsonProperty("bidir")]
        public bool Bidirectional { get; private set; } = false;

        [JsonProperty("label")]
        public string Label { get; private set; } = string.Empty;

        [JsonProperty("properties")]
        public IReadOnlyDictionary<string, object> Properties { get; private set; }

        public static BirdsNestRelationship GetRelationship(IRelationship relrecord)
        {
            BirdsNestRelationship newrel = new BirdsNestRelationship();

            newrel.DbId = relrecord.Id;
            newrel.Properties = relrecord.Properties;
            newrel.Label = relrecord.Type;
            newrel.Source = relrecord.StartNodeId;
            newrel.Target = relrecord.EndNodeId;
            return newrel;
        }
    }
}
