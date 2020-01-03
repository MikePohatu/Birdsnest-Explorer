using common;
using Neo4j.Driver.V1;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMScanner.CmConverter
{
    interface ICmCollector
    {
        string Query { get; }
        string ProgressMessage { get; }
        NeoQueryData CollectData();
        string GetSummaryString(IResultSummary summary);
    }
}
