using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using common;
using Neo4j.Driver.V1;

namespace NeoProxy
{
    public class NeoService: IDisposable
    {
        public readonly IDriver Driver;

        public NeoService()
        {
            string configfile = AppDomain.CurrentDomain.BaseDirectory + @"\config.json";

            //load the config
            using (NeoConfiguration config = NeoConfiguration.ReadConfigurationFromFile(configfile))
            {
                this.Driver = Neo4jConnector.ConnectToNeo(config);
            }
        }

        public string GetAll()
        {
            string result = "test";
            using (ISession session = this.Driver.Session())
            {
                session.ReadTransaction(tx =>
                {
                    string query = "MATCH (n) RETURN n";
                    IStatementResult dbresult = tx.Run(query);

                    result = dbresult.Single()[0].As<string>();
                    //foreach (var record in dbresult)
                    //{
                    //    var nodeProps = JsonConvert.SerializeObject(record[0].As<INode>().Properties);
                    //    Folder f = (JsonConvert.DeserializeObject<Folder>(nodeProps));
                    //    folders.Add(f.Path, f);
                    //}
                    //result = new TransactionResult<Dictionary<string, Folder>>(dbresult);
                    //result.Result = folders;
                });

                return result;
            }
        }

        public void Dispose()
        {
            this.Driver?.Dispose();
        }
    }
}
