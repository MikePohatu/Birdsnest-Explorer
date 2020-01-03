using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Neo4j.Driver.V1;

namespace common
{
    public static class NeoWriter
    {
        public static string ScanID { get; set; }

        public static IResultSummary RunQuery(string query, NeoQueryData data, ISession session)
        {
            if (string.IsNullOrWhiteSpace(data.ScanID))
            {
                data.ScanID = NeoWriter.ScanID;
            }

            var result = session.WriteTransaction(tx => tx.Run(query, data));
            return result.Summary;
        }
    }
}
