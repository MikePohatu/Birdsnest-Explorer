using System;
using System.Text;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using common;
using Neo4j.Driver.V1;
using Newtonsoft.Json;

namespace Console.neo4jProxy
{
    public class Neo4jService : IDisposable
    {
        public readonly IDriver Driver;

        public Neo4jService(NeoConfiguration config)
        {
            Config neo4jconfig = new Config();
            neo4jconfig.ConnectionIdleTimeout = Config.InfiniteInterval;
            neo4jconfig.MaxConnectionLifetime = Config.InfiniteInterval;
            neo4jconfig.SocketKeepAlive = true;

            //load the config
            this.Driver = Neo4jConnector.ConnectToNeo(config, neo4jconfig);
        }

        public object GetAll()
        {
            using (ISession session = this.Driver.Session())
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

        public object GetNode(long nodeid)
        {
            using (ISession session = this.Driver.Session())
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

        public object GetRelationships(List<long> nodeids)
        {
            using (ISession session = this.Driver.Session())
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

        public object GetDirectRelationships(List<long> nodeids)
        {
            using (ISession session = this.Driver.Session())
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

        public object GetRelatedNodes(long nodeid)
        {
            using (ISession session = this.Driver.Session())
            {
                ResultSet returnedresults = new ResultSet();
                try
                {
                    session.ReadTransaction(tx =>
                    {
                        string query = "MATCH (n)-[]-(m) " +
                            "WHERE ID(n)=$id " +
                            "RETURN m";
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

        public ResultSet SearchPath(string sourcetype, string sourceprop, string sourceval, string reltype, int relmin, int relmax, char dir, string tartype, string tarprop, string tarval)
        {
            //validate the types/labels 
            List<string> edgetypes = GetEdgeLabels();
            List<string> nodetypes = GetNodeLabels();

            string rellabel = string.Empty;
            string sourcelabel = string.Empty;
            string targetlabel = string.Empty;
            string relright = string.Empty;
            string relleft = string.Empty;

            if (string.IsNullOrWhiteSpace(reltype))
            { rellabel = string.Empty; }
            else if (edgetypes.Exists(x => string.Equals(x, reltype)))
            { rellabel = ":" + reltype;  }
            else
            { throw new ArgumentException("Invalid relationship type: " + reltype); }

            if ((dir == 'B') || (dir == 'b')) { /*do nothing*/ }
            else if ((dir == 'L') || (dir == 'l')) { relleft = "<"; }
            else if ((dir == 'R') || (dir == 'r')) { relright = ">"; }
            else
            { throw new ArgumentException("Invalid relationship direction: " + dir); }

            if (string.IsNullOrWhiteSpace(sourcetype))
            { sourcelabel = string.Empty; }
            else if (nodetypes.Exists(x => string.Equals(x, sourcetype)))
            { sourcelabel = ":" + sourcetype; }
            else
            { throw new ArgumentException("Invalid source type: " + sourcetype); }

            if (string.IsNullOrWhiteSpace(tartype))
            { targetlabel = string.Empty; }
            else if (nodetypes.Exists(x => string.Equals(x, tartype)))
            { targetlabel = ":" + tartype; }
            else
            { throw new ArgumentException("Invalid target type: " + tartype); }



            using (ISession session = this.Driver.Session())
            {
                ResultSet returnedresults = new ResultSet();
                try
                {
                    var props = new
                    {
                        sourceprop = sourceprop == null ? string.Empty : sourceprop,
                        sourceval = sourceval == null ? string.Empty : sourceval,
                        tarprop = tarprop == null ? string.Empty : tarprop,
                        tarval = tarval == null ? string.Empty : tarval
                    };

                    session.ReadTransaction(tx =>
                    {
                        StringBuilder builder = new StringBuilder();
                        bool whereset = false;

                        if (relmax > 0)
                        {
                            builder.Append("MATCH path=(s" + sourcelabel+ ")"+ relleft+"-[" + rellabel + "*" + relmin + ".." + relmax + "]-"+relright+"(t" + targetlabel + ") ");
                        }
                        else
                        {
                            builder.Append("MATCH (s" + sourcelabel + ") ");
                        }

                        if (!string.IsNullOrEmpty(sourceval)) {
                            builder.Append("WHERE s[{sourceprop}] = $sourceval ");
                            whereset = true;
                        }

                        if (relmax > 0)
                        { 
                            if (!string.IsNullOrEmpty(tarval))
                            {
                                if (whereset) { builder.Append("AND "); }
                                else { builder.Append("WHERE "); }
                                builder.Append("t[{tarprop}] = $tarval ");
                            }

                            builder.Append("UNWIND nodes(path) as n RETURN DISTINCT n");
                        }
                        else
                        {
                            builder.Append("RETURN s");
                        }
                        
                        

                        string query = builder.ToString();
                        Debug.WriteLine("Search query: " + query);


                        IStatementResult dbresult = tx.Run(query,props);
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

        public List<string> GetNodeLabels()
        {
            IStatementResult dbresult = null;
            using (ISession session = this.Driver.Session())
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
            using (ISession session = this.Driver.Session())
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
            using (ISession session = this.Driver.Session())
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
            string regexterm = "(?i)" + term + ".*";
            
            IStatementResult dbresult = null;
            using (ISession session = this.Driver.Session())
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

        public IEnumerable<string> GetNodeProperties(string type)
        {
            IStatementResult dbresult = null;
            using (ISession session = this.Driver.Session())
            {
                try
                {
                    session.ReadTransaction(tx =>
                    {
                        if (string.IsNullOrEmpty(type))
                        {
                            string query = "MATCH (n) UNWIND keys(n) as props RETURN DISTINCT props ORDER BY props";
                            dbresult = tx.Run(query);
                        }
                        else
                        {
                            string query = "MATCH (n) WHERE $type IN labels(n) UNWIND keys(n) as props RETURN DISTINCT props ORDER BY props";
                            dbresult = tx.Run(query, new { type = type });
                        }
                    });
                }
                catch
                {
                    //logging to add
                }
            }

            return ParseStringListResults(dbresult);
        }


        public IEnumerable<string> SearchNodePropertyValues (string type, string property, string searchterm)
        {
            if (string.IsNullOrEmpty(type) || string.IsNullOrEmpty(property) || string.IsNullOrEmpty(searchterm)) { return new List<string>(); }
            string regexterm = "(?i).*" + searchterm + ".*";

            IStatementResult dbresult = null;
            using (ISession session = this.Driver.Session())
            {
                try
                {
                    session.ReadTransaction(tx =>
                    {
                        string query = "MATCH (n) WHERE $type IN labels(n) AND n[{prop}]  =~ $regex RETURN DISTINCT n[{prop}] ORDER BY n[{prop}] LIMIT 10";
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

        private List<string> ParseStringListResults(IStatementResult dbresult)
        {
            List<string> results = new List<string>();
            foreach (IRecord record in dbresult)
            {
                foreach (string key in record.Keys)
                {
                    results.Add(record[key].ToString());
                }
            }
            return results;
        }

        private ResultSet ParseResults(IStatementResult neoresult)
        {
            ResultSet results = new ResultSet();

            foreach (IRecord record in neoresult)
            {
                foreach (string key in record.Keys)
                {
                    AddToResultSet(record[key], results);
                }
            }

            return results;
        }

        //private ResultSet ParseResults(IStatementResultCursor neoresult)
        //{
        //    ResultSet results = new ResultSet();

        //    foreach (IRecord record in neoresult.ToListAsync())
        //    {
        //        foreach (string key in record.Keys)
        //        {
        //            AddToResultSet(record[key], results);
        //        }
        //    }

        //    return results;
        //}


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
            this.Driver?.Dispose();
        }
    }
}
