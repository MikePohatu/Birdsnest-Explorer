﻿#region license
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
using System.Diagnostics;
using common;
using Microsoft.Graph;
using Microsoft.Identity.Client;
using Neo4j.Driver;
using AzureADScanner.Azure;
using CSharpVitamins;

namespace AzureADScanner
{
    class Program
    {
        static void Main(string[] args)
        {
            Stopwatch steptimer = new Stopwatch();
            Stopwatch totaltimer = new Stopwatch();
            string _appdir = AppDomain.CurrentDomain.BaseDirectory;
            string neoconfigfile = _appdir + @"\config\neoconfig.json";
            string configfile = _appdir + @"\config\aadconfig.json";
            bool batchmode = false;
            string scanid = ShortGuid.NewGuid().ToString();
            string scannerid = string.Empty;

            IDriver driver = null;

            totaltimer.Start();
            try
            {
                foreach (string arg in args)
                {
                    string[] param = arg.Split(new[] { ":" }, 2, StringSplitOptions.None);
                    switch (param[0].ToUpper())
                    {
                        case "/?":
                            ShowUsage();
                            Environment.Exit(0);
                            break;
                        case "/CONFIG":
                            configfile = param[1];
                            break;
                        case "/BATCH":
                            batchmode = true;
                            break;
                        default:
                            break;
                    }
                }
            }
            catch
            {
                Console.WriteLine("There is a problem with arguments: " + string.Join(" ", args));
                Console.WriteLine("");
                ShowUsage();
                Environment.Exit(1);
            }

            //load the config
            try
            {
                using (Configuration config = Configuration.LoadConfiguration(configfile))
                {
                    Console.WriteLine("Loading config for scanner: " + scannerid);

                    scannerid = config.ScannerID;
                    NeoWriter.ScannerID = scannerid;
                    Connector.Instance.Init(config);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("There was an error connecting to the server with your configuration");
                Console.WriteLine(e.Message);
                if (batchmode == false) { Console.ReadLine(); }
                Environment.Exit(1);
            }

            

            try
            {
                using (NeoConfiguration config = NeoConfiguration.LoadConfigurationFile(neoconfigfile))
                {
                    driver = Neo4jConnector.ConnectToNeo(config);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("There was an error loading your neo4j configuration");
                Console.WriteLine(e.Message);
                if (batchmode == false) { Console.ReadLine(); }
                Environment.Exit(2);
            }

            NeoWriter.ScanID = scanid;

            var aadgroupmembers = new AadGroupMemberships();
            aadgroupmembers.GroupIDs = AadGroups.Instance.GroupIDs;

            List <IDataCollectorAsync> collectors = new List<IDataCollectorAsync>
            {
                
                AadUsers.Instance,
                new AadUserToAdUserConnections(),
                AadGroups.Instance,
                aadgroupmembers,
                AadSites.Instance,
                new AadDrives()
            };

            NeoWriter.WriteHeaders();

            foreach (IDataCollectorAsync collector in collectors)
            {
                try
                {
                    NeoWriter.WriteIDataCollector(collector, driver, true, true);
                }
                catch (Exception e)
                {
                    Console.WriteLine("There was an error collecting data for " + collector.GetType());
                    Console.WriteLine(e.Message);
                }
            }







            //cleanup

            if (batchmode == true)
            {
                Console.Write("Exiting.");
                for (int i = 0; i < 3; i++)
                {
                    System.Threading.Thread.Sleep(500);
                    Console.Write(".");
                }
            }
            else
            {
                Console.WriteLine();
                Console.WriteLine("Press any key to exit");
                Console.ReadLine();
            }
        }

        private static void ShowUsage()
        {
            Console.WriteLine();
            Console.WriteLine("Usage: AzureADScanner.exe /config:<configfile> /batch");
            Console.WriteLine("/batch makes scanner run in batch mode and does not wait before exit");
        }

        
    }
}
