using System;
using System.DirectoryServices;

namespace ADScanner.ActiveDirectory
{
    internal static class QueryHandler
    {
        public static SearchResultCollection GetAllGroupResults(DirectoryEntry de)
        {
            SearchResultCollection results = null;
            using (DirectorySearcher searcher = new DirectorySearcher(de))
            {
                try
                {
                    searcher.Filter = ("(&(objectCategory=group))");
                    searcher.SearchScope = SearchScope.Subtree;
                    searcher.PropertiesToLoad.Add("canonicalName");
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
                    Console.WriteLine("Found "+ results.Count +" groups");
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
                    searcher.Filter = ("(&(objectCategory=user))");
                    searcher.SearchScope = SearchScope.Subtree;
                    searcher.PropertiesToLoad.Add("canonicalName");
                    searcher.PropertiesToLoad.Add("cn");
                    searcher.PropertiesToLoad.Add("memberof");
                    searcher.PropertiesToLoad.Add("name");
                    searcher.PropertiesToLoad.Add("samaccountname");
                    searcher.PropertiesToLoad.Add("objectcategory");
                    searcher.PropertiesToLoad.Add("objectSid");
                    searcher.PropertiesToLoad.Add("distinguishedName");
                    searcher.PropertiesToLoad.Add("primaryGroupID");

                    searcher.PageSize = 1000;
                    Console.WriteLine("Searching for users");
                    results = searcher.FindAll();
                    Console.WriteLine("Found " + results.Count + " users");
                }
                catch (Exception e)
                {
                    Console.WriteLine("Error retrieving users from AD: " + e.Message);
                }
            }
            return results;
        }

        public static SearchResultCollection GetAllComputerResults(DirectoryEntry de)
        {
            SearchResultCollection results = null;
            using (DirectorySearcher searcher = new DirectorySearcher(de))
            {
                try
                {
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
                    Console.WriteLine("Found " + results.Count + " computers");
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
