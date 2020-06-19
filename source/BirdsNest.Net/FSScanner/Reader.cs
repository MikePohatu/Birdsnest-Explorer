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
using Neo4j.Driver.V1;
using Newtonsoft.Json;
using common;

namespace FSScanner
{
    public static class Reader
    {
        public static TransactionResult<List<Folder>> GetAllFoldersAsList(string rootpath, IDriver driver)
        {
            List<Folder> folders = new List<Folder>();
            TransactionResult<List<Folder>> result = null;

            using (ISession session = driver.Session())
            {
                session.ReadTransaction(tx =>
                {
                    string query = "MATCH (n:" + Types.Folder + ") WHERE n.path STARTS WITH $rootpath RETURN n";
                    IStatementResult dbresult = tx.Run(query, new { rootpath });
                    foreach (var record in dbresult)
                    {
                        var nodeProps = JsonConvert.SerializeObject(record[0].As<INode>().Properties);
                        folders.Add(JsonConvert.DeserializeObject<Folder>(nodeProps));
                    }
                    result = new TransactionResult<List<Folder>>(dbresult);
                    result.Result = folders;
                });
            }

            return result;
        }

        public static TransactionResult<Dictionary<string, Folder>> GetAllFoldersAsDict(string rootpath, IDriver driver)
        {
            Dictionary<string,Folder> folders = new Dictionary<string, Folder>();
            TransactionResult<Dictionary<string, Folder>> result = null;

            using (ISession session = driver.Session())
            {
                session.ReadTransaction(tx =>
                {
                    string query = "MATCH (n:" + Types.Folder + ") WHERE n.path STARTS WITH $rootpath RETURN n";
                    IStatementResult dbresult = tx.Run(query, new { rootpath });
                    foreach (var record in dbresult)
                    {
                        var nodeProps = JsonConvert.SerializeObject(record[0].As<INode>().Properties);
                        Folder f = (JsonConvert.DeserializeObject<Folder>(nodeProps));
                        folders.Add(f.Path, f);
                    }
                    result = new TransactionResult<Dictionary<string, Folder>>(dbresult);
                    result.Result = folders;
                });
            }

            return result;
        }
    }
}
