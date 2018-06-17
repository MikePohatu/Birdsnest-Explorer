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
            int counter = 0;
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
                    if (results != null)
                    {
                        foreach (SearchResult result in results)
                        {
                            ADGroup g = new ADGroup(result);
                            counter = counter + Writer.MergeADGroupMemberObjectOnPath(g, session, scanid);
                        }
                    }
                }

                //load the users
                using (SearchResultCollection results = QueryHandler.GetAllUserResults(rootDE))
                {
                    if (results != null)
                    {
                        foreach (SearchResult result in results)
                        {
                            ADUser u = new ADUser(result);
                            counter = counter + Writer.MergeADGroupMemberObjectOnPath(u, session, scanid);
                        }
                    }
                }

                //load the computers
                using (SearchResultCollection results = QueryHandler.GetAllComputerResults(rootDE))
                {
                    if (results != null)
                    {
                        foreach (SearchResult result in results)
                        {
                            ADComputer c = new ADComputer(result);
                            counter = counter + Writer.MergeADGroupMemberObjectOnPath(c, session, scanid);
                        }
                    }
                }

                //create primary group mappings
                counter = counter + Writer.CreatePrimaryGroupRelationships(session, scanid);
                Console.WriteLine("Created " + counter + " primary group relationships");

                //*cleanup deleted items
                //remove group memberships that have been deleted
                Console.WriteLine("Deleted " + Writer.RemoveDeletedGroupMemberShips(session, scanid) + " relationships");

                //mark deleted objects
                Console.WriteLine("Deleted " + Writer.FindAndMarkDeletedItems(Labels.User, session, scanid) + " deleted user relationships");
                Console.WriteLine("Deleted " + Writer.FindAndMarkDeletedItems(Labels.Computer, session, scanid) + " deleted computer relationships");
            }

            //cleanup
            driver.Dispose();
            rootDE.Dispose();

            Console.Write("Finished. Exiting.");
            for (int i =0; i<3; i++)
            {
                System.Threading.Thread.Sleep(500);
                Console.Write(".");
            }
            System.Threading.Thread.Sleep(1000);
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
