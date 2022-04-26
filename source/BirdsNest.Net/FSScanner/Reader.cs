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
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FSScanner
{
    public static class Reader
    {
        //public static async Task<TransactionResult<List<Folder>>> GetAllFoldersAsList(string rootpath, IDriver driver)
        //{
        //    List<Folder> folders = new List<Folder>();
        //    TransactionResult<List<Folder>> result = null;

        //    IAsyncSession session = driver.AsyncSession()

        //    try
        //    {
        //        session.ReadTransactionAsync(async tx =>
        //        {
        //            string query = "MATCH (n:" + Types.Folder + ") WHERE n.path STARTS WITH $rootpath RETURN n";
        //            IResultCursor dbresult = await tx.RunAsync(query, new { rootpath });
        //            while (await dbresult.FetchAsync())
        //            {
        //                await dbresult.ForEachAsync((record) =>
        //                {
        //                    var nodeProps = JsonConvert.SerializeObject(record[0].As<INode>().Properties);
        //                    folders.Add(JsonConvert.DeserializeObject<Folder>(nodeProps));
        //                });
        //                result = new TransactionResult<List<Folder>>(dbresult);
        //                result.Result = folders;
        //            }
        //        });
        //    }
        //    finally
        //    {
        //        await session.CloseAsync();
        //    }

        //    return result;
        //}

        public async static Task<TransactionResult<Dictionary<string, Folder>>> GetAllFoldersAsDict(string rootpath, IDriver driver)
        {
            Dictionary<string, Folder> folders = new Dictionary<string, Folder>();
            TransactionResult<Dictionary<string, Folder>> result = null;

            IAsyncSession session = driver.AsyncSession();
            try
            {
                await session.ReadTransactionAsync(async tx =>
                {
                    string query = "MATCH (n:" + Types.Folder + ") WHERE n.path STARTS WITH $rootpath RETURN n";
                    IResultCursor dbresult = await tx.RunAsync(query, new { rootpath });
                    await dbresult.ForEachAsync(record =>
                    {
                        var nodeProps = JsonConvert.SerializeObject(record[0].As<INode>().Properties);
                        Folder f = (JsonConvert.DeserializeObject<Folder>(nodeProps));
                        folders.Add(f.Path, f);
                    });
                    //foreach (var record in dbresult.ForEachAsync)
                    //{
                    //    var nodeProps = JsonConvert.SerializeObject(record[0].As<INode>().Properties);
                    //    Folder f = (JsonConvert.DeserializeObject<Folder>(nodeProps));
                    //    folders.Add(f.Path, f);
                    //}
                    var summary = await dbresult.ConsumeAsync();
                    result = new TransactionResult<Dictionary<string, Folder>>(summary);
                    result.Result = folders;
                });
            }
            finally
            {
                await session.CloseAsync();
            }

            return result;
        }
    }
}
