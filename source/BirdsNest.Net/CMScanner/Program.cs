using common;
using Neo4j.Driver.V1;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CMScanner.CmConverter;

namespace CMScanner
{
    class Program
    {
        static void Main(string[] args)
        {
            Stopwatch steptimer = new Stopwatch();
            Stopwatch totaltimer = new Stopwatch();
            string _appdir = AppDomain.CurrentDomain.BaseDirectory;
            string neoconfigfile = _appdir + @"\neoconfig.json";
            string configfile = _appdir + @"\cmconfig.json";
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
                    scannerid = config.ScannerID;
                    if (string.IsNullOrEmpty(config.Username)) { 
                        Connector.Instance.Connect(config.SiteServer);
                    }
                    else { 
                        Connector.Instance.Connect(config.Username, config.Password, config.Domain, config.SiteServer);
                    }
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

            
            int count = 0;
            NeoWriter.ScanID = scanid;

            List<ICmCollector> collectors = new List<ICmCollector>();
            collectors.Add(new CmCollections());
            collectors.Add(new CmLimitingCollections());
            collectors.Add(new CmApplications());
            collectors.Add(new CmPackages());
            collectors.Add(new CmPackagePrograms());
            collectors.Add(new CmTaskSequences());
            collectors.Add(new CmDeployments());
            collectors.Add(new CmUsers());
            collectors.Add(new CmDevices());
            collectors.Add(new CmDeviceAdConnections());
            collectors.Add(new CmUserAdConnections());
            collectors.Add(new CmCollectionMemberships());

            foreach (ICmCollector collector in collectors)
            {
                Console.Write(collector.ProgressMessage);
                NeoQueryData collectionsdata = collector.CollectData();
                collectionsdata.ScanID = scanid;
                collectionsdata.ScannerID = scannerid;
                var summary = NeoWriter.RunQuery(collector.Query, collectionsdata, driver.Session());
                Console.WriteLine(collector.GetSummaryString(summary));
            }

            //cleanup
            count = Cleanup.CleanupCmObjects(driver, scanid, scannerid);
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
