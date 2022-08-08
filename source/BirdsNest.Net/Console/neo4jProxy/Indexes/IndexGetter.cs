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
using Neo4j.Driver;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Console.neo4jProxy.Indexes
{
    public static class IndexGetter
    {
        public static async Task<object> GatherDbIndexes35Async(Neo4jService service)
        {
            //get the index details from db
            string query = "CALL db.indexes() yield indexName, tokenNames, properties, state, type";
            string labelName = string.Empty;
            string indexName = string.Empty;
            string propertyName = string.Empty;
            string state = string.Empty;
            string type = string.Empty;
            SortedDictionary<string, SortedDictionary<string, Index>> indexes = new SortedDictionary<string, SortedDictionary<string, Index>>();

            await service.ProcessDelegatePerRecordFromQueryAsync(query, null, (IRecord record) =>
            {
                if (record == null) { return; };

                try
                {
                    var tokens = (List<object>)record["labelsOrTypes"];
                    labelName = (string)tokens[0];
                    var properties = (List<object>)record["properties"];
                    propertyName = (string)properties[0];
                    indexName = (string)record["name"];
                    state = (string)record["state"];
                    type = (string)record["uniqueness"];
                    bool isconstraint = type == "UNIQUE" ? true : false;

                    SortedDictionary<string, Index> indexdic;
                    if (!indexes.TryGetValue(labelName, out indexdic))
                    {
                        indexdic = new SortedDictionary<string, Index>();
                        indexes.Add(labelName, indexdic);
                    }

                    indexdic.Add(propertyName, new Index { Label = labelName, IndexName = indexName, PropertyName = propertyName, State = state, IsConstraint = isconstraint });
                }
                catch (Exception e)
                {
                    //_logger.Error("Error in GatherDbStats-edgeCount: " + e.Message);
                }
            });

            return indexes;
        }

        public static async Task<object> GatherDbIndexes4Async(Neo4jService service)
        {
            //get the index details from db
            string query = "CALL db.indexes() yield name, labelsOrTypes, properties, state, type, uniqueness";
            string labelName = string.Empty;
            string indexName = string.Empty;
            string propertyName = string.Empty;
            string state = string.Empty;
            string type = string.Empty;
            SortedDictionary<string, SortedDictionary<string, Index>> indexes = new SortedDictionary<string, SortedDictionary<string, Index>>();

            await service.ProcessDelegatePerRecordFromQueryAsync(query, null, (IRecord record) =>
            {
                if (record == null) { return; };

                try
                {
                    var tokens = (List<object>)record["labelsOrTypes"];
                    labelName = (string)tokens[0];
                    var properties = (List<object>)record["properties"];
                    propertyName = (string)properties[0];
                    indexName = (string)record["name"];
                    state = (string)record["state"];
                    type = (string)record["uniqueness"];
                    bool isconstraint = type == "UNIQUE" ? true : false;

                    SortedDictionary<string, Index> indexdic;
                    if (!indexes.TryGetValue(labelName, out indexdic))
                    {
                        indexdic = new SortedDictionary<string, Index>();
                        indexes.Add(labelName, indexdic);
                    }

                    indexdic.Add(propertyName, new Index { Label = labelName, IndexName = indexName, PropertyName = propertyName, State = state, IsConstraint = isconstraint });
                }
                catch (Exception e)
                {
                    //_logger.Error("Error in GatherDbStats-edgeCount: " + e.Message);
                }
            });

            return indexes;
        }
    }
}
