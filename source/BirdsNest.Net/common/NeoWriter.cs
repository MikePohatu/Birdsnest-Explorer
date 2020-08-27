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
using Neo4j.Driver.V1;

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

        public static List<IResultSummary> RunQuery(string query, NeoQueryData data, IDriver driver)
        {
            return RunQuery(query, data, driver, false);
        }
        public static List<IResultSummary> RunQuery(string query, NeoQueryData data, IDriver driver, bool showprogressdots)
        {
            
            return RunQuery(query, data, driver, false, showprogressdots);
        }

        public static List<IResultSummary> RunQuery(string query, NeoQueryData data, IDriver driver, bool showstats, bool showprogressdots)
        {
            List<IResultSummary> summaries = new List<IResultSummary>();
            ISession session;
            int itemscount = 0;

            if (string.IsNullOrWhiteSpace(data.ScanID))
            {
                data.ScanID = NeoWriter.ScanID;
            }

            if (data.Properties != null)
            {
                while (data.Properties.Count > 1000)
                {

                    NeoQueryData subdata = new NeoQueryData();
                    subdata.ScanID = data.ScanID;
                    subdata.ScannerID = data.ScannerID;
                    subdata.Properties = ListExtensions.ListPop<object>(data.Properties, 1000);

                    itemscount += subdata.Properties.Count;

                    IStatementResult subresult;
                    using (session = driver.Session())
                    {
                        subresult = session.WriteTransaction(tx => tx.Run(query, subdata));
                    }
                    if (subresult?.Summary != null) { summaries.Add(subresult.Summary); }
                    if (showprogressdots) { Console.Write("."); }
                }
            }

            IStatementResult result;
            using (session = driver.Session())
            {
                if (data.Properties != null) { itemscount += data.Properties.Count; }
                result = session.WriteTransaction(tx => tx.Run(query, data));
            }
            if (result?.Summary != null) { summaries.Add(result.Summary); }

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
        public static List<IResultSummary> WriteIDataCollector(IDataCollector collector, IDriver driver, bool showprogressdots)
        {
            NeoQueryData collectionsdata = collector.CollectData();
            collectionsdata.ScanID = NeoWriter.ScanID;
            collectionsdata.ScannerID = NeoWriter.ScannerID;
            List<IResultSummary> summaries = NeoWriter.RunQuery(collector.Query, collectionsdata, driver, showprogressdots);
            return summaries;
        }

        public static List<IResultSummary> WriteIDataCollector(IDataCollector collector, IDriver driver)
        {
            return WriteIDataCollector(collector, driver, false);
        }

        public static List<IResultSummary> WriteIDataCollector(IDataCollector collector, IDriver driver, bool showstats, bool showprogressdots)
        {
            ConsoleWriter.Write(collector.ProgressMessage);
            NeoQueryData collectionsdata = collector.CollectData();
            collectionsdata.ScanID = NeoWriter.ScanID;
            collectionsdata.ScannerID = NeoWriter.ScannerID;
            List<IResultSummary> summaries = NeoWriter.RunQuery(collector.Query, collectionsdata, driver, showstats, showprogressdots);
            return summaries;
        }
    }
}
