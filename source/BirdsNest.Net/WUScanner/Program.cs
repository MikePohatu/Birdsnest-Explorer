using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.UpdateServices.Administration;
using common;
using Neo4j.Driver.V1;

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
            int relcounter = 0;
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

                //Writer.DomainID = wuconfig.ID;
                //if (string.IsNullOrEmpty(Writer.DomainID))
                //{
                //    Console.WriteLine("Your configuration does not have a scanner ID. A random ID will be generated for you below:");
                //    Console.WriteLine(ShortGuid.NewGuid().ToString());
                //    Console.WriteLine();
                //    if (batchmode == false)
                //    {
                //        Console.WriteLine("Press any key to exit");
                //        Console.ReadLine();
                //    }
                //    Environment.Exit(2);
                //}
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
                Console.WriteLine("Reading update information. This may take several minutes. Please wait...");
                UpdateCollection declinedupdates = _updateserver.GetUpdates(ApprovedStates.Declined, DateTime.MinValue, DateTime.MaxValue, null, null);
                Console.WriteLine("Found " + declinedupdates.Count + " declined updates");

                UpdateCollection notapprovedupdates = _updateserver.GetUpdates(ApprovedStates.NotApproved, DateTime.MinValue, DateTime.MaxValue, null, null);
                Console.WriteLine("Found " + notapprovedupdates.Count + " not approved updates");

                UpdateCollection approvedupdates = _updateserver.GetUpdates(ApprovedStates.LatestRevisionApproved, DateTime.MinValue, DateTime.MaxValue, null, null);
                Console.WriteLine("Found " + approvedupdates.Count + " approved updates");

                UpdateCollection staleapprovalupdates = _updateserver.GetUpdates(ApprovedStates.HasStaleUpdateApprovals, DateTime.MinValue, DateTime.MaxValue, null, null);
                Console.WriteLine("Found " + staleapprovalupdates.Count + " stale approval updates");

                foreach (IUpdate update in notapprovedupdates)
                {
                    if (update.HasSupersededUpdates)
                    {
                        UpdateCollection related = update.GetRelatedUpdates(UpdateRelationship.UpdatesSupersededByThisUpdate);
                        break;
                    }
                }
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
            Console.WriteLine("Usage: ADScanner.exe /config:<configfile> /batch");
            Console.WriteLine("/batch makes scanner run in batch mode and does not wait before exit");
        }
    }
}
