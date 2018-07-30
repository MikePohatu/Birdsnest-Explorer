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

        public ResultSet SearchPath(string sourcetype, string sourceprop, string sourceval, string reltype, int relmin, int relmax, string tartype, string tarprop, string tarval)
        {
            //validate the types/labels 
            List<string> edgetypes = GetEdgeLabels();
            List<string> nodetypes = GetNodeLabels();

            string rellabel = string.Empty;
            string sourcelabel = string.Empty;
            string targetlabel = string.Empty;

            
            if (string.IsNullOrWhiteSpace(reltype))
            { rellabel = string.Empty; }
            else if (edgetypes.Exists(x => string.Equals(x, reltype)))
            { rellabel = ":" + reltype;  }
            else
            { throw new ArgumentException("Invalid relationship type: " + reltype); }

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
                            builder.Append("MATCH path=(s" + sourcelabel+ ")-[" + reltype + "*" + relmin + ".." + relmax + "]->(t" + targetlabel + ") ");
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

                            builder.Append("RETURN DISTINCT t");
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

        public List<string> GetEdgeLabels()
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
