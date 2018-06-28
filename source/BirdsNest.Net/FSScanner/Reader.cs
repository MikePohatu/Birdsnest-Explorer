using System.Collections.Generic;
using Neo4j.Driver.V1;
using Newtonsoft.Json;
using common;

namespace FSScanner
{
    public static class Reader
    {
        public static TransactionResult<List<Folder>> GetAllFoldersAsList(string rootpath, ISession session)
        {
            List<Folder> folders = new List<Folder>();
            TransactionResult<List<Folder>> result = null;

            session.ReadTransaction(tx =>
            {
                string query = "MATCH (n:" + Types.Folder + ") WHERE n.path STARTS WITH $rootpath RETURN n";
                IStatementResult dbresult = tx.Run(query, new { rootpath });
                foreach (var record in dbresult)
                {
                    var nodeProps = JsonConvert.SerializeObject(record[0].As<INode>().Properties);
                    folders.Add(JsonConvert.DeserializeObject<Folder>(nodeProps));
                }
                result = new TransactionResult<List<Folder>>(dbresult);
                result.Result = folders;
            });

            return result;
        }

        public static TransactionResult<Dictionary<string, Folder>> GetAllFoldersAsDict(string rootpath, ISession session)
        {
            Dictionary<string,Folder> folders = new Dictionary<string, Folder>();
            TransactionResult<Dictionary<string, Folder>> result = null;

            session.ReadTransaction(tx =>
            {
                string query = "MATCH (n:" + Types.Folder + ") WHERE n.path STARTS WITH $rootpath RETURN n";
                IStatementResult dbresult = tx.Run(query, new { rootpath });
                foreach (var record in dbresult)
                {
                    var nodeProps = JsonConvert.SerializeObject(record[0].As<INode>().Properties);
                    Folder f = (JsonConvert.DeserializeObject<Folder>(nodeProps));
                    folders.Add(f.Path, f);
                }
                result = new TransactionResult<Dictionary<string, Folder>>(dbresult);
                result.Result = folders;
            });

            return result;
        }
    }
}
