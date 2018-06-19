using System;
using System.DirectoryServices;
using Neo4j.Driver.V1;
using ADScanner.ActiveDirectory;
using ADScanner.Neo4j;
using ADScanner.common;

namespace ADScanner
{
    class Program
    {
        static void Main(string[] args)
        {
            string _appdir = AppDomain.CurrentDomain.BaseDirectory;
            string configfile = _appdir + @"\config.json";
            int relcounter = 0;
            int counter = 0;
            bool batchmode = false;
            string scanid = ShortGuid.NewGuid().ToString();

            IDriver driver;
            DirectoryEntry rootDE;

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
            using (Configuration config = LoadConfig(configfile))
            {
                driver = ConnectToNeo(config);
                rootDE = ConnectToAD(config);
            }

            //open the session to neo4j
            using (ISession session = driver.Session())
            {
                //load the groups
                using (SearchResultCollection results = QueryHandler.GetAllGroupResults(rootDE))
                {
                    Console.Write("Adding groups to graph");
                    if (results != null)
                    {
                        foreach (SearchResult result in results)
                        {
                            ADGroup g = new ADGroup(result);
                            relcounter = relcounter + Writer.MergeADGroup(g, session, scanid);
                            counter++;
                            if (counter == 100)
                            {
                                Console.Write(".");
                                counter = 0;
                            }
                        }
                    }
                    Console.WriteLine();
                }
            }

            using (ISession session = driver.Session())
            {
                //load the users
                using (SearchResultCollection results = QueryHandler.GetAllUserResults(rootDE))
                {
                    Console.WriteLine("Adding users to graph");
                    if (results != null)
                    {
                        foreach (SearchResult result in results)
                        {
                            ADUser u = new ADUser(result);
                            relcounter = relcounter + Writer.MergeAdUser(u, session, scanid);
                        }
                    }
                }
            }

            using (ISession session = driver.Session())
            {
                //load the computers
                using (SearchResultCollection results = QueryHandler.GetAllComputerResults(rootDE))
                {
                    Console.WriteLine("Adding computers to graph");
                    if (results != null)
                    {
                        foreach (SearchResult result in results)
                        {
                            ADComputer c = new ADComputer(result);
                            relcounter = relcounter + Writer.MergeAdComputer(c, session, scanid);
                        }
                    }
                }
            }

            using (ISession session = driver.Session())
            {
                //create primary group mappings
                relcounter = relcounter + Writer.CreatePrimaryGroupRelationships(session, scanid);
                Console.WriteLine("Created " + relcounter + " primary group relationships");
            }

            using (ISession session = driver.Session())
            {
                //*cleanup deleted items
                //remove group memberships that have been deleted
                Console.WriteLine("Deleted " + Writer.RemoveDeletedGroupMemberShips(session, scanid) + " relationships");
            }

            using (ISession session = driver.Session())
            {
                //mark deleted objects
                Console.WriteLine("Deleted " + Writer.FindAndMarkDeletedItems(Types.User, session, scanid) + " deleted user relationships");
                Console.WriteLine("Deleted " + Writer.FindAndMarkDeletedItems(Types.Computer, session, scanid) + " deleted computer relationships");
            }

            //cleanup
            driver.Dispose();
            rootDE.Dispose();

            Console.Write("Finished. Exiting.");
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

        private static Configuration LoadConfig(string configfile)
        {
            try
            {
                return FileHandler.ReadConfigurationFromFile(configfile);
            }
            catch(Exception e)
            {
                Console.WriteLine("Error loading configuration file: " + e.Message);
                Environment.Exit(1001);
            }

            return null;
        }

        private static IDriver ConnectToNeo(Configuration config)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(config.DB_Password) || string.IsNullOrWhiteSpace(config.DB_Username))
                { return GraphDatabase.Driver(config.DB_URI); }
                else
                { return GraphDatabase.Driver(config.DB_URI, AuthTokens.Basic(config.DB_Username, config.DB_Password)); }
                
            }
            catch(Exception e)
            {
                Console.WriteLine("Error connecting to Neo4j: " + e.Message);
                Environment.Exit(1002);
            }
            return null;
        }

        private static DirectoryEntry ConnectToAD(Configuration config)
        {
            try
            {
                Console.WriteLine("Connecting to domain: " + config.AD_DomainPath);
                if (string.IsNullOrWhiteSpace(config.AD_Password) || string.IsNullOrWhiteSpace(config.AD_Username))
                { return new DirectoryEntry(config.AD_DomainPath); }
                else
                { return new DirectoryEntry(config.AD_DomainPath, config.AD_Username, config.AD_Password); } 
            }
            catch (Exception e)
            {
                Console.WriteLine("Error connecting to Active Directory: " + e.Message);
                Environment.Exit(1003);
            }
            return null;
        }
    }
}
