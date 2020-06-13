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
#endregion
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

            if (jsonObject["type"] == null) { return null; }
            switch (jsonObject["type"].Value<string>())
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
                case "NUMBER":
                    retcond = new NumberCondition();
                    break;
                case "BOOLEAN":
                    retcond = new BooleanCondition();
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
