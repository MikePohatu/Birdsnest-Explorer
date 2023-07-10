#region license
// Copyright (c) 2019-2023 "20Road"
// 20Road Limited [https://www.20road.com]
//
// This file is part of Birdsnest Explorer.
//
// Birdsnest Explorer is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, version 3 of the License.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.
#endregion

//generic class to record results from a database transaction
using Neo4j.Driver;
using System;
using System.Collections.Generic;

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

        public TransactionResult(IResultSummary summary)
        {
            this.CreatedNodeCount = summary.Counters.NodesCreated;
            this.DeletedNodeCount = summary.Counters.NodesDeleted;
            this.CreatedEdgeCount = summary.Counters.RelationshipsCreated;
            this.DeletedEdgeCount = summary.Counters.RelationshipsDeleted;
            this.ElapsedMilliseconds = summary.ResultConsumedAfter;
        }

        public TransactionResult(List<IResultSummary> summaries)
        {
            foreach (IResultSummary summary in summaries)
            {
                this.CreatedNodeCount += summary.Counters.NodesCreated;
                this.DeletedNodeCount += summary.Counters.NodesDeleted;
                this.CreatedEdgeCount += summary.Counters.RelationshipsCreated;
                this.DeletedEdgeCount += summary.Counters.RelationshipsDeleted;
                this.ElapsedMilliseconds += summary.ResultConsumedAfter;
            }
        }
    }
}
