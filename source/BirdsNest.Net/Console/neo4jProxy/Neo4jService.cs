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
using System.Text;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Neo4j.Driver.V1;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using Console.Plugins;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;

namespace Console.neo4jProxy
{
    public class Neo4jService : IDisposable
    {
        private readonly ILogger<Neo4jService> _logger;
        private readonly IDriver _driver;
        private readonly PluginManager _pluginmanager;

        public Neo4jService(IConfiguration configuration, ILogger<Neo4jService> logger, PluginManager manager)
        {
            this._logger = logger;
            this._pluginmanager = manager;

            //load the config
            Stopwatch stopwatch = Stopwatch.StartNew();

            using (NeoConfiguration config = NeoConfiguration.LoadIConfigurationSection(configuration.GetSection("neo4jSettings")))
            {
                this._driver = Neo4jConnector.ConnectToNeo(config);
                stopwatch.Stop();
            }

            this._logger.LogInformation("Connected to neo4j in {elapsed} ms", stopwatch.ElapsedMilliseconds);
        }


        public async Task<ResultSet> GetResultSetFromQueryAsync(string query, object props)
        {
            ResultSet returnedresults = new ResultSet();
            using (ISession session = this._driver.Session())
            {
                try
                {
                    await session.ReadTransactionAsync(async (tx) =>
                    {
                        IStatementResultCursor reader = await tx.RunAsync(query, props);
                        while (await reader.FetchAsync())
                        {
                            // Each current read in buffer can be reached via Current
                            returnedresults.Append(ParseRecord(reader.Current));
                        }
                    });
                }
                catch (Exception e)
                {
                    this._logger.LogError("Error running query from {0}: {1}", e.StackTrace, e.Message);
                    this._logger.LogError("Query {0}", query);
                }
                finally
                {
                    await session.CloseAsync();
                }
            }

            return returnedresults;
        }

        public async Task<ResultSet> GetNodeAsync(long nodeid)
        {
            string query = "MATCH (n) " +
                        "WHERE ID(n)=$id " +
                        "RETURN n";

            return await this.GetResultSetFromQueryAsync(query, new { id = nodeid });
        }

        public async Task<ResultSet> GetEdgeAsync(long edgeid)
        {
            string query = "MATCH ()-[r]->() " +
                            "WHERE ID(r)=$edgeid " +
                            "RETURN r";

            return await this.GetResultSetFromQueryAsync(query, null);
        }

        public async Task<ResultSet> GetRelationshipsAsync(List<long> nodeids)
        {
            string query = "UNWIND $ids AS nodeid " +
                            "MATCH (s)-[r]-(t) " +
                            "WHERE ID(s)=nodeid AND ID(t) IN $ids " +
                            "RETURN DISTINCT r";
            return await this.GetResultSetFromQueryAsync(query, new { ids = nodeids });
        }

        public async Task<ResultSet> GetDirectLoopsAsync(List<long> nodeids)
        {
            string query = "UNWIND $ids AS nodeid " +
                            "MATCH p = ((s) -[r1]->(t) -[r2]->(s)) " +
                            "WHERE ID(s)= nodeid AND ID(t) IN $ids " +
                            "WITH collect(r1) + collect(r2) as rels " +
                            "UNWIND rels as r " +
                            "RETURN DISTINCT r";

            return await this.GetResultSetFromQueryAsync(query, new { ids = nodeids });
        }

        public async Task<ResultSet> GetDirectRelationshipsAsync(List<long> nodeids)
        {
            string query = "UNWIND $ids AS nodeid " +
                            "MATCH (s)-[r]-() " +
                            "WHERE ID(s)=nodeid " +
                            "RETURN DISTINCT r";

            return await this.GetResultSetFromQueryAsync(query, new { ids = nodeids });
        }

        public async Task<ResultSet> GetNodesAsync(List<long> nodeids)
        {
            string query = "UNWIND $ids AS nodeid " +
                            "MATCH (s) " +
                            "WHERE ID(s)=nodeid " +
                            "RETURN DISTINCT s ORDER BY LOWER(s.name)";

            return await this.GetResultSetFromQueryAsync(query, new { ids = nodeids });
        }

        public async Task<ResultSet> GetRelatedNodesAsync(long nodeid)
        {
            string query = "MATCH (n)-[]-(m) " +
                            "WHERE ID(n)=$id " +
                            "RETURN m ORDER BY LOWER(m.name)";

            return await this.GetResultSetFromQueryAsync(query, new { id = nodeid });
        }


        public async Task<ResultSet> GetAllRelatedAsync(long nodeid)
        {
            string query = "MATCH (n)-[r]-(m) " +
                        "WHERE ID(n)=$id " +
                        "RETURN m,r ORDER BY toLower(m.name)";

            return await this.GetResultSetFromQueryAsync(query, new { id = nodeid });
        }


        //*************************
        // Search functions
        //*************************

		public async Task<ResultSet> SimpleSearch(string searchterm)
		{
            string regexterm = "(?i).*" + Regex.Escape(searchterm) + ".*";
            string query = "MATCH (n) WHERE n.name =~ $regex RETURN n";

            return await this.GetResultSetFromQueryAsync(query, new { regex = regexterm });
		}

        public async Task<ResultSet> AdvancedSearch(AdvancedSearch.Search search)
        {
            string query = search.ToTokenizedSearchString();
            ResultSet returnedresults = new ResultSet();

            //validate the types/labels 
            using (ISession session = this._driver.Session())
            {
                try
                {
                    await session.ReadTransactionAsync(async (tx) =>
                    {
                        IStatementResultCursor reader = await tx.RunAsync(query, search.Tokens.Properties);
                        while (await reader.FetchAsync())
                        {
                            returnedresults.Append(ParseRecord(reader.Current));
                        }
                    });
                }
                catch (Exception e)
                {
                    this._logger.LogError("Error running advanced search: " + e.Message);
                }
                finally
                {
                    await session.CloseAsync();
                }
            }

            return returnedresults;
        }

        public async Task<IEnumerable<string>> SearchNodePropertyValuesAsync (string type, string property, string searchterm)
        {
            if (string.IsNullOrEmpty(property) || string.IsNullOrEmpty(searchterm)) { return new List<string>(); }
            
            //replace the slashes so it picks up fs paths
            string regexterm = "(?i).*" + Regex.Escape(searchterm) + ".*";
			string typequery = string.Empty;
			if (string.IsNullOrWhiteSpace(type) == false) { typequery = "$type IN labels(n) AND "; }

           
            List<string> results = new List<string>();

            using (ISession session = this._driver.Session())
            {
                try
                {
                    string query = "MATCH (n) WHERE " + typequery + "n[{prop}]  =~ $regex RETURN DISTINCT n[{prop}] ORDER BY n[{prop}] LIMIT 20";

                    await session.ReadTransactionAsync(async (tx) =>
                    {
                        IStatementResultCursor reader = await tx.RunAsync(query, new { type, prop = property, regex = regexterm });
                        while (await reader.FetchAsync())
                        {
                            results.AddRange(ParseStringListRecord(reader.Current));
                        }

                    });
                }
                catch (Exception e)
                {
                    this._logger.LogError("Error querying SearchNodePropertyValues: " + e.Message);
                }
                finally
                {
                    await session.CloseAsync();
                }
            }

            return results;
        }
        
		public async Task<IEnumerable<string>> SearchEdgePropertyValuesAsync(string type, string property, string searchterm)
		{
			if (string.IsNullOrWhiteSpace(type)) { throw new ArgumentException("Type is required"); }
			if (string.IsNullOrWhiteSpace(property)) { throw new ArgumentException("Property is required"); }
			if (this._pluginmanager.EdgeDataTypes.Keys.Contains(type) == false) { throw new ArgumentException("Type not valid: " + type); }

			if (string.IsNullOrEmpty(type) || string.IsNullOrEmpty(property) || string.IsNullOrEmpty(searchterm)) { return new List<string>(); }

			string regexterm = "(?i).*" + searchterm + ".*";

            
            List<string> results = new List<string>();

            using (ISession session = this._driver.Session())
            {
                try
                {
                    string query = "MATCH ()-[r:" + type + "]->() WHERE r[{prop}] =~ $regex RETURN DISTINCT r[{prop}] ORDER BY r[{prop}] LIMIT 20";

                    await session.ReadTransactionAsync(async (tx) =>
                    {
                        IStatementResultCursor reader = await tx.RunAsync(query, new { prop = property, regex = regexterm });
                        while (await reader.FetchAsync())
                        {
                            results.AddRange(ParseStringListRecord(reader.Current));
                        }

                    });
                }
                catch (Exception e)
                {
                    this._logger.LogError("Error querying SearchNodePropertyValues: " + e.Message);
                }
                finally
                {
                    await session.CloseAsync();
                }
            }

            return results;
		}

        private List<string> ParseStringListResults(IStatementResult neoresult)
        {
            List<string> results = new List<string>();
            if (neoresult == null) { return results; };

            try
            {
                foreach (IRecord record in neoresult)
                {
                    foreach (string key in record.Keys)
                    {
                        results.Add(record[key].ToString());
                    }
                }
            }
            catch (Exception e)
            {
                this._logger.LogError("Error in ParseStringListResults: " + e.Message);
            }

            return results;
        }

        private List<string> ParseStringListRecord(IRecord record)
        {
            List<string> results = new List<string>();
            if (record == null) { return results; };

            try
            {
                foreach (string key in record.Keys)
                {
                    results.Add(record[key].ToString());
                }
            }
            catch (Exception e)
            {
                this._logger.LogError("Error in ParseStringListRecord: " + e.Message);
            }

            return results;
        }

        private ResultSet ParseResults(IStatementResult neoresult)
        {
            ResultSet results = new ResultSet();

            try
            {
                foreach (IRecord record in neoresult)
                {
                    foreach (string key in record.Keys)
                    {
                        AddToResultSet(record[key], results);
                    }
                }
            }
            catch(Exception e)
            {
                this._logger.LogError("Error parsing results: " + e.Message);
            }
            

            return results;
        }

        private ResultSet ParseRecord(IRecord record)
        {
            ResultSet results = new ResultSet();

            try
            {
                foreach (string key in record.Keys)
                {
                    AddToResultSet(record[key], results);
                }
            }
            catch (Exception e)
            {
                this._logger.LogError("Error parsing results: " + e.Message);
            }


            return results;
        }

        private void AddToResultSet(object o, ResultSet results)
        {
            INode node = o as INode;
            if (node != null)
            {
                results.Nodes.Add(BirdsNestNode.GetNode(node));
                return;
            }

            List<object> nodelist = o as List<object>;
            if (nodelist != null)
            {
                foreach (object obj in nodelist) { AddToResultSet(obj, results); }
                return;
            }

            IRelationship rel = o as IRelationship;
            if (rel != null)
            {
                results.Edges.Add(BirdsNestRelationship.GetRelationship(rel));
                return;
            }

            IPath path = o as IPath;
            if (path != null)
            {
                foreach (INode n in path.Nodes) { results.Nodes.Add(BirdsNestNode.GetNode(n)); }
                foreach (IRelationship r in path.Relationships) { results.Edges.Add(BirdsNestRelationship.GetRelationship(r)); }
                return;
            }
        }

        public void Dispose()
        {
            this._driver?.Dispose();
        }
    }
}
