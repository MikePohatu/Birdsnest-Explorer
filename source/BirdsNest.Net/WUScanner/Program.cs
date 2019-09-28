﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.UpdateServices.Administration;
using common;
using Neo4j.Driver.V1;
using WUScanner.Neo4j;

namespace WUScanner
{
    class Program
    {
        static void Main(string[] args)
        {
            Stopwatch steptimer = new Stopwatch();
            Stopwatch totaltimer = new Stopwatch();

            Configuration wuconfig = null;

            string _appdir = AppDomain.CurrentDomain.BaseDirectory;
            string neoconfigfile = _appdir + @"\neoconfig.json";
            string configfile = _appdir + @"\wuconfig.json";
            //int relcounter = 0;
            bool batchmode = false;
            string scanid = ShortGuid.NewGuid().ToString();
            IUpdateServer _updateserver = null;

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
                wuconfig = Configuration.LoadConfiguration(configfile);

                if (string.IsNullOrWhiteSpace(wuconfig.ServerName) || wuconfig.Port <= 0 )
                {
                    throw new ArgumentException("Configuration is invalid");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("There was an error loading your configuration");
                Console.WriteLine(e.Message);
                if (batchmode == false) { Console.ReadLine(); }
                Environment.Exit(1);
            }



            //load the neo4j config
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

            try
            {
                if (!string.IsNullOrWhiteSpace(wuconfig.UserName))
                {
 
                }
                else
                {
                    Console.WriteLine("Connecting to server " + wuconfig.ServerName);
                    _updateserver = AdminProxy.GetUpdateServer(wuconfig.ServerName, wuconfig.UseSSL, wuconfig.Port);
                }

                if (_updateserver == null) { throw new ArgumentException("Unable to connect to update server " + wuconfig.ServerName); }
            }
            catch (Exception e)
            {
                Console.WriteLine("There was an error communicating with the WSUS server: ");
                Console.WriteLine(e.Message);
                Console.WriteLine(e.TargetSite.ToString());
                if (batchmode == false) { Console.ReadLine(); }
                Environment.Exit(1);
            }


            try
            {
                //The main bit 
                UpdateResults updateinfo = new UpdateResults();
                WUQueryHandler.PopulateUpdateResults(_updateserver, updateinfo);
                Console.WriteLine("Found " + updateinfo.Updates.Count + " updates");
                Console.WriteLine("Found " + updateinfo.SupersededUpdates.Count + " superseded updates");

                IEnumerable<object> sublist;

                Console.Write("Writing updates to database.");
                while (updateinfo.Updates.Count > 1000)
                {
                    Console.Write(".");
                    sublist = ListExtensions.ListPop(updateinfo.Updates, 1000);
                    Writer.MergeUpdates(sublist, driver, scanid);
                }
                if (updateinfo.Updates.Count > 0) 
                {
                    sublist = updateinfo.Updates;
                    Writer.MergeUpdates(sublist, driver, scanid); 
                }
                Console.WriteLine();
                

                Console.Write("Writing supersedence to database.");
                while (updateinfo.SupersededUpdates.Count > 1000)
                {
                    Console.Write(".");
                    sublist = ListExtensions.ListPop(updateinfo.SupersededUpdates, 1000);
                    Writer.MergeSupersedence(sublist, driver, scanid);
                }
                if (updateinfo.SupersededUpdates.Count > 0) { Writer.MergeSupersedence(updateinfo.SupersededUpdates, driver, scanid); }
                Console.WriteLine();

                Writer.UpdateMetadata(driver);
            }
            catch (Exception e)
            {
                Console.WriteLine("There was an error processing updates");
                Console.WriteLine(e.Message);
            }

            if (batchmode == false) {
                Console.WriteLine("Finished. Press Enter to continue");
                Console.ReadLine(); }
        }

        private static void ShowUsage()
        {
            Console.WriteLine();
            Console.WriteLine("Usage: WUScanner.exe /config:<configfile> /batch");
            Console.WriteLine("/batch makes scanner run in batch mode and does not wait before exit");
        }
    }
}
