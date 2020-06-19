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
#endregion
using System.Collections.Generic;
using System.Text;
using Neo4j.Driver.V1;
using common;
using Microsoft.UpdateServices.Administration;

namespace WUScanner.Neo4j
{
    public static class Writer
    {
        public static int MergeUpdates(IEnumerable<object> updates, IDriver driver, string scanid)
        {
            Dictionary<string, object> scanprops = new Dictionary<string, object>();
            scanprops.Add("scanid", scanid);
            scanprops.Add("updates", updates);

            string query = "UNWIND $updates as update " +
                "MERGE (n:" + Types.WUUpdate + " { id: update.ID }) " +
                "SET n.name = update.Name " +
                "SET n.IsDeclined = update.IsDeclined " +
                "SET n.IsSuperseded = update.IsSuperseded " +
                "SET n.UpdateType = update.UpdateType " +
                "SET n.Description = update.Description " +
                "SET n.IsDeclined = update.IsDeclined " +
                "SET n.AdditionalInformation = update.AdditionalInformationUrls " +
                "SET n.CreationDate = update.CreationDate " +
                "SET n.KB = update.KB " +
                "SET n.Classification = update.Classification " +
                "SET n.Products = update.Products " +
                "SET n.layout = 'tree' " +
                "SET n.lastscan = $scanid " +
                "RETURN n ";

            using (ISession session = driver.Session())
            {
                IStatementResult result = session.WriteTransaction(tx => tx.Run(query, scanprops));
                return result.Summary.Counters.NodesCreated;
            }
        }

        public static int MergeSupersedence(IEnumerable<object> mappings, IDriver driver, string scanid)
        {
            Dictionary<string, object> scanprops = new Dictionary<string, object>();
            scanprops.Add("scanid", scanid);
            scanprops.Add("mappings", mappings);

            //KeyValuePair<string, string> test;

            string query = "UNWIND $mappings as mapping " +
                "MATCH (new:" + Types.WUUpdate + " { id: mapping.relatedid }) " +
                "MATCH (old:" + Types.WUUpdate + " { id: mapping.updateid }) " +
                "WITH new, old, mapping " +
                "MERGE p=(new)-[r:" + Types.Supersedes + "]->(old) " +
                "SET r.lastscan = $scanid " +
                "RETURN p ";

            using (ISession session = driver.Session())
            {
                IStatementResult result = session.WriteTransaction(tx => tx.Run(query, scanprops));
                return result.Summary.Counters.RelationshipsCreated;
            }
        }

        public static void UpdateMetadata(IDriver driver)
        {
            List<string> types = new List<string>() { Types.WUUpdate };

            foreach (string type in types)
            {
                string query =
                "MATCH (n:" + type + ") " +
                "WITH DISTINCT keys(n) as props " +
                "UNWIND props as p " +
                "WITH DISTINCT p as disprops " +
                "WITH collect(disprops) as allprops " +
                "MERGE(i: _Metadata { name: 'NodeProperties'}) " +
                "SET i." + type + " = allprops " +
                "RETURN i";
                using (ISession session = driver.Session())
                {
                    session.WriteTransaction(tx => tx.Run(query));
                }
            }
        }
    }
}
