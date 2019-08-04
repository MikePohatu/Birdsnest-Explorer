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

            //load the config
            try
            {
                wuconfig = Configuration.LoadConfiguration(configfile);

                if (string.IsNullOrWhiteSpace(wuconfig.HostName) || wuconfig.Port <= 0 )
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
                    IUpdateServer updateserver = AdminProxy.GetUpdateServer(wuconfig.HostName, wuconfig.UseSSL, wuconfig.Port);
                }

                
                //UpdateCollection allupdates = updateserver.GetUpdates(ApprovedStates.Any, DateTime.MinValue, DateTime.MaxValue, null, null);
            }
            catch (Exception e)
            {
                Console.WriteLine("There was an error communicating with the WSUS server: ");
                Console.WriteLine(e.Message);
                Console.WriteLine(e.TargetSite.ToString());
                if (batchmode == false) { Console.ReadLine(); }
            }
            

        }
    }
}
