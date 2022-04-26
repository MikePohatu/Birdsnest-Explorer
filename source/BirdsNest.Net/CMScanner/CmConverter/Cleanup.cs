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
using Neo4j.Driver;
using System;
using System.Collections.Generic;

namespace CMScanner.CmConverter
{
    public static class Cleanup
    {
        public static void CleanupCmObjects(IDriver driver)
        {
            NeoQueryData collectionsdata = new NeoQueryData();
            collectionsdata.ScanID = NeoWriter.ScanID;
            collectionsdata.ScannerID = NeoWriter.ScannerID;

            //nodes first
            List<string> cmnodetypes = new List<string> {
                Types.CMApplication,
                Types.CMPackage,
                Types.CMPackageProgram,
                Types.CMSoftwareUpdateGroup,
                Types.CMTaskSequence,
                Types.CMCollection,
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

                Console.Write("Cleaning up " + type);
                NeoWriter.RunQuery(query, collectionsdata, driver, true, true);
            }

            //any remaining edges
            List<string> cmreltypes = new List<string> {
                Types.CMLimitingCollectionFor,
                Types.CmExcludes,
                Types.CmIncludes,
                Types.CMUnknownCollectionRelationship,
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

                Console.Write("Cleaning up " + type);
                NeoWriter.RunQuery(query, collectionsdata, driver, true, true);
            }


            //We don't delete software updates. MS might expire something which might remove it from SCCM.
            //We want to keep that info
            query = "MATCH (n:" + Types.CMSoftwareUpdate + ") " +
                "WHERE n.scannerid=$ScannerID AND n.lastscan<>$ScanID " +
                "SET n.IsExpired = true " +
                "RETURN n";

            Console.Write("Finding expired updates");
            NeoWriter.RunQuery(query, collectionsdata, driver, true, true);
        }
    }
}
