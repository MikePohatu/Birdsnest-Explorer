using System;
using System.Text;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using common;
using Neo4j.Driver.V1;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using Console.Plugins;

namespace Console.neo4jProxy
{
    public class Neo4jService : IDisposable
    {
        private readonly ILogger<Neo4jService> _logger;
        private readonly IDriver _driver;
        private readonly PluginManager _pluginmanager;

        public Neo4jService(ILogger<Neo4jService> logger, NeoConfiguration config, PluginManager manager)
        {
            this._logger = logger;
            this._pluginmanager = manager;
            Config neo4jconfig = new Config();
            neo4jconfig.ConnectionIdleTimeout = Config.InfiniteInterval;
            neo4jconfig.MaxConnectionLifetime = Config.InfiniteInterval;
            neo4jconfig.SocketKeepAlive = true;

            //load the config
            Stopwatch stopwatch = Stopwatch.StartNew();
            this._driver = Neo4jConnector.ConnectToNeo(config, neo4jconfig);
            stopwatch.Stop();
            this._logger.LogInformation("Connected to neo4j in {elapsed} ms", stopwatch.ElapsedMilliseconds);
        }


        public ResultSet GetResultSetFromQuery(string query)
        {
            using (ISession session = this._driver.Session())
            {
                ResultSet returnedresults = new ResultSet();
                try
                {
                    session.ReadTransaction(tx =>
                    {
                        IStatementResult dbresult = tx.Run(query);
                        returnedresults.Append(ParseResults(dbresult));
                    });
                }
                catch
                {
                    //logging to add
                }

                return returnedresults;
            }
        }

        public ResultSet GetAll()
        {
            using (ISession session = this._driver.Session())
            {
                ResultSet returnedresults = new ResultSet();
                try
                {
                    session.ReadTransaction(tx =>
                    {
                        string query = "MATCH (n) RETURN n";
                        IStatementResult dbresult = tx.Run(query);
                        returnedresults.Append(ParseResults(dbresult));
                    });

                    session.ReadTransaction(tx =>
                    {
                        var resultids = from node in returnedresults.Nodes select node.DbId;
                        string query = "UNWIND $ids AS nodeid " +
                            "MATCH (n)-[r]->() " +
                            "WHERE ID(n)=nodeid " +
                            "RETURN r";
                        IStatementResult dbresult = tx.Run(query, new { ids = resultids });
                        returnedresults.Append(ParseResults(dbresult));
                    });
                }
                catch
                {
                    //logging to add
                }

                return returnedresults;
            }
        }

        public ResultSet GetNode(long nodeid)
        {
            using (ISession session = this._driver.Session())
            {
                ResultSet returnedresults = new ResultSet();
                try
                {
                    session.ReadTransaction(tx =>
                    {
                        string query = "MATCH (n) " +
                            "WHERE ID(n)=$nodeid " +
                            "RETURN n";
                        IStatementResult dbresult = tx.Run(query, new { nodeid = nodeid });
                        returnedresults.Append(ParseResults(dbresult));
                    });
                }
                catch
                {
                    //logging to add
                }

                return returnedresults;
            }
        }

        public async Task<object> GetRelationshipsAsync(List<long> nodeids)
        {
            object o = null;
            await Task.Run(() => o = GetRelationships(nodeids));

            return o;
        }

        public ResultSet GetRelationships(List<long> nodeids)
        {
            using (ISession session = this._driver.Session())
            {
                ResultSet returnedresults = new ResultSet();
                try
                {
                    session.ReadTransaction(tx =>
                    {
                        string query = "UNWIND $ids AS nodeid " +
                            "MATCH (s)-[r]-(t) " +
                            "WHERE ID(s)=nodeid AND ID(t) IN $ids " +
                            "RETURN DISTINCT r";
                        IStatementResult dbresult = tx.Run(query, new { ids = nodeids });
                        returnedresults.Append(ParseResults(dbresult));
                    });
                }
                catch
                {
                    //logging to add
                }

                return returnedresults;
            }
        }

        public async Task<object> GetDirectRelationshipsAsync(List<long> nodeids)
        {
            object o = null;
            await Task.Run(() => o = GetDirectRelationships(nodeids));

            return o;
        }

        public ResultSet GetDirectRelationships(List<long> nodeids)
        {
            using (ISession session = this._driver.Session())
            {
                ResultSet returnedresults = new ResultSet();
                try
                {
                    session.ReadTransaction(tx =>
                    {
                        string query = "UNWIND $ids AS nodeid " +
                            "MATCH (s)-[r]-() " +
                            "WHERE ID(s)=nodeid " +
                            "RETURN DISTINCT r";
                        IStatementResult dbresult = tx.Run(query, new { ids = nodeids });
                        returnedresults.Append(ParseResults(dbresult));
                    });
                }
                catch
                {
                    //logging to add
                }

                return returnedresults;
            }
        }

        public async Task<object> GetNodesAsync(List<long> nodeids)
        {
            object o = null;
            await Task.Run(() => o = GetNodes(nodeids));

            return o;
        }

        public ResultSet GetNodes(List<long> nodeids)
        {
            using (ISession session = this._driver.Session())
            {
                ResultSet returnedresults = new ResultSet();
                try
                {
                    session.ReadTransaction(tx =>
                    {
                        string query = "UNWIND $ids AS nodeid " +
                            "MATCH (s) " +
                            "WHERE ID(s)=nodeid " +
                            "RETURN DISTINCT s ORDER BY LOWER(s.name)";
                        IStatementResult dbresult = tx.Run(query, new { ids = nodeids });
                        returnedresults.Append(ParseResults(dbresult));
                    });
                }
                catch
                {
                    //logging to add
                }

                return returnedresults;
            }
        }

        public ResultSet GetRelatedNodes(long nodeid)
        {
            using (ISession session = this._driver.Session())
            {
                ResultSet returnedresults = new ResultSet();
                try
                {
                    session.ReadTransaction(tx =>
                    {
                        string query = "MATCH (n)-[]-(m) " +
                            "WHERE ID(n)=$id " +
                            "RETURN m ORDER BY LOWER(m.name)";
                        IStatementResult dbresult = tx.Run(query, new { id = nodeid });
                        returnedresults.Append(ParseResults(dbresult));
                    });
                }
                catch
                {
                    //logging to add
                }

                return returnedresults;
            }
        }


        public ResultSet GetAllRelated(long nodeid)
        {
            using (ISession session = this._driver.Session())
            {
                ResultSet returnedresults = new ResultSet();
                try
                {
                    session.ReadTransaction(tx =>
                    {
                        string query = "MATCH (n)-[r]-(m) " +
                            "WHERE ID(n)=$id " +
                            "RETURN m,r ORDER BY LOWER(m.name)";
                        IStatementResult dbresult = tx.Run(query, new { id = nodeid });
                        returnedresults.Append(ParseResults(dbresult));
                    });
                }
                catch
                {
                    //logging to add
                }

                return returnedresults;
            }
        }


        //*************************
        // Search functions
        //*************************

		public ResultSet SimpleSearch(string searchterm)
		{
			using (ISession session = this._driver.Session())
			{
				ResultSet returnedresults = new ResultSet();
				try
				{
					string regexterm = "(?i).*" + Regex.Escape(searchterm) + ".*";
					string query = "MATCH (n) WHERE n.name =~ $regex RETURN n";
					session.ReadTransaction(tx =>
					{
						IStatementResult dbresult = tx.Run(query, new { regex = regexterm });
						returnedresults.Append(ParseResults(dbresult));
					});
				}
				catch
				{
					//logging to add
				}

				return returnedresults;
			}
		}

        public ResultSet AdvancedSearch(AdvancedSearch.Search search)
        {
            //validate the types/labels 
            List<string> edgetypes = GetEdgeLabels();
            List<string> nodetypes = GetNodeLabels();

            using (ISession session = this._driver.Session())
            {
                ResultSet returnedresults = new ResultSet();
                try
                {
                    string query = search.ToTokenizedSearchString();

                    session.ReadTransaction(tx =>
                    {
                        IStatementResult dbresult = tx.Run(query, search.Tokens.Properties);
                        returnedresults.Append(this.ParseResults(dbresult));
                    });
                }
                catch
                {
                    //logging to add
                }

                return returnedresults;
            }
        }

        public List<string> GetNodeLabels()
        {
            IStatementResult dbresult = null;
            using (ISession session = this._driver.Session())
            {
                try
                {
                    session.ReadTransaction(tx =>
                    {
                        string query = "CALL db.labels() YIELD label RETURN label ORDER BY label";
                        dbresult = tx.Run(query);
                    });
                }
                catch
                {
                    //logging to add
                }
            }

            return ParseStringListResults(dbresult);
        }

        public List<string> GetEdgeLabels()
        {
            IStatementResult dbresult = null;
            using (ISession session = this._driver.Session())
            {
                try
                {
                    session.ReadTransaction(tx =>
                    {
                        string query = "CALL db.relationshipTypes() YIELD relationshipType RETURN relationshipType ORDER BY relationshipType";
                        dbresult = tx.Run(query);
                    });
                }
                catch
                {
                    //logging to add
                }
            }

            return ParseStringListResults(dbresult);
        }


        /// <summary>
        /// Test function. not currently in use
        /// </summary>
        /// <returns></returns>
        public int GetAllNodesCount()
        {
            IStatementResult dbresult = null;
            using (ISession session = this._driver.Session())
            {
                try
                {
                    session.ReadTransaction(tx =>
                    {
                        string query = "MATCH (n) RETURN count(n)";
                        dbresult = tx.Run(query);
                    });
                    return dbresult.Single()[0].As<int>();
                }
                catch
                {
                    //logging to add
                }
            }

            return 0;
        }


        public IEnumerable<string> SearchNodeNames(string term, int searchlimit)
        {
            string regexterm = "(?i)" + Regex.Escape(term) + ".*";
            
            IStatementResult dbresult = null;
            using (ISession session = this._driver.Session())
            {
                try
                {
                    session.ReadTransaction(tx =>
                    {
                        string query = "MATCH (n) WHERE n.name =~ $regex RETURN n.name LIMIT $limit";
                        dbresult = tx.Run(query, new { regex = regexterm, limit = searchlimit });
                    });
                }
                catch
                {
                    //logging to add
                }
            }

            return ParseStringListResults(dbresult);
        }


        //example build query
        //MATCH(n:AD_Group)
        //WITH DISTINCT keys(n) as props
        //MERGE(i:_Meta { name: 'NodeDetails'})
        //SET i.AD_Group = props
        //RETURN i
        public SortedDictionary<string, List<string>> GetNodeProperties()
        {
            SortedDictionary<string, List<string>> result = new SortedDictionary<string, List<string>>();

            IStatementResult dbresult = null;
            using (ISession session = this._driver.Session())
            {
                try
                {
                    session.ReadTransaction(tx =>
                    {
                        string query = "MATCH (i:_Metadata {name:'NodeProperties'}) RETURN i";
                        dbresult = tx.Run(query);
                    });
                }
                catch
                {
                    //logging to add
                }
            }

            ResultSet r = ParseResults(dbresult);

            foreach (BirdsNestNode node in r.Nodes)
            {
                foreach (string key in node.Properties.Keys)
                {
                    if (key != "name")
                    {
                        object val;
                        if (node.Properties.TryGetValue(key, out val))
                        {
                            List<string> lst = new List<string>();
                            foreach (object o in (List<object>)val)
                            {
                                lst.Add((string)o);
                            }
                            lst.Sort();
                            result.Add(key, lst);
                        }
                    }
                }
            }

            return result;
        }

        public IEnumerable<string> SearchNodePropertyValues (string type, string property, string searchterm)
        {
            if (string.IsNullOrEmpty(property) || string.IsNullOrEmpty(searchterm)) { return new List<string>(); }
            
            //replace the slashes so it picks up fs paths
            string regexterm = "(?i).*" + Regex.Escape(searchterm) + ".*";
			string typequery = string.Empty;
			if (string.IsNullOrWhiteSpace(type) == false) { typequery = "$type IN labels(n) AND "; }

			IStatementResult dbresult = null;
            using (ISession session = this._driver.Session())
            {
                try
                {
                    session.ReadTransaction(tx =>
                    {
                        string query = "MATCH (n) WHERE "+ typequery + "n[{prop}]  =~ $regex RETURN DISTINCT n[{prop}] ORDER BY n[{prop}] LIMIT 20";
                        dbresult = tx.Run(query, new { type = type, prop = property, regex= regexterm });
                    });
                }
                catch
                {
                    //logging to add
                }
            }

            return ParseStringListResults(dbresult);
        }

        public SortedDictionary<string, List<string>> GetEdgeProperties()
        {
            SortedDictionary<string, List<string>> result = new SortedDictionary<string, List<string>>();

            IStatementResult dbresult = null;
            using (ISession session = this._driver.Session())
            {
                try
                {
                    session.ReadTransaction(tx =>
                    {
                        string query = "MATCH (i:_Metadata {name:'EdgeProperties'}) RETURN i";
                        dbresult = tx.Run(query);
                    });
                }
                catch
                {
                    //logging to add
                }
            }

            ResultSet r = ParseResults(dbresult);

            foreach (BirdsNestNode node in r.Nodes)
            {
                foreach (string key in node.Properties.Keys)
                {
                    if (key != "name")
                    {
                        object val;
                        if (node.Properties.TryGetValue(key, out val))
                        {
                            List<string> lst = new List<string>();
                            foreach (object o in (List<object>)val)
                            {
                                lst.Add((string)o);
                            }
                            lst.Sort();
                            result.Add(key, lst);
                        }
                    }
                }
            }

            return result;
        }

		public IEnumerable<string> SearchEdgePropertyValues(string type, string property, string searchterm)
		{
			IStatementResult dbresult = null;
			if (string.IsNullOrWhiteSpace(type)) { throw new ArgumentException("Type is required"); }
			if (string.IsNullOrWhiteSpace(property)) { throw new ArgumentException("Property is required"); }
			if (this._pluginmanager.EdgeLabels.Contains(type) == false) { throw new ArgumentException("Type not valid: " + type); }

			if (string.IsNullOrEmpty(type) || string.IsNullOrEmpty(property) || string.IsNullOrEmpty(searchterm)) { return new List<string>(); }

			string regexterm = "(?i).*" + searchterm + ".*";

			using (ISession session = this._driver.Session())
			{
				try
				{
					session.ReadTransaction(tx =>
					{
						string query = "MATCH ()-[r:"+type+"]->() WHERE r[{prop}] =~ $regex RETURN DISTINCT r[{prop}] ORDER BY r[{prop}] LIMIT 20";
						dbresult = tx.Run(query, new { prop = property, regex = regexterm });
					});
				}
				catch(Exception e)
				{
					this._logger.LogError("Unable to run SearchEdgePropertyValues query: " + e.Message);
				}
			}

			return ParseStringListResults(dbresult);
		}

        public async Task<bool> UpdateMetadataAsync(string label)
        {
            bool b = false;
            await Task.Run(() => b = UpdateMetadata(label));

            return b;
        }

        public bool UpdateMetadata(string label)
        {
            if (this._pluginmanager.NodeLabels.Contains(label))
            {
                string query =
                "MATCH (n:" + label + ") " +
                "WITH DISTINCT keys(n) as props " +
                "UNWIND props as p " +
                "WITH DISTINCT p as disprops " +
                "WITH collect(disprops) as allprops " +
                "MERGE(i: _Metadata { name: 'NodeProperties'}) " +
                "SET i." + label + " = allprops " +
                "RETURN i";
                using (ISession session = this._driver.Session())
                {
                    session.WriteTransaction(tx => tx.Run(query));
                }
            }
            else if (this._pluginmanager.EdgeLabels.Contains(label))
            {
                string query = "MATCH ()-[r:" + label + "]->()" +
                "WITH DISTINCT keys(r) as props " +
                "UNWIND props as p " +
                "WITH DISTINCT p as disprops " +
                "WITH collect(disprops) as allprops " +
                "MERGE(i: _Metadata { name: 'EdgeProperties'}) " +
                "SET i." + label + " = allprops " +
                "RETURN i";
                using (ISession session = this._driver.Session())
                {
                    session.WriteTransaction(tx => tx.Run(query));
                }
            }
            else
            {
                throw new ArgumentException(label + " is not a valid plugin label");
            }
        
            return true;
        }

        private List<string> ParseStringListResults(IStatementResult dbresult)
        {
            List<string> results = new List<string>();
            try
            {
                foreach (IRecord record in dbresult)
                {
                    foreach (string key in record.Keys)
                    {
                        results.Add(record[key].ToString());
                    }
                }
            }
            catch (Exception e)
            {
                this._logger.LogError("Error parsing list results: " + e.Message);
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
