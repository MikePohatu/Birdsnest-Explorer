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
            string configfile = _appdir + @"\fsconfig.json";
            //int relcounter = 0;
            bool batchmode = false;
            string scanid = ShortGuid.NewGuid().ToString();

            IDriver driver;

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

            using (Configuration config = Configuration.LoadConfiguration(configfile))
            {
                foreach (Credential cred in config.Credentials)
                {
                    NetworkCredential netcred = new NetworkCredential(cred.Username,cred.Password,cred.Domain);
                    credentials.Add(cred.ID, netcred);
                }
                datastores = config.Datastores;
                driver = Neo4jConnector.ConnectToNeo(config);
            }


            

            foreach (DataStore ds in datastores)
            {
                Crawler crawler = new Crawler(driver);
                foreach (FileSystem fs in ds.FileSystems)
                {
                    NetworkCredential fscred;
                    if (credentials.TryGetValue(fs.CredentialID, out fscred))
                    {
                        crawler.CrawlRoot(ds,fs.Path, fscred);
                    }
                } 
            }

            Console.WriteLine("Finished");
            if (batchmode == false) { Console.ReadLine(); }
            
        }
    }
}
