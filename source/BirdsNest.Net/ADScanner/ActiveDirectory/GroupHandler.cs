using System;
using System.Data;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;

namespace ADScanner.ActiveDirectory
{
    internal static class GroupHandler
    {
        public static SearchResultCollection GetAllGroups(DirectoryEntry de)
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
                    //searcher.SizeLimit = 10000;
                    searcher.PageSize = 1000;
                    results = searcher.FindAll();
                }
                catch (Exception e)
                {
                    Console.WriteLine("Error retrieving groups from AD: " + e.Message);
                }
            }
            return results;
        }       
    }
}
