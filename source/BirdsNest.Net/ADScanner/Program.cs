using System;
using System.DirectoryServices;
using Neo4j.Driver.V1;
using ADScanner.ActiveDirectory;
using ADScanner.Neo4j;

namespace ADScanner
{
    class Program
    {
        static void Main(string[] args)
        {
            string _appdir = AppDomain.CurrentDomain.BaseDirectory;
            string configfile = _appdir + @"\config.json";
            int relcount = 0;

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
                            Writer.MergeNodeOnPath(g, session);
                            relcount = relcount + Writer.AddIsMemberOfADGroups(g, g.MemberOfDNs, session);
                            relcount = relcount + Writer.AddMembersOfADGroup(g, g.MemberDNs, session);
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
                            Writer.MergeNodeOnPath(u, session);
                            relcount = relcount + Writer.AddIsMemberOfADGroups(u, u.MemberOfDNs, session);
                            relcount = relcount + Writer.AddIsMemberOfPrimaryADGroup(u, session);
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
                            Writer.MergeNodeOnPath(c, session);
                            relcount = relcount + Writer.AddIsMemberOfADGroups(c, c.MemberOfDNs, session);
                            relcount = relcount + Writer.AddIsMemberOfPrimaryADGroup(c, session);
                        }
                    }
                }

                Console.WriteLine("Processed " + relcount + " relationships");
            }

            //cleanup
            driver.Dispose();
            rootDE.Dispose();

            Console.Write("Finished");
            for (int i =0; i<3; i++)
            {
                System.Threading.Thread.Sleep(1000);
                Console.Write(".");
            }
            Console.WriteLine(".");
            Console.WriteLine("Exiting");
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
                if (string.IsNullOrWhiteSpace(config.AD_Password) || string.IsNullOrWhiteSpace(config.AD_Username))
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
