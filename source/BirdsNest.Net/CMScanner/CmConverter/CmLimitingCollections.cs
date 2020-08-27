#region license
// Copyright (c) 2019-2020 "20Road"
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
using common;
using Neo4j.Driver.V1;

namespace CMScanner.CmConverter
{
    public class CmLimitingCollections: IDataCollector
    {
        public string ProgressMessage { get { return "Connecting limiting collections"; } }
        public string Query
        {
            get
            {
                return "MATCH (n:" + Types.CMCollection + ")" +
                "MATCH (l:" + Types.CMCollection + " {id:n.limitingcollection}) " +
                "MERGE (l)-[r:" + Types.CMLimitingCollection + "]->(n) " +
                "SET r.lastscan=$ScanID " +
                "SET r.scannerid=$ScannerID " +
                "RETURN n.name";
            }
        }

        public NeoQueryData CollectData()
        {
            return new NeoQueryData();
        }

        public string GetSummaryString(IResultSummary summary)
        {
            return summary.Counters.RelationshipsCreated + " connected";
        }

        public static CmLimitingCollections GetInstance() { return new CmLimitingCollections(); }
    }
}
