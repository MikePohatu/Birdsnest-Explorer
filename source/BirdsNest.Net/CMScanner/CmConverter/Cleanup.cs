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
        public static void CleanupCmObjects(IDriver driver, string scanid, string scannerid, int[] tabstops)
        {
            IResultSummary summary;
            NeoQueryData collectionsdata = new NeoQueryData();
            collectionsdata.ScanID = scanid;
            collectionsdata.ScannerID = scannerid;

            //nodes first
            List<string> cmnodetypes = new List<string> {
                Types.CMConfigurationItem,
                Types.CMDevice,
                Types.CMUser,
                Types.CMClientSettings
            };

            string query;
            foreach (string type in cmnodetypes)
            {
                query = "MATCH (n:" + type + ") " +
                "WHERE n.scannerid=$ScannerID AND n.lastscan<>$ScanID " +
                "DETACH DELETE n " +
                "RETURN n";

                summary = NeoWriter.RunQuery(query, collectionsdata, driver.Session());
                string[] sumvals = {
                    "Cleaning up " + type,
                    summary.Counters.NodesCreated.ToString(),
                    summary.Counters.RelationshipsCreated.ToString(),
                    summary.Counters.NodesDeleted.ToString(),
                    summary.Counters.RelationshipsDeleted.ToString(),
                    summary.Counters.PropertiesSet.ToString()
                    };

                ConsoleWriter.WriteLine(tabstops, sumvals);
            }

            //any remaining edges
            List<string> cmreltypes = new List<string> { 
                Types.CMLimitingCollection,
                Types.CMMemberOf,
                Types.CMReferences,
                Types.CMHasProgram,
                Types.CMHasObject,
                Types.CMHasDeployment
            };



            foreach (string type in cmreltypes)
            {
                query = "MATCH ()-[r:" + type + "]->() " +
                "WHERE r.scannerid=$ScannerID AND r.lastscan<>$ScanID " +
                "DELETE r " +
                "RETURN r";

                summary = NeoWriter.RunQuery(query, collectionsdata, driver.Session());
                string[] sumvals = {
                    "Cleaning up " + type,
                    summary.Counters.NodesCreated.ToString(),
                    summary.Counters.RelationshipsCreated.ToString(),
                    summary.Counters.NodesDeleted.ToString(),
                    summary.Counters.RelationshipsDeleted.ToString(),
                    summary.Counters.PropertiesSet.ToString()
                    };

                ConsoleWriter.WriteLine(tabstops, sumvals);
            }
        }
    }
}
