using System;
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
                ResultSet results = new ResultSet();

                session.ReadTransaction(tx =>
                {
                    string query = "MATCH (n) OPTIONAL MATCH (n)-[r]-() RETURN n, r";
                    IStatementResult dbresult = tx.Run(query);

                    foreach (IRecord record in dbresult)
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
                                results.Relationships.Add(BirdsNestRelationship.GetRelationship(rel));
                                continue;
                            }
                        }
                    }
                });

                return results;
            }
        }

        public void Dispose()
        {
            this.Driver?.Dispose();
        }
    }
}
