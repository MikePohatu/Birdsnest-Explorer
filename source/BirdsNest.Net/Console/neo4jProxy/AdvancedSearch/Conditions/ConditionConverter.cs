using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Console.neo4jProxy.AdvancedSearch.Conditions
{

    //https://skrift.io/articles/archive/bulletproof-interface-deserialization-in-jsonnet/
    public class ConditionConverter: JsonConverter
    {
        public override bool CanWrite => false;
        public override bool CanRead => true;
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(ICondition);
        }

        public override void WriteJson(JsonWriter writer,
            object value, JsonSerializer serializer)
        {
            throw new InvalidOperationException("Use default serialization.");
        }

        public override object ReadJson(JsonReader reader,
            Type objectType, object existingValue,
            JsonSerializer serializer)
        {
            JObject jsonObject = JObject.Load(reader);
            ICondition retcond = null;

            if (jsonObject["Type"] == null) { return null; }
            switch (jsonObject["Type"].Value<string>())
            {
                case "AND":
                    retcond = new AndOrCondition();
                    break;
                case "OR":
                    retcond = new AndOrCondition();
                    break;
                case "STRING":
                    retcond = new StringCondition();
                    break;
                case "MATH":
                    retcond = new MathCondition();
                    break;
                case "REGEX":
                    retcond = new RegExCondition();
                    break;
                default:
                    break;
            }

            serializer.Populate(jsonObject.CreateReader(), retcond);
            return retcond;
        }
    }
}
