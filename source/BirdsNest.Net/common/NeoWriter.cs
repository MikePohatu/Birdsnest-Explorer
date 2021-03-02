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
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Neo4j.Driver;

namespace common
{
    public static class NeoWriter
    {
        public static int[] Tabs { get; } = { 0, 3, 60, 67, 74, 81, 88 };
        public static string ScanID { get; set; }
        public static string ScannerID { get; set; }


        public static void WriteHeaders()
        {
            string[] headervals = { "Description", string.Empty, "(n)+", "[r]+", "(n)-", "[r]-", "Properties set" };
            ConsoleWriter.WriteLine(Tabs, headervals);
        }


        // ****************************************
        // Synchronous calls of Async methods
        // ****************************************

        public static List<IResultSummary> RunQuery(string query, NeoQueryData data, IDriver driver)
        {
            return RunQueryAsync(query, data, driver).Result;
        }
        public static List<IResultSummary> RunQuery(string query, NeoQueryData data, IDriver driver, bool showprogressdots)
        {

            return RunQueryAsync(query, data, driver, showprogressdots).Result;
        }

        public static List<IResultSummary> RunQuery(string query, NeoQueryData data, IDriver driver, bool showstats, bool showprogressdots)
        {
            return RunQueryAsync(query, data, driver, showstats, showprogressdots).Result;
        }

        public static List<IResultSummary> WriteIDataCollector(IDataCollector collector, IDriver driver, bool showprogressdots)
        {
            return WriteIDataCollectorAsync(collector, driver, showprogressdots).Result;
        }

        public static List<IResultSummary> WriteIDataCollector(IDataCollector collector, IDriver driver)
        {
            return WriteIDataCollectorAsync(collector, driver).Result;
        }

        public static List<IResultSummary> WriteIDataCollector(IDataCollector collector, IDriver driver, bool showstats, bool showprogressdots)
        {
            return WriteIDataCollectorAsync(collector, driver, showstats, showprogressdots).Result;
        }

        // ****************************************
        // Synchronous calls of Async methods
        // ****************************************
        public async static Task<List<IResultSummary>> RunQueryAsync(string query, NeoQueryData data, IDriver driver)
        {
            return await RunQueryAsync(query, data, driver, false);
        }
        public async static Task<List<IResultSummary>> RunQueryAsync(string query, NeoQueryData data, IDriver driver, bool showprogressdots)
        {
            return await RunQueryAsync(query, data, driver, false, showprogressdots);
        }

        public async static Task<List<IResultSummary>> RunQueryAsync(string query, NeoQueryData data, IDriver driver, bool showstats, bool showprogressdots)
        {
            List<IResultSummary> summaries = new List<IResultSummary>();
            IAsyncSession session = driver.AsyncSession();
            int itemscount = 0;

            if (string.IsNullOrWhiteSpace(data.ScanID))
            {
                data.ScanID = NeoWriter.ScanID;
            }

            if (string.IsNullOrWhiteSpace(data.ScannerID))
            {
                data.ScannerID = NeoWriter.ScannerID;
            }

            //main query
            try
            {
                if (data.Properties != null)
                {
                    while (data.Properties.Count > 0)
                    {

                        NeoQueryData subdata = new NeoQueryData();
                        subdata.ScanID = data.ScanID;
                        subdata.ScannerID = data.ScannerID;
                        subdata.Properties = ListExtensions.ListPop<object>(data.Properties, 1000);

                        itemscount += subdata.Properties.Count;

                        await session.WriteTransactionAsync(async (tx) =>
                        {
                            IResultCursor reader = await tx.RunAsync(query, subdata);
                            var summary = await reader.ConsumeAsync();
                            if (summary != null) { summaries.Add(summary); }
                            if (showprogressdots) { Console.Write("."); }
                        });
                    }
                } else
                {
                    NeoQueryData subdata = new NeoQueryData();
                    subdata.ScanID = data.ScanID;
                    subdata.ScannerID = data.ScannerID;

                    await session.WriteTransactionAsync(async (tx) =>
                    {
                        IResultCursor reader = await tx.RunAsync(query, subdata);
                        var summary = await reader.ConsumeAsync();
                        if (summary != null) { summaries.Add(summary); }
                        if (showprogressdots) { Console.Write("."); }
                    });
                }
            }
            finally
            {
                await session.CloseAsync();
            }

            

            Console.WriteLine();

            if (showstats)
            {
                int nodescreated = 0;
                int nodesdeleted = 0;
                int relscreated = 0;
                int relsdeleted = 0;
                int propsset = 0;

                foreach (IResultSummary summary in summaries)
                {
                    nodescreated += summary.Counters.NodesCreated;
                    nodesdeleted += summary.Counters.NodesDeleted;
                    relscreated += summary.Counters.RelationshipsCreated;
                    relsdeleted += summary.Counters.RelationshipsDeleted;
                    propsset += summary.Counters.PropertiesSet;
                }

                string[] sumvals = {
                string.Empty,
                $"Done, processed {itemscount.ToString()} items",
                nodescreated.ToString(),
                relscreated.ToString(),
                nodesdeleted.ToString(),
                relsdeleted.ToString(),
                propsset.ToString()
                };


                ConsoleWriter.WriteLine(Tabs, sumvals);
            }

            return summaries;
        }

        /// <summary>
        /// Write the data collector to the database with no output messaging other than progress dots
        /// </summary>
        /// <param name="collector"></param>
        /// <param name="driver"></param>
        /// <returns></returns>
        public async static Task<List<IResultSummary>> WriteIDataCollectorAsync(IDataCollector collector, IDriver driver, bool showprogressdots)
        {
            NeoQueryData collectionsdata = collector.CollectData();
            List<IResultSummary> summaries = await NeoWriter.RunQueryAsync(collector.Query, collectionsdata, driver, showprogressdots);
            return summaries;
        }

        public static async Task<List<IResultSummary>> WriteIDataCollectorAsync(IDataCollector collector, IDriver driver)
        {
            return await WriteIDataCollectorAsync(collector, driver, false);
        }

        public static async Task<List<IResultSummary>> WriteIDataCollectorAsync(IDataCollector collector, IDriver driver, bool showstats, bool showprogressdots)
        {
            ConsoleWriter.Write(collector.ProgressMessage);
            NeoQueryData collectionsdata = collector.CollectData();
            List<IResultSummary> summaries = await RunQueryAsync(collector.Query, collectionsdata, driver, showstats, showprogressdots);
            return summaries;
        }
    }
}
