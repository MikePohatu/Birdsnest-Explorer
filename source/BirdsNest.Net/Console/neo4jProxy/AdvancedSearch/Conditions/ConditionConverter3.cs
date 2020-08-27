#region license
// Copyright (c) 2019-2020 "20Road"
// 20Road Limited [https://www.20road.com]
//
// This file is part of Birdsnest Explorer.
//
// Birdsnest Explorer is free software: you can redistribute it and/or modify
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
using System;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Console.neo4jProxy.AdvancedSearch.Conditions
{
    public class ConditionConverter3: JsonConverter<ICondition>
    {
        public override ICondition Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartObject)
            {
                throw new JsonException();
            }

            while (reader.Read())
            {
                // Get the key.
                //if (reader.TokenType != JsonTokenType.PropertyName)
                //{
                //    throw new JsonException();
                //}

                string propertyName = reader.GetString();

                if (propertyName == "type")
                {
                    Utf8JsonReader subreader = new Utf8JsonReader(reader.ValueSequence, new JsonReaderOptions());
                    reader.Read();
                    string val = reader.GetString();

                    ICondition retcond = null;

                    if (val == null) { return null; }
                    switch (val)
                    {
                        case "AND":
                            
                            retcond = JsonSerializer.Deserialize<AndOrCondition>(ref reader, options);
                            break;
                        case "OR":
                            retcond = JsonSerializer.Deserialize<AndOrCondition>(ref reader, options);
                            break;
                        case "STRING":
                            retcond = JsonSerializer.Deserialize<StringCondition>(ref subreader, options);
                            break;
                        case "NUMBER":
                            retcond = JsonSerializer.Deserialize<NumberCondition>(ref reader, options);
                            break;
                        case "BOOLEAN":
                            retcond = JsonSerializer.Deserialize<BooleanCondition>(ref reader, options);
                            break;
                        case "REGEX":
                            retcond = JsonSerializer.Deserialize<RegExCondition>(ref reader, options);
                            break;
                        default:
                            break;
                    }

                    return retcond;
                } 
                else
                {
                    reader.Skip();
                }
                
            }

            return null;
            //throw new JsonException();
        }

        public override void Write(
            Utf8JsonWriter writer,
            ICondition condition,
            JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }
    }

}

