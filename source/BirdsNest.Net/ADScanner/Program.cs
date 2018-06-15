using System;
using System.Diagnostics;
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
            IDriver driver;
            DirectoryEntry rootDE;

            foreach (string arg in args)
            {
                string[] param = arg.Split(new[] { ":" }, 2, StringSplitOptions.None);
                Debug.WriteLine(param[0] + " " + param[1]);
                switch (param[0].ToUpper())
                {
                    case "/CONFIG":
                        configfile = param[1];
                        break;
                    default:
                        break;
                }
            }

            using (Configuration config = LoadConfig(configfile))
            {
                driver = ConnectToNeo(config);
                rootDE = ConnectToAD(config);
            }

            SearchResultCollection results = QueryHandler.GetAllGroupResults(rootDE);
            if (results != null)
            {
                foreach (SearchResult result in results)
                {
                    ADGroup g = new ADGroup(result);
                    Writer.MergeNodeOnPath(g, driver);
                    Writer.AddIsMemberOfADGroups(g, g.MemberOfDNs,driver);
                    Writer.AddMembersOfADGroup(g, g.MemberDNs, driver);
                }
            }

            results = QueryHandler.GetAllUserResults(rootDE);
            if (results != null)
            {
                foreach (SearchResult result in results)
                {
                    ADUser u = new ADUser(result);
                    Writer.MergeNodeOnPath(u, driver);
                    Writer.AddIsMemberOfADGroups(u, u.MemberOfDNs, driver);
                }
            }

            results = QueryHandler.GetAllComputerResults(rootDE);
            if (results != null)
            {
                foreach (SearchResult result in results)
                {
                    ADComputer c = new ADComputer(result);
                    Writer.MergeNodeOnPath(c, driver);
                    Writer.AddIsMemberOfADGroups(c, c.MemberOfDNs, driver);
                }
            }

            driver.Dispose();
            rootDE.Dispose();

            Console.WriteLine("Finished");
            Console.ReadLine();
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
                return GraphDatabase.Driver(config.DB_URI, AuthTokens.Basic(config.DB_Username, config.DB_Password));
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
