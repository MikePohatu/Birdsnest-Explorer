using System;
using System.DirectoryServices;
using System.Collections.Generic;
using System.Diagnostics;
using Neo4j.Driver.V1;
using ADScanner.ActiveDirectory;
using ADScanner.Neo4j;
using common;

namespace ADScanner
{
    class Program
    {
        static void Main(string[] args)
        {
            Stopwatch steptimer = new Stopwatch();
            Stopwatch totaltimer = new Stopwatch();

            string _appdir = AppDomain.CurrentDomain.BaseDirectory;
            string neoconfigfile = _appdir + @"\neoconfig.json";
            string configfile = _appdir + @"\adconfig.json";
            int relcounter = 0;
            bool batchmode = false;
            string scanid = ShortGuid.NewGuid().ToString();

            IDriver driver = null;
            DirectoryEntry rootDE = null;

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
                using (Configuration config = Configuration.LoadConfiguration(configfile))
                {
                    Writer.DomainID = config.ID;
                    if (string.IsNullOrEmpty(Writer.DomainID))
                    {
                        Console.WriteLine("Your configuration does not have a scanner ID. A random ID will be generated for you below:");
                        Console.WriteLine(ShortGuid.NewGuid().ToString());
                        Console.WriteLine();
                        if (batchmode == false)
                        {
                            Console.WriteLine("Press any key to exit");
                            Console.ReadLine();
                        }
                        Environment.Exit(2);
                    }
                    rootDE = ConnectToAD(config);
                }
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
            

            

            //process groups
            //load the groups
            using (SearchResultCollection results = QueryHandler.GetAllGroupResults(rootDE))
            {
                Console.Write("Adding groups to graph");
                if (results != null)
                {
                    steptimer.Restart();
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
                            Console.Write(".");
                            groupcount = groupcount + Writer.MergeADGroups(ListExtensions.ListPop(groupprops,1000), driver);
                        }
                        foreach (string dn in g.MemberOfDNs)
                        {
                            groupmappings.Add(Writer.GetGroupRelationshipObject(g.Type, g.ID, dn, scanid));
                        }
                    }
                    //purge any remaining groups and mappings to the database
                    if (groupprops.Count > 0) { groupcount = groupcount + Writer.MergeADGroups(groupprops, driver); }
                    steptimer.Stop();
                    Console.WriteLine();
                    Console.WriteLine("Created " + groupcount + " groups in " + steptimer.ElapsedMilliseconds + "ms");

                    Console.Write("Creating group->group mappings");
                    steptimer.Restart();
                    while (groupmappings.Count > 0)
                    {
                        relationshipcount = relationshipcount + Writer.MergeGroupRelationships(Types.Group, ListExtensions.ListPop(groupmappings, 1000), driver);
                        Console.Write(".");
                    }
                    steptimer.Stop();
                    Console.WriteLine();
                    Console.WriteLine("Created " + relationshipcount + " group->group mappings in " + steptimer.ElapsedMilliseconds + "ms");
                    Console.WriteLine();
                }   
            }

            //process users
            //load the users
            using (SearchResultCollection results = QueryHandler.GetAllUserResults(rootDE))
            {
                Console.Write("Adding users to graph");
                if (results != null)
                {
                    steptimer.Restart();
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
                            Console.Write(".");
                            usercount = usercount + Writer.MergeAdUsers(ListExtensions.ListPop(userprops, 1000), driver);
                        }
                        foreach (string dn in u.MemberOfDNs)
                        {
                            groupmappings.Add(Writer.GetGroupRelationshipObject(u.Type, u.ID, dn, scanid));
                        }
                    }
                    //purge any remaining groups and mappings to the database
                    if (userprops.Count > 0) { usercount = usercount + Writer.MergeAdUsers(userprops, driver); }
                    steptimer.Stop();
                    Console.WriteLine();
                    Console.WriteLine("Created " + usercount + " users in " + steptimer.ElapsedMilliseconds + "ms");

                    Console.Write("Creating user->group mappings");
                    steptimer.Restart();
                    while (groupmappings.Count > 0)
                    {
                        relationshipcount = relationshipcount + Writer.MergeGroupRelationships(Types.User, ListExtensions.ListPop(groupmappings, 1000), driver);
                        Console.Write(".");
                    }
                    steptimer.Stop();
                    Console.WriteLine();
                    Console.WriteLine("Created " + relationshipcount + " user->group mappings in " + steptimer.ElapsedMilliseconds + "ms");
                    Console.WriteLine();
                }
            }

            //load the computers
            using (SearchResultCollection results = QueryHandler.GetAllComputerResults(rootDE))
            {
                Console.Write("Adding computers to graph");
                if (results != null)
                {
                    steptimer.Restart();
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
                            Console.Write(".");
                            computercount = computercount + Writer.MergeAdComputers(ListExtensions.ListPop(compprops, 1000), driver);
                        }
                        foreach (string dn in c.MemberOfDNs)
                        {
                            groupmappings.Add(Writer.GetGroupRelationshipObject(c.Type, c.ID, dn, scanid));
                        }
                    }
                    //purge any remaining groups and mappings to the database
                    if (compprops.Count > 0) { computercount = computercount + Writer.MergeAdComputers(compprops, driver); }
                    steptimer.Stop();
                    Console.WriteLine();
                    Console.WriteLine("Created " + computercount + " computers in " + steptimer.ElapsedMilliseconds + "ms");

                    Console.Write("Creating computer->group mappings");
                    steptimer.Restart();
                    while (groupmappings.Count > 0)
                    {
                        relationshipcount = relationshipcount + Writer.MergeGroupRelationships(Types.Computer, ListExtensions.ListPop(groupmappings, 1000), driver);
                        Console.Write(".");
                    }
                    steptimer.Stop();
                    Console.WriteLine();
                    Console.WriteLine("Created " + relationshipcount + " computer->group mappings in " + steptimer.ElapsedMilliseconds + "ms");
                    Console.WriteLine();
                }
            }

            steptimer.Restart();
            //create primary group mappings
            relcounter = relcounter + Writer.CreatePrimaryGroupRelationships(driver, scanid);
            steptimer.Stop();
            Console.WriteLine("Created " + relcounter + " primary group relationships in " + steptimer.ElapsedMilliseconds + "ms");

            steptimer.Restart();
            //create primary group mappings
            int propcounter = Writer.UpdateMemberCounts(driver);
            steptimer.Stop();
            Console.WriteLine("Created " + propcounter + " group membership counts updated in " + steptimer.ElapsedMilliseconds + "ms");


            Console.WriteLine();
            Console.WriteLine("Cleaning up deleted items");
            steptimer.Restart();

            //*cleanup deleted items
            //remove group memberships that have been deleted
            Console.WriteLine("Deleted " + Writer.RemoveDeletedGroupMemberShips(driver, scanid) + " relationships");

            //mark deleted objects
            Console.WriteLine("Marked " + Writer.FindAndMarkDeletedItems(Types.User, driver, scanid) + " users as deleted");
            Console.WriteLine("Marked " + Writer.FindAndMarkDeletedItems(Types.Computer, driver, scanid) + " computers as deleted");
            Console.WriteLine("Marked " + Writer.FindAndMarkDeletedItems(Types.Group, driver, scanid) + " groups as deleted");
            Console.WriteLine();
            steptimer.Stop();
            Console.WriteLine("Finished cleaning up deleted items in " + steptimer.ElapsedMilliseconds + "ms");

            //mark deleted objects
            steptimer.Restart();
            Writer.SetGroupScope(driver);
            steptimer.Stop();
            //Console.WriteLine();
            Console.WriteLine("Set group scopes in " + steptimer.ElapsedMilliseconds + "ms");

            //mark deleted objects
            steptimer.Restart();
            Writer.UpdateMetadata(driver);
            steptimer.Stop();
            //Console.WriteLine();
            Console.WriteLine("Updated metadata " + steptimer.ElapsedMilliseconds + "ms");


            //cleanup
            driver.Dispose();
            rootDE.Dispose();

            totaltimer.Stop();
            double totaltime = totaltimer.ElapsedMilliseconds / 1000;
            Console.WriteLine();
            Console.WriteLine("Finished in " + totaltime + "secs");
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
