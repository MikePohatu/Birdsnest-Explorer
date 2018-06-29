using System.Collections.Generic;
using Neo4j.Driver.V1;
using System;
using System.Text;
using common;

namespace CustomAppScanner
{
    public class Writer
    {
        public int UpdateApplication(Application app, ISession session)
        {
            string query = "MERGE (app:" + Types.Application +" {name:$Name}) "+
                "SET app.lastscan=$ScanId " +
                "RETURN app";

            IStatementResult result = session.WriteTransaction(tx => tx.Run(query, app));
            return result.Summary.Counters.NodesDeleted;
        }
    }
}
