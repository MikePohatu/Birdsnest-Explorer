using System;
using System.DirectoryServices;

namespace ADScanner.ActiveDirectory
{
    public static class QueryHandler
    {
        public static SearchResultCollection GetAllGroupResults(DirectoryEntry de)
        {
            SearchResultCollection results = null;
            using (DirectorySearcher searcher = new DirectorySearcher(de))
            {
                try
                {
                    DateTime start = DateTime.Now;
                    searcher.Filter = ("(&(objectCategory=group))");
                    searcher.SearchScope = SearchScope.Subtree;
                    searcher.PropertiesToLoad.Add("cn");
                    searcher.PropertiesToLoad.Add("memberof");
                    searcher.PropertiesToLoad.Add("name");
                    searcher.PropertiesToLoad.Add("samaccountname");
                    searcher.PropertiesToLoad.Add("grouptype");
                    searcher.PropertiesToLoad.Add("member");
                    searcher.PropertiesToLoad.Add("objectcategory");
                    searcher.PropertiesToLoad.Add("objectSid");
                    searcher.PropertiesToLoad.Add("distinguishedName");

                    searcher.PageSize = 1000;
                    Console.WriteLine("Searching for groups");
                    results = searcher.FindAll();
                    TimeSpan elapsed = DateTime.Now - start;
                    Console.WriteLine("Found " + results.Count + " groups in " + elapsed.Milliseconds + " milliseconds.");
                }
                catch (Exception e)
                {
                    Console.WriteLine("Error retrieving groups from AD: " + e.Message);
                }
            }
            return results;
        }

        public static SearchResultCollection GetAllUserResults(DirectoryEntry de)
        {
            SearchResultCollection results = null;
            using (DirectorySearcher searcher = new DirectorySearcher(de))
            {
                try
                {
                    DateTime start = DateTime.Now;
                    searcher.Filter = ("(&(objectCategory=user))");
                    searcher.SearchScope = SearchScope.Subtree;
                    searcher.PropertiesToLoad.Add("canonicalName");
                    searcher.PropertiesToLoad.Add("cn");
                    searcher.PropertiesToLoad.Add("memberof");
                    searcher.PropertiesToLoad.Add("name");
                    searcher.PropertiesToLoad.Add("samaccountname");
                    searcher.PropertiesToLoad.Add("objectcategory");
                    searcher.PropertiesToLoad.Add("objectSid");
                    searcher.PropertiesToLoad.Add("displayName");
                    searcher.PropertiesToLoad.Add("distinguishedName");
                    searcher.PropertiesToLoad.Add("primaryGroupID");
                    searcher.PropertiesToLoad.Add("userAccountControl");
                    
                    searcher.PageSize = 1000;
                    Console.WriteLine("Searching for users");
                    results = searcher.FindAll();
                    TimeSpan elapsed = DateTime.Now - start;
                    Console.WriteLine("Found " + results.Count + " users in " + elapsed.Milliseconds + " milliseconds.");
                }
                catch (Exception e)
                {
                    Console.WriteLine("Error retrieving users from AD: " + e.Message);
                }
            }
            return results;
        }

        //public static SearchResultCollection GetDeletedUserResults(DirectoryEntry de)
        //{
            
        //    SearchResultCollection results = null;
        //    using (DirectorySearcher searcher = new DirectorySearcher(de))
        //    {
        //        try
        //        {
        //            DateTime start = DateTime.Now;
        //            searcher.Filter = ("(&(isDeleted=TRUE)(userAccountControl:1.2.840.113556.1.4.417:=512))");
        //            searcher.SearchScope = SearchScope.Subtree;
        //            searcher.PropertiesToLoad.Add("cn");
        //            searcher.PropertiesToLoad.Add("name");
        //            searcher.PropertiesToLoad.Add("samaccountname");
        //            searcher.PropertiesToLoad.Add("lastKnownParent");
        //            searcher.PropertiesToLoad.Add("objectSid");
        //            searcher.PropertiesToLoad.Add("distinguishedName");
        //            searcher.PropertiesToLoad.Add("userAccountControl");

        //            searcher.PageSize = 1000;
        //            Console.WriteLine("Searching for deleted users");
        //            results = searcher.FindAll();
        //            TimeSpan elapsed = DateTime.Now - start;
        //            Console.WriteLine("Found " + results.Count + " deleted users in " + elapsed.Milliseconds + " milliseconds.");
        //        }
        //        catch (Exception e)
        //        {
        //            Console.WriteLine("Error retrieving deleted users from AD: " + e.Message);
        //        }
        //    }
        //    return results;
        //}

        public static SearchResultCollection GetAllComputerResults(DirectoryEntry de)
        {
            SearchResultCollection results = null;
            using (DirectorySearcher searcher = new DirectorySearcher(de))
            {
                try
                {
                    DateTime start = DateTime.Now;
                    searcher.Filter = ("(&(objectCategory=computer))");
                    searcher.SearchScope = SearchScope.Subtree;
                    searcher.PropertiesToLoad.Add("memberof");
                    searcher.PropertiesToLoad.Add("name");
                    searcher.PropertiesToLoad.Add("cn");
                    searcher.PropertiesToLoad.Add("samaccountname");
                    searcher.PropertiesToLoad.Add("objectcategory");
                    searcher.PropertiesToLoad.Add("objectSid");
                    searcher.PropertiesToLoad.Add("distinguishedName");
                    searcher.PropertiesToLoad.Add("operatingSystem");
                    searcher.PropertiesToLoad.Add("operatingSystemVersion");
                    searcher.PropertiesToLoad.Add("primaryGroupID");

                    searcher.PageSize = 1000;
                    Console.WriteLine("Searching for computers");
                    results = searcher.FindAll();
                    TimeSpan elapsed = DateTime.Now - start;
                    Console.WriteLine("Found " + results.Count + " computers in " + elapsed.Milliseconds + " milliseconds.");
                }
                catch (Exception e)
                {
                    Console.WriteLine("Error retrieving computers from AD: " + e.Message);
                }
            }
            return results;
        }
    }
}
