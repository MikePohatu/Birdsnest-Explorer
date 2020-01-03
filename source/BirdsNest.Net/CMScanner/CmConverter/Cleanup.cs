#region license
// Copyright (c) 2019-2020 "20Road"
// 20Road Limited [https://20road.com]
//
// This file is part of BirdsNest.
//
// BirdsNest is free software: you can redistribute it and/or modify
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
//
#endregion
using common;
using Neo4j.Driver.V1;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMScanner.CmConverter
{
    public static class Cleanup
    {
        public static int CleanupCmObjects(IDriver driver, string scanid, string scannerid)
        {
            int count= 0;
            IResultSummary summary;
            NeoQueryData collectionsdata = new NeoQueryData();
            collectionsdata.ScanID = scanid;
            collectionsdata.ScannerID = scannerid;

            //nodes first
            string query = "MATCH (n:" + Types.CMConfigurationItem + ") " +
                "WHERE n.lastscan<>$ScanID " +
                "DETACH DELETE n " +
                "RETURN n";

            summary = NeoWriter.RunQuery(query, collectionsdata, driver.Session());
            count = count + summary.Counters.NodesDeleted;

            //any remaining edges
            query = "MATCH ()-[r:" + Types.CMHasObject + "]->() " +
                "WHERE r.lastscan<>$ScanID " +
                "DELETE r " +
                "RETURN r";

            summary = NeoWriter.RunQuery(query, collectionsdata, driver.Session());
            count = count + summary.Counters.RelationshipsDeleted;

            query = "MATCH ()-[r:" + Types.CMLimitingCollection + "]->() " +
                "WHERE r.lastscan<>$ScanID " +
                "DELETE r " +
                "RETURN r";

            summary = NeoWriter.RunQuery(query, collectionsdata, driver.Session());
            count = count + summary.Counters.RelationshipsDeleted;

            return count;
        }
    }
}
