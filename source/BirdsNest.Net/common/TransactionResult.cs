//generic class to record results from a database transaction
using Neo4j.Driver.V1;
using System;

namespace common
{
    public class TransactionResult<T>
    {
        public int CreatedNodeCount { get; private set; } = 0;
        public int DeletedNodeCount { get; private set; } = 0;
        public int CreatedEdgeCount { get; private set; } = 0;
        public int DeletedEdgeCount { get; private set; } = 0;
        public T Result { get; set; }
        public TimeSpan ElapsedMilliseconds;

        public TransactionResult(int creatednodecount, int deletednodecount, int creatededgecount, int deletededgecount, TimeSpan elapsedms)
        {
            this.CreatedNodeCount = creatednodecount;
            this.DeletedNodeCount = deletednodecount;
            this.CreatedEdgeCount = creatededgecount;
            this.DeletedEdgeCount = deletededgecount;
            this.ElapsedMilliseconds = elapsedms;
        }

        public TransactionResult(IStatementResult dbresult)
        {
            this.CreatedNodeCount = dbresult.Summary.Counters.NodesCreated;
            this.DeletedNodeCount = dbresult.Summary.Counters.NodesDeleted;
            this.CreatedEdgeCount = dbresult.Summary.Counters.RelationshipsCreated;
            this.DeletedEdgeCount = dbresult.Summary.Counters.RelationshipsDeleted;
            this.ElapsedMilliseconds = dbresult.Summary.ResultConsumedAfter;
        }
    }
}
