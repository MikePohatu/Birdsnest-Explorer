using System;
using System.Text;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using common;
using Neo4j.Driver.V1;
using Newtonsoft.Json;

namespace NeoProxy
{
    public class Neo4jService : IDisposable
    {
        public readonly IDriver Driver;

        public Neo4jService()
        {
            string configfile = AppDomain.CurrentDomain.BaseDirectory + @"\config.json";

            //load the config
            using (NeoConfiguration config = NeoConfiguration.ReadConfigurationFromFile(configfile))
            {
                this.Driver = Neo4jConnector.ConnectToNeo(config);
            }
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

        private ResultSet ParseResults(IStatementResult neoresult)
        {
            ResultSet results = new ResultSet();

            foreach (IRecord record in neoresult)
            {
                foreach (string key in record.Keys)
                {
                    INode node = record[key] as INode;
                    if (node != null)
                    {
                        results.Nodes.Add(BirdsNestNode.GetNode(node));
                        continue;
                    }

                    IRelationship rel = record[key] as IRelationship;
                    if (rel != null)
                    {
                        results.Edges.Add(BirdsNestRelationship.GetRelationship(rel));
                        continue;
                    }
                }
            }

            return results;
        }




        //*************************
        // Search functions
        //*************************

        public ResultSet SearchPath(string sourcetype, string sourceprop, string sourceval, string relationship, string tartype, string tarprop, string tarval)
        {
            using (ISession session = this.Driver.Session())
            {
                ResultSet returnedresults = new ResultSet();
                try
                {
                    var props = new
                    {
                        sourcetype = sourcetype == null ? string.Empty : sourcetype,
                        sourceprop = sourceprop == null ? string.Empty : sourceprop,
                        sourceval = sourceval == null ? string.Empty : sourceval,
                        relationship = relationship == null ? string.Empty : relationship,
                        tartype = tartype == null ? string.Empty : tartype,
                        tarprop = tarprop == null ? string.Empty : tarprop,
                        tarval = tarval == null ? string.Empty : tarval
                    };

                    session.ReadTransaction(tx =>
                    {
                        StringBuilder builder = new StringBuilder();
                        bool predacate = false;

                        builder.Append("MATCH path=(s)-[r]->(t) WHERE ");
                        if (!string.IsNullOrEmpty(sourcetype))
                        {
                            builder.Append("$sourcetype IN labels(s) ");
                            predacate = true;
                        }

                        if (!string.IsNullOrEmpty(sourceval)) {
                            if (predacate) { builder.Append("AND "); }
                            builder.Append("s[{sourceprop}] = $sourceval ");
                            predacate = true;
                        }
                        if (!string.IsNullOrEmpty(relationship)) {
                            if (predacate) { builder.Append("AND "); }
                            builder.Append("TYPE(r) = $relationship ");
                            predacate = true;
                        }

                        if (!string.IsNullOrEmpty(tartype))
                        {
                            if (predacate) { builder.Append("AND "); }
                            builder.Append("$tartype IN labels(t) ");
                        }

                        if (!string.IsNullOrEmpty(tarval)) {
                            if (predacate) { builder.Append("AND "); }
                            builder.Append("t[{tarprop}] = $tarval ");
                        }
                        builder.Append("RETURN s,t");

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

        public IEnumerable<string> GetNodeLabels()
        {
            IStatementResult dbresult = null;
            using (ISession session = this.Driver.Session())
            {
                try
                {
                    session.ReadTransaction(tx =>
                    {
                        string query = "CALL db.labels()";
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

        public IEnumerable<string> GetEdgeLabels()
        {
            IStatementResult dbresult = null;
            using (ISession session = this.Driver.Session())
            {
                try
                {
                    session.ReadTransaction(tx =>
                    {
                        string query = "CALL db.relationshipTypes()";
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
                        string query = "MATCH (n) WHERE $type IN labels(n) AND n[{prop}]  =~ $regex RETURN n[{prop}] ORDER BY n[{prop}]";
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

        public void Dispose()
        {
            this.Driver?.Dispose();
        }
    }
}
