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

            IDriver driver;
            DirectoryEntry rootDE;

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
            using (Configuration config = Configuration.LoadConfiguration(configfile))
            {
                rootDE = ConnectToAD(config);
            }

            using (NeoConfiguration config = NeoConfiguration.LoadConfiguration(neoconfigfile))
            {
                driver = Neo4jConnector.ConnectToNeo(config);
            }

            //process groups
            using (ISession session = driver.Session())
            {
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
                                groupcount = groupcount + Writer.MergeADGroups(ListExtensions.ListPop(groupprops,1000), session);
                            }
                            foreach (string dn in g.MemberOfDNs)
                            {
                                groupmappings.Add(Writer.GetGroupRelationshipObject(g.Type, g.ID, dn, scanid));
                            }
                        }
                        //purge any remaining groups and mappings to the database
                        if (groupprops.Count > 0) { groupcount = groupcount + Writer.MergeADGroups(groupprops, session); }
                        steptimer.Stop();
                        Console.WriteLine();
                        Console.WriteLine("Created " + groupcount + " groups in " + steptimer.ElapsedMilliseconds + "ms");

                        Console.Write("Creating group->group mappings");
                        steptimer.Restart();
                        while (groupmappings.Count > 0)
                        {
                            relationshipcount = relationshipcount + Writer.MergeGroupRelationships(Types.Group, ListExtensions.ListPop(groupmappings, 1000), session);
                            Console.Write(".");
                        }
                        steptimer.Stop();
                        Console.WriteLine();
                        Console.WriteLine("Created " + relationshipcount + " group->group mappings in " + steptimer.ElapsedMilliseconds + "ms");
                        Console.WriteLine();
                    }   
                }
            }

            //process users
            using (ISession session = driver.Session())
            {
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
                                usercount = usercount + Writer.MergeAdUsers(ListExtensions.ListPop(userprops, 1000), session);
                            }
                            foreach (string dn in u.MemberOfDNs)
                            {
                                groupmappings.Add(Writer.GetGroupRelationshipObject(u.Type, u.ID, dn, scanid));
                            }
                        }
                        //purge any remaining groups and mappings to the database
                        if (userprops.Count > 0) { usercount = usercount + Writer.MergeAdUsers(userprops, session); }
                        steptimer.Stop();
                        Console.WriteLine();
                        Console.WriteLine("Created " + usercount + " users in " + steptimer.ElapsedMilliseconds + "ms");

                        Console.Write("Creating user->group mappings");
                        steptimer.Restart();
                        while (groupmappings.Count > 0)
                        {
                            relationshipcount = relationshipcount + Writer.MergeGroupRelationships(Types.User, ListExtensions.ListPop(groupmappings, 1000), session);
                            Console.Write(".");
                        }
                        steptimer.Stop();
                        Console.WriteLine();
                        Console.WriteLine("Created " + relationshipcount + " user->group mappings in " + steptimer.ElapsedMilliseconds + "ms");
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
                                computercount = computercount + Writer.MergeAdComputers(ListExtensions.ListPop(compprops, 1000), session);
                            }
                            foreach (string dn in c.MemberOfDNs)
                            {
                                groupmappings.Add(Writer.GetGroupRelationshipObject(c.Type, c.ID, dn, scanid));
                            }
                        }
                        //purge any remaining groups and mappings to the database
                        if (compprops.Count > 0) { computercount = computercount + Writer.MergeAdComputers(compprops, session); }
                        steptimer.Stop();
                        Console.WriteLine();
                        Console.WriteLine("Created " + computercount + " computers in " + steptimer.ElapsedMilliseconds + "ms");

                        Console.Write("Creating computer->group mappings");
                        steptimer.Restart();
                        while (groupmappings.Count > 0)
                        {
                            relationshipcount = relationshipcount + Writer.MergeGroupRelationships(Types.Computer, ListExtensions.ListPop(groupmappings, 1000), session);
                            Console.Write(".");
                        }
                        steptimer.Stop();
                        Console.WriteLine();
                        Console.WriteLine("Created " + relationshipcount + " computer->group mappings in " + steptimer.ElapsedMilliseconds + "ms");
                        Console.WriteLine();
                    }
                }
            }

            using (ISession session = driver.Session())
            {
                steptimer.Restart();
                //create primary group mappings
                relcounter = relcounter + Writer.CreatePrimaryGroupRelationships(session, scanid);
                steptimer.Stop();
                Console.WriteLine("Created " + relcounter + " primary group relationships in " + steptimer.ElapsedMilliseconds + "ms");
            }


            using (ISession session = driver.Session())
            {
                steptimer.Restart();
                //create primary group mappings
                int propcounter = Writer.UpdateMemberCounts(session);
                steptimer.Stop();
                Console.WriteLine("Created " + propcounter + " group membership counts updated in " + steptimer.ElapsedMilliseconds + "ms");
            }

            Console.WriteLine();
            Console.WriteLine("Cleaning up deleted items");
            steptimer.Restart();
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
            steptimer.Stop();
            Console.WriteLine("Finished cleaning up deleted items in " + steptimer.ElapsedMilliseconds + "ms");
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
