using common;
using Neo4j.Driver.V1;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CMScanner.Sccm;
using CMScanner.Neo4j;

namespace CMScanner
{
    class Program
    {
        static void Main(string[] args)
        {
            Stopwatch steptimer = new Stopwatch();
            Stopwatch totaltimer = new Stopwatch();

            SccmConnector _connector = new SccmConnector();

            string _appdir = AppDomain.CurrentDomain.BaseDirectory;
            string neoconfigfile = _appdir + @"\neoconfig.json";
            string configfile = _appdir + @"\cmconfig.json";
            bool batchmode = false;
            Writer.ScanID = ShortGuid.NewGuid().ToString();

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
                    if (string.IsNullOrEmpty(config.Username)) { _connector.Connect(config.SiteServer); }
                    else { _connector.Connect(config.Username,config.Password,config.Domain, config.SiteServer); }
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

            //collections
            int count = Writer.MergeCollections(_connector.GetCollections(), driver.Session());
            Console.WriteLine("Created " + count + " collection nodes");
            Writer.ConnectLimitingCollections(driver.Session());

            //applications
            count = Writer.MergeApplications(_connector.GetApplications(), driver.Session());
            Console.WriteLine("Created " + count + " application nodes");

            //packages
            count = Writer.MergePackages(_connector.GetPackages(), driver.Session());
            Console.WriteLine("Created " + count + " package nodes");

            //package programs
            count = Writer.MergePackagePrograms(_connector.GetPackagePrograms(), driver.Session());
            Console.WriteLine("Created " + count + " package program nodes");

            //task sequences
            count = Writer.MergeTaskSequences(_connector.GetTaskSequences(), driver.Session());
            Console.WriteLine("Created " + count + " task sequence nodes");

            //SUGs
            //count = Writer.MergeSoftwareUpdateGroups(_connector.getso(), driver.Session());
            //Console.WriteLine("Created " + count + " package nodes");

            //deployments - applications
            count = Writer.MergeApplicationDeployments(_connector.GetApplicationDeployments(), driver.Session());
            Console.WriteLine("Created " + count + " application deployment relationships");

            //deployments - Package programs
            count = Writer.MergePackageProgramDeployments(_connector.GetPackageProgramDeployments(), driver.Session());
            Console.WriteLine("Created " + count + " package program deployment relationships");

            //devices
            count = Writer.MergeDevices(_connector.GetAllDevices(), driver.Session());
            Console.WriteLine("Created " + count + " devices");

            //users
            count = Writer.MergeUsers(_connector.GetAllUsers(), driver.Session());
            Console.WriteLine("Created " + count + " users");

            //collection members
            count = Writer.MergeCollectionMembers(_connector.GetCollectionMemberships(), driver.Session());
            Console.WriteLine("Created " + count + " collection memberships");

            //ad mappings
            count = Writer.ConnectCmToAdObjects(driver.Session());
            Console.WriteLine("Created " + count + " AD to CM mappings");

            //cleanup
            count = Writer.CleanupCmObjects(driver.Session());
            Console.WriteLine("Cleaned up " + count + " items");

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
            Console.WriteLine("Usage: CMScanner.exe /config:<configfile> /batch");
            Console.WriteLine("/batch makes scanner run in batch mode and does not wait before exit");
        }
    }
}
