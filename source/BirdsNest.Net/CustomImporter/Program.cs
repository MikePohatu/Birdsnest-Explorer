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
using Neo4j.Driver;
using System;
using common;
using CSharpVitamins;
using System.Text;
using System.Collections.Generic;

namespace CustomImporter
{
    class Program
    {
        static void Main(string[] args)
        {
            string appdir = AppDomain.CurrentDomain.BaseDirectory;
            string neoconfigfile = appdir + @"\config\neoconfig.json";
            string configfile = appdir + @"\config\ciconfig.json";
            bool batchmode = false;
            IDriver driver = null;
            string scanid = ShortGuid.NewGuid().ToString();

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

            Configuration config = null;

            try
            {
                config = Configuration.LoadConfiguration(configfile);
            }
            catch (Exception e)
            {
                Console.WriteLine("There was an error loading config: " + configfile);
                Console.WriteLine(e.Message);
                Environment.Exit(2);
            }

            //load the neo4j config
            try
            {
                using (NeoConfiguration neoconfig = NeoConfiguration.LoadConfigurationFile(neoconfigfile))
                {
                    driver = Neo4jConnector.ConnectToNeo(neoconfig);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("There was an error loading your neo4j configuration");
                Console.WriteLine(e.Message);
                if (batchmode == false) { Console.ReadLine(); }
                Environment.Exit(2);
            }


            foreach (CustomItem item in config.Items)
            {
                object prop;
                if (item.Properties.TryGetValue(item.PrimaryProperty, out prop) == false)
                {
                    Console.WriteLine("Primary property does not have a value");
                    Environment.Exit(10);
                }
                StringBuilder builder = new StringBuilder();
                builder.AppendLine($"MERGE (n:{item.PrimaryType} {{{item.PrimaryProperty}:\"{prop}\"}})");

                if (item.Types.Count > 1)
                {
                    foreach (string type in item.Types)
                    {
                        if (type != item.PrimaryType)
                        {
                            builder.AppendLine($"SET n:{type}");
                        }
                    }
                }

                foreach (string key in item.Properties.Keys)
                {
                    object o = item.Properties[key];
                    string s = o as string;
                    if (s == null)
                    {
                        builder.AppendLine($"SET n.{key}={o}");
                    }
                    else
                    {
                        builder.AppendLine($"SET n.{key}=\"{s}\"");
                    }
                        
                }
                builder.AppendLine("RETURN n");

                string query = builder.ToString();
                NeoQueryData data = new NeoQueryData();
                data.ScanID = scanid;
                data.ScannerID = config.ScannerID;
                data.Properties = config.Items;
                NeoWriter.RunQuery(query, data, driver, true);
            }









            Console.WriteLine();
            Console.WriteLine("Finished");
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
                Console.WriteLine("Press any key to exit");
                Console.ReadLine();
            }
        }

        private static void ShowUsage()
        {
            Console.WriteLine();
            Console.WriteLine("Usage: CIScanner.exe /config:<configfile> /batch");
            Console.WriteLine("/batch makes scanner run in batch mode and does not wait before exit");
        }
    }
}
