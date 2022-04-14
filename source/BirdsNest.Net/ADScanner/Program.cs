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
using System.Diagnostics;
using Neo4j.Driver;
using ADScanner.ActiveDirectory;
using common;
using CSharpVitamins;
using System.DirectoryServices.AccountManagement;

namespace ADScanner
{
    class Program
    {
        private static bool _batchmode = false;
        static void Main(string[] args)
        {
            Stopwatch totaltimer = new Stopwatch();

            string _appdir = AppDomain.CurrentDomain.BaseDirectory;
            string neoconfigfile = _appdir + @"\config\neoconfig.json";
            string configfile = _appdir + @"\config\adconfig.json";
            NeoWriter.ScanID = ShortGuid.NewGuid().ToString();

            IDriver driver = null;
            PrincipalContext context = null;

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
                            _batchmode = true;
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
                ExitError(1);
            }


            //load the config
            try
            {
                using (Configuration config = Configuration.LoadConfiguration(configfile))
                {
                    if (string.IsNullOrEmpty(config.ID))
                    {
                        Console.WriteLine("Your configuration does not have a scanner ID. A random ID will be generated for you below:");
                        Console.WriteLine(ShortGuid.NewGuid().ToString());
                        Console.WriteLine();
                        ExitError(2);
                    }
                    NeoWriter.ScannerID = config.ID;
                    context = Connector.CreatePrincipalContext(config);
                }
            }
            catch (Exception e)
            {
                ExitError(e, "There was an error loading your configuration", 1);
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
                ExitError(e, "There was an error loading your neo4j configuration", 2);
            }

            
            Console.WriteLine($"Starting scan\nScanner ID: {NeoWriter.ScannerID}\nScan ID: {NeoWriter.ScanID}\n");
            

            NeoWriter.WriteHeaders();

            //write the foreign principals
            NeoWriter.WriteIDataCollector(new ForeignSecurityPrincipalCollector(context), driver, true, true);

            //process users
            NeoWriter.WriteIDataCollector(new UsersCollector(context), driver, true, true);
            NeoWriter.WriteIDataCollector(new ManagerCollector(), driver, true, true);

            //load the computers
            NeoWriter.WriteIDataCollector(new ComputersCollector(context), driver, true, true);

            //process groups
            GroupsCollector groupscollector = new GroupsCollector(context);
            NeoWriter.WriteIDataCollector(groupscollector, driver, true, true);
            NeoWriter.WriteIDataCollector(groupscollector.GetMembershipsCollector(), driver, true, true);

            //process foreign item connections
            NeoWriter.WriteIDataCollector(new ForeignSecurityPrincipalConnectionCollector(), driver, true, true);
            NeoWriter.WriteIDataCollector(new RemoteForeignSecurityPrincipalConnectionCollector(), driver, true, true);

            NeoQueryData nopropsdata = new NeoQueryData();
            nopropsdata.ScanID = NeoWriter.ScanID;
            nopropsdata.ScannerID = NeoWriter.ScannerID;

            //create primary group mappings
            Console.Write("Setting primary groups");
            NeoWriter.RunQueryAsync(StandAloneQueries.SetPrimaryGroupRelationships, nopropsdata, driver, true, true).GetAwaiter().GetResult();

            Console.WriteLine();
            Console.WriteLine("*Cleaning up");

            //*cleanup deleted items
            //remove group memberships that have been deleted
            Console.Write("Deleted group memberships");
            NeoWriter.RunQuery(StandAloneQueries.DeletedGroupMemberships, nopropsdata, driver, true, true);

            Console.Write("Deleted foreign group memberships");
            NeoWriter.RunQuery(StandAloneQueries.DeletedForeignGroupMemberShips, nopropsdata, driver, true, true);

            Console.Write("Deleted manager relationships");
            NeoWriter.RunQuery(StandAloneQueries.DeletedManagers, nopropsdata, driver, true, true);

            //mark deleted objects
            Console.Write("Mark deleted users");
            NeoWriter.RunQuery(StandAloneQueries.GetMarkDeletedObjectsQuery(Types.User), nopropsdata, driver, true, true);

            Console.Write("Mark deleted computers");
            NeoWriter.RunQuery(StandAloneQueries.GetMarkDeletedObjectsQuery(Types.Computer), nopropsdata, driver, true, true);

            Console.Write("Mark deleted groups");
            NeoWriter.RunQuery(StandAloneQueries.GetMarkDeletedObjectsQuery(Types.Group), nopropsdata, driver, true, true);
            
            Console.WriteLine("*Finished cleaning up");
            Console.WriteLine();

            Console.Write("Setting group scopes");
            NeoWriter.RunQuery(StandAloneQueries.SetGroupScope, nopropsdata, driver, true, true);

            Console.Write("Updating member counts");
            NeoWriter.RunQuery(StandAloneQueries.UpdateMemberCounts, nopropsdata, driver, true, true);

            //cleanup
            driver.Dispose();
            context.Dispose();

            totaltimer.Stop();
            double totaltime = totaltimer.ElapsedMilliseconds / 1000;
            Console.WriteLine();
            Console.WriteLine("Finished in " + totaltime + "secs");
            if (_batchmode == true)
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
                Console.WriteLine("Press enter to exit");
                Console.ReadLine();
            }
        }

        public static void ExitError(Exception e, string message, int returncode)
        {
            Console.WriteLine(message);
            Console.WriteLine(e.Message);
            if (_batchmode == false) 
            {
                Console.WriteLine("Press enter to exit");
                Console.ReadLine(); 
            }
            Environment.Exit(returncode);
        }

        public static void ExitError(string message, int returncode)
        {
            Console.WriteLine(message);
            if (_batchmode == false) { Console.ReadLine(); }
            Environment.Exit(returncode);
        }

        public static void ExitError(int returncode)
        {
            if (_batchmode == false) { Console.ReadLine(); }
            Environment.Exit(returncode);
        }

        private static void ShowUsage()
        {
            Console.WriteLine();
            Console.WriteLine("Usage: ADScanner.exe /config:<configfile> /batch");
            Console.WriteLine("/batch makes scanner run in batch mode and does not wait before exit");
        }
    }
}
