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
                    else { _connector.Connect(config.Username, config.Password, config.Domain, config.SiteServer); }
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
            int count = 0;
            Console.Write("Creating collection nodes");
            List<SccmCollection> collections = _connector.GetCollections();
            count = MergeList(collections, Writer.MergeCollections, driver);
            Console.WriteLine("Created " + count + " collection nodes");

            Console.WriteLine("Creating limiting collection connections");
            count = Writer.ConnectLimitingCollections(driver.Session());
            Console.WriteLine("Created " + count + " limiting collection connections");

            //applications
            Console.Write("Creaing application nodes");
            List<SccmApplication> applications = _connector.GetApplications();
            count = MergeList(applications, Writer.MergeApplications, driver);
            Console.WriteLine("Created " + count + " application nodes");

            //packages
            Console.Write("Creating package nodes");
            List<SccmPackage> packages = _connector.GetPackages();
            count = MergeList(packages, Writer.MergePackages, driver);
            Console.WriteLine("Created " + count + " package nodes");

            //package programs
            Console.Write("Creating  package program nodes");
            List<SccmPackageProgram> packageprograms = _connector.GetPackagePrograms();
            count = MergeList(packageprograms, Writer.MergePackagePrograms, driver);
            Console.WriteLine("Created " + count + " package program nodes");

            //task sequences
            Console.Write("Creating task sequence nodes");
            List<SccmTaskSequence> tasksequences = _connector.GetTaskSequences();
            count = MergeList(tasksequences, Writer.MergeTaskSequences, driver);
            Console.WriteLine("Created " + count + " task sequence nodes");

            //SUGs
            //count = Writer.MergeSoftwareUpdateGroups(_connector.getso(), driver.Session());
            //Console.WriteLine("Created " + count + " package nodes");

            //deployments - applications
            Console.Write("Creating application deployment relationships");
            List<SMS_DeploymentSummary> summarys = _connector.GetApplicationDeployments();
            count = MergeList(summarys, Writer.MergeApplicationDeployments, driver);
            Console.WriteLine("Created " + count + " application deployment relationships");

            //deployments - Package programs
            Console.Write("Creating package program deployment relationships");
            summarys = _connector.GetPackageProgramDeployments();
            count = MergeList(summarys, Writer.MergePackageProgramDeployments, driver);
            Console.WriteLine("Created " + count + " package program deployment relationships");

            //devices
            Console.Write("Creating devices");
            List<SccmDevice> devs = _connector.GetAllDevices();
            count = MergeList(devs, Writer.MergeDevices, driver);
            Console.WriteLine("Created " + count + " devices");

            //users
            Console.Write("Creating users");
            List<SccmUser> users = _connector.GetAllUsers();
            count = MergeList(users, Writer.MergeUsers, driver);
            Console.WriteLine("Created " + count + " users");

            //ad mappings
            count = Writer.ConnectCmToAdObjects(driver.Session());
            Console.WriteLine("Created " + count + " AD to CM mappings");

            //collection members
            Console.Write("Creating collection memberships");
            List<object> memberships = _connector.GetCollectionMemberships();
            count = MergeList(memberships, Writer.MergeCollectionMembers, driver);
            Console.WriteLine("Created " + count + " collection memberships");

            //cleanup
            count = Writer.CleanupCmObjects(driver.Session());
            Console.WriteLine("Cleaned up " + count + " items");

            //Metadata
            Writer.UpdateMetadata(driver);
            Console.WriteLine("Updated metadata");

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
            Console.WriteLine("Usage: CMScanner.exe /config:<configfile> /batch");
            Console.WriteLine("/batch makes scanner run in batch mode and does not wait before exit");
        }

        private delegate int MergeDelegate<T>(List<T> itemlist, ISession session);
        private static int MergeList<T>(List<T> itemlist, MergeDelegate<T> method, IDriver driver)
        {
            int count = 0;
            while (itemlist.Count > 1000)
            {
                count = count + method(ListExtensions.ListPop(itemlist, 1000), driver.Session());
                Console.Write(".");
            }
            count = count + method(itemlist, driver.Session());
            Console.WriteLine(".");
            return count;
        }
    }
}
