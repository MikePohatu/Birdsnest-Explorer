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
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Neo4j.Driver.V1;

namespace Console.neo4jProxy
{
    public class StatsGetter
    {
        private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();
        private readonly Neo4jService _neoservice;

        public StatsGetter(Neo4jService service)
        {
            this._neoservice = service;
        }

        public async Task<object> GatherDbStats()
        {
            
            SortedDictionary<string, long> nodeLabelCounts = new SortedDictionary<string, long>();
            SortedDictionary<string, long> edgeLabelCounts = new SortedDictionary<string, long>();
            SortedDictionary<string, long> totals = new SortedDictionary<string, long>();

            //get the node label stats
            string nodesquery = "MATCH (n) RETURN DISTINCT count(labels(n)) as labelCount, labels(n) as labels";
            await this._neoservice.ProcessDelegatePerRecordFromQueryAsync(nodesquery, null, (IRecord record)=> {
                if (record == null) { return; };

                try
                {
                    List<object> labels = record["labels"] as List<object>;
                    long count = (long)record["labelCount"];
                    if (labels != null)
                    {
                        foreach (object label in labels)
                        {
                            string labelstring = label.ToString();
                            long current = 0;
                            if (nodeLabelCounts.TryGetValue(labelstring, out current)) { nodeLabelCounts[labelstring] = count + current; }
                            else { nodeLabelCounts.Add(labelstring, count); }
                        }
                    }
                }
                catch (Exception e)
                {
                    _logger.Error("Error in GatherDbStats-nodeLabelCounts: " + e.Message);
                }
            });

            

            //get the relationship label stats
            string edgesquery = "MATCH ()-[r]->() RETURN count(TYPE(r)) as labelCount, TYPE(r) as label";

            await this._neoservice.ProcessDelegatePerRecordFromQueryAsync(edgesquery, null, (IRecord record) => {
                if (record == null) { return; };

                try
                {
                    string label = record["label"] as string;
                    long count = (long)record["labelCount"];
                    long current = 0;
                    if (label != null)
                    {
                        if (edgeLabelCounts.TryGetValue(label, out current)) { edgeLabelCounts[label] = count; }
                        else { edgeLabelCounts.Add(label, count); }
                    }
                }
                catch (Exception e)
                {
                    _logger.Error("Error in GatherDbStats-edgeLabelCounts: " + e.Message);
                }
            });

            //get the total node count
            string nodetotal = "MATCH (n) RETURN count(n) as nodeCount";
            await this._neoservice.ProcessDelegatePerRecordFromQueryAsync(nodetotal, null, (IRecord record) => {
                if (record == null) { return; };

                try
                {
                    long count = (long)record["nodeCount"];
                    totals.Add("nodes", count);
                }
                catch (Exception e)
                {
                    _logger.Error("Error in GatherDbStats-nodeCount: " + e.Message);
                }
            });

            //get the total edge count
            string edgetotalquery = "MATCH ()-[r]-() RETURN count(r) as edgeCount";
            await this._neoservice.ProcessDelegatePerRecordFromQueryAsync(edgetotalquery, null, (IRecord record) => {
                if (record == null) { return; };

                try
                {
                    long count = (long)record["edgeCount"];
                    totals.Add("edges", count);
                }
                catch (Exception e)
                {
                    _logger.Error("Error in GatherDbStats-edgeCount: " + e.Message);
                }
            });

            //get the server details
            string serverdeetsquery = "call dbms.components() yield name, versions, edition unwind versions as version return name, version, edition";
            string name = string.Empty;
            string version = string.Empty;
            string edition = string.Empty;

            await this._neoservice.ProcessDelegatePerRecordFromQueryAsync(serverdeetsquery, null, (IRecord record) => {
                if (record == null) { return; };

                try
                {
                    name = (string)record["name"];
                    version = (string)record["version"];
                    edition = (string)record["edition"];
                }
                catch (Exception e)
                {
                    _logger.Error("Error in GatherDbStats-edgeCount: " + e.Message);
                }
            });

            object results = new { name, version, edition, nodeLabelCounts, edgeLabelCounts, totals };
            return results;
        }
    }
}
