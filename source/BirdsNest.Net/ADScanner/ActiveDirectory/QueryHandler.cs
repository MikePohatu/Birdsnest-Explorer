#region license
// Copyright (c) 2019-2020 "20Road"
// 20Road Limited [https://20road.com]
//
// This file is part of BirdsNest.
//
// BirdsNest is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, version 3 of the License.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.
//
#endregion
using System;
using System.DirectoryServices;
using System.Diagnostics;

namespace ADScanner.ActiveDirectory
{
    public static class QueryHandler
    {
        public static SearchResultCollection GetAllGroupResults(DirectoryEntry de)
        {
            Stopwatch timer = new Stopwatch();
            SearchResultCollection results = null;
            using (DirectorySearcher searcher = new DirectorySearcher(de))
            {
                try
                {
                    timer.Start();
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
                    searcher.PropertiesToLoad.Add("description");
                    searcher.PropertiesToLoad.Add("info");

                    searcher.PageSize = 1000;
                    Console.WriteLine("Searching for groups");
                    results = searcher.FindAll();
                    timer.Stop();
                    Console.WriteLine("Found " + results.Count + " groups in " + timer.ElapsedMilliseconds + "ms.");
                }
                catch (Exception e)
                {
                    timer.Stop();
                    Console.WriteLine("Error retrieving groups from AD: " + e.Message);
                }
            }
            return results;
        }

        public static SearchResultCollection GetAllUserResults(DirectoryEntry de)
        {
            Stopwatch timer = new Stopwatch();
            SearchResultCollection results = null;
            using (DirectorySearcher searcher = new DirectorySearcher(de))
            {
                try
                {
                    timer.Start();
                    searcher.Filter = ("(&(objectCategory=person)(objectClass=user))");
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
                    searcher.PropertiesToLoad.Add("description");
                    searcher.PropertiesToLoad.Add("userPrincipalName");
                    searcher.PropertiesToLoad.Add("info");

                    searcher.PageSize = 1000;
                    Console.WriteLine("Searching for users");
                    results = searcher.FindAll();
                    timer.Stop();
                    Console.WriteLine("Found " + results.Count + " users in " + timer.ElapsedMilliseconds + "ms.");
                }
                catch (Exception e)
                {
                    timer.Stop();
                    Console.WriteLine("Error retrieving users from AD: " + e.Message);
                }
            }
            return results;
        }

        //public static SearchResultCollection GetDeletedUserResults(DirectoryEntry de)
        //{
        //    Stopwatch timer = new Stopwatch();
        //    SearchResultCollection results = null;
        //    using (DirectorySearcher searcher = new DirectorySearcher(de))
        //    {
        //        try
        //        {
        //            timer.Start();
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
        //            timer.Stop();
        //            Console.WriteLine("Found " + results.Count + " deleted users in " + timer.ElapsedMilliseconds + "ms.");
        //        }
        //        catch (Exception e)
        //        {
        //            timer.Stop();
        //            Console.WriteLine("Error retrieving deleted users from AD: " + e.Message);
        //        }
        //    }
        //    return results;
        //}

        public static SearchResultCollection GetAllComputerResults(DirectoryEntry de)
        {
            Stopwatch timer = new Stopwatch();
            SearchResultCollection results = null;
            using (DirectorySearcher searcher = new DirectorySearcher(de))
            {
                try
                {
                    timer.Start();
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
                    searcher.PropertiesToLoad.Add("description");
                    searcher.PropertiesToLoad.Add("info");

                    searcher.PageSize = 1000;
                    Console.WriteLine("Searching for computers");
                    results = searcher.FindAll();
                    timer.Stop();
                    Console.WriteLine("Found " + results.Count + " computers in " + timer.ElapsedMilliseconds + "ms.");
                }
                catch (Exception e)
                {
                    timer.Stop();
                    Console.WriteLine("Error retrieving computers from AD: " + e.Message);
                }
            }
            return results;
        }
    }
}
