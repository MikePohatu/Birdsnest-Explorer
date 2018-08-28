using System;
using System.Collections.Generic;
using System.Diagnostics;
using Neo4j.Driver.V1;
using System.Linq;
using common;
using System.Net;

namespace FSScanner
{
    class Program
    {
        static void Main(string[] args)
        {
            Stopwatch steptimer = new Stopwatch();
            Stopwatch totaltimer = new Stopwatch();

            Dictionary<string, NetworkCredential> credentials = new Dictionary<string, NetworkCredential>();
            List<DataStore> datastores = new List<DataStore>();

            string _appdir = AppDomain.CurrentDomain.BaseDirectory;
            string neoconfigfile = _appdir + @"\neoconfig.json";
            string configfile = _appdir + @"\fsconfig.json";
            //int relcounter = 0;
            bool batchmode = false;
            string scanid = ShortGuid.NewGuid().ToString();

            IDriver driver = null;

            totaltimer.Start();
            foreach (string arg in args)
            {
                string[] param = arg.Split(new[] { ":" }, 2, StringSplitOptions.None);
                switch (param[0].ToUpper())
                {
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

            try
            {
                using (Configuration config = Configuration.LoadConfiguration(configfile))
                {
                    foreach (Credential cred in config.Credentials)
                    {
                        NetworkCredential netcred = new NetworkCredential(cred.Username, cred.Password, cred.Domain);
                        credentials.Add(cred.ID, netcred);
                    }
                    datastores = config.Datastores;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("There was an error loading your configuration");
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
            

            foreach (DataStore ds in datastores)
            {
                Crawler crawler = new Crawler(driver);
                foreach (FileSystem fs in ds.FileSystems)
                {
                    if (string.IsNullOrEmpty(fs.Path))
                    {
                        Console.WriteLine("Filesystem missing \"path\" property");
                        continue;
                    }
                    NetworkCredential fscred;
                    if (!string.IsNullOrEmpty(fs.CredentialID) && (credentials.TryGetValue(fs.CredentialID, out fscred)))
                    {
                        crawler.CrawlRoot(ds, fs.Path, fscred);
                    }
                    else
                    {
                        crawler.CrawlRoot(ds, fs.Path);
                    }
                }
            }

            totaltimer.Stop();
            Console.WriteLine("Finished in " + (totaltimer.ElapsedMilliseconds/1000) + " seconds");
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
    }
}
