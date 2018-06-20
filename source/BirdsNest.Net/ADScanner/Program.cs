using System;
using System.DirectoryServices;
using System.Collections.Generic;
using Neo4j.Driver.V1;
using ADScanner.ActiveDirectory;
using ADScanner.Neo4j;
using ADScanner.common;
using System.Linq;

namespace ADScanner
{
    class Program
    {
        static void Main(string[] args)
        {
            string _appdir = AppDomain.CurrentDomain.BaseDirectory;
            string configfile = _appdir + @"\config.json";
            int relcounter = 0;
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
                        int groupcount = 0;
                        int relationshipcount = 0;
                        List<object> groupmappings = new List<object>();
                        List<Dictionary<string, object>> groupprops = new List<Dictionary<string, object>>(); 

                        foreach (SearchResult result in results)
                        {
                            ADGroup g = new ADGroup(result,scanid);
                            groupprops.Add(g.Properties);
                            if (groupprops.Count >= 1000)
                            {
                                Console.Write("*");
                                groupcount = groupcount + Writer.MergeADGroups(ListPop(groupprops,1000), session);
                            }
                            foreach (string dn in g.MemberOfDNs)
                            {
                                groupmappings.Add(Writer.GetGroupRelationshipObject(g.ID, dn, scanid));
                                if (groupmappings.Count >= 1000)
                                {
                                    relationshipcount = relationshipcount + Writer.MergeGroupRelationships(ListPop(groupmappings, 1000), session);
                                    Console.Write("#");
                                }
                            }
                        }
                        
                        //purge any remaining groups and mappings to the database
                        if (groupprops.Count > 0) { groupcount = groupcount + Writer.MergeADGroups(groupprops, session); } 
                        if (groupmappings.Count > 0 ) { relationshipcount = relationshipcount + Writer.MergeGroupRelationships(groupmappings, session); }
                        Console.WriteLine();
                        Console.WriteLine("Created " + groupcount + " groups");
                        Console.WriteLine("Created " + relationshipcount + " group->group mappings");
                        Console.WriteLine();
                    }   
                }
            }

            using (ISession session = driver.Session())
            {
                //load the users
                using (SearchResultCollection results = QueryHandler.GetAllUserResults(rootDE))
                {
                    Console.Write("Adding users to graph");
                    if (results != null)
                    {
                        int usercount = 0;
                        int relationshipcount = 0;
                        List<object> groupmappings = new List<object>();
                        List<Dictionary<string, object>> userprops = new List<Dictionary<string, object>>();

                        foreach (SearchResult result in results)
                        {
                            ADUser u = new ADUser(result, scanid);
                            userprops.Add(u.Properties);
                            if (userprops.Count >= 1000)
                            {
                                Console.Write("*");
                                usercount = usercount + Writer.MergeAdUsers(ListPop(userprops, 1000), session);
                            }
                            foreach (string dn in u.MemberOfDNs)
                            {
                                groupmappings.Add(Writer.GetGroupRelationshipObject(u.ID, dn, scanid));
                                if (groupmappings.Count >= 1000)
                                {
                                    relationshipcount = relationshipcount + Writer.MergeGroupRelationships(ListPop(groupmappings, 1000), session);
                                    Console.Write("#");
                                }
                            }
                        }

                        //purge any remaining groups and mappings to the database
                        if (userprops.Count > 0) { usercount = usercount + Writer.MergeAdUsers(userprops, session); }
                        if (groupmappings.Count > 0) { relationshipcount = relationshipcount + Writer.MergeGroupRelationships(groupmappings, session); }
                        Console.WriteLine();
                        Console.WriteLine("Created " + usercount + " users");
                        Console.WriteLine("Created " + relationshipcount + " user->group mappings");
                        Console.WriteLine();
                    }
                }
            }

            using (ISession session = driver.Session())
            {
                //load the computers
                using (SearchResultCollection results = QueryHandler.GetAllComputerResults(rootDE))
                {
                    Console.Write("Adding computers to graph");
                    if (results != null)
                    {
                        int computercount = 0;
                        int relationshipcount = 0;

                        List<object> groupmappings = new List<object>();
                        List<Dictionary<string, object>> compprops = new List<Dictionary<string, object>>();
                        foreach (SearchResult result in results)
                        {
                            ADComputer c = new ADComputer(result, scanid);
                            compprops.Add(c.Properties);
                            if (compprops.Count >= 1000)
                            {
                                Console.Write("*");
                                computercount = computercount + Writer.MergeAdComputers(ListPop(compprops, 1000), session);
                            }
                            foreach (string dn in c.MemberOfDNs)
                            {
                                groupmappings.Add(Writer.GetGroupRelationshipObject(c.ID, dn, scanid));
                                if (groupmappings.Count >= 1000)
                                {
                                    relationshipcount = relationshipcount + Writer.MergeGroupRelationships(ListPop(groupmappings, 1000), session);
                                    Console.Write("#");
                                }
                            }
                        }

                        //purge any remaining groups and mappings to the database
                        if (compprops.Count > 0) { computercount = computercount + Writer.MergeAdComputers(compprops, session); }
                        if (groupmappings.Count > 0) { relationshipcount = relationshipcount + Writer.MergeGroupRelationships(groupmappings, session); }
                        Console.WriteLine();
                        Console.WriteLine("Created " + computercount + " computers");
                        Console.WriteLine("Created " + relationshipcount + " computer->group mappings");
                        Console.WriteLine();
                    }
                }
            }

            using (ISession session = driver.Session())
            {
                //create primary group mappings
                relcounter = relcounter + Writer.CreatePrimaryGroupRelationships(session, scanid);
                Console.WriteLine("Created " + relcounter + " primary group relationships");
            }

            Console.WriteLine();
            Console.WriteLine("Cleaning up deleted items");
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

            Console.WriteLine();
            Console.WriteLine("Finished.");
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

        private static List<T> ListPop<T>(List<T> list, int count)
        {
            List<T> newlist = new List<T>();
            newlist.AddRange(list.Take(count));
            list.RemoveRange(0, count);
            return newlist;
        }
    }
}
