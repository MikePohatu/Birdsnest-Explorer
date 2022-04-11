#region license
// Copyright (c) 2019-2020 "20Road"
// 20Road Limited [https://www.20road.com]
//
// This file is part of Birdsnest Explorer.
//
// Birdsnest Explorer is free software: you can redistribute it and/or modify
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
#endregion
using common;
using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;

namespace ADScanner.ActiveDirectory
{
    public class UsersCollector: IDataCollector
    {
        private PrincipalContext _context;

        public string ProgressMessage { get { return "Creating user nodes"; } }

        public string Query
        {
            get
            {
                return "UNWIND $Properties AS u" +
                    " MERGE (n:" + Types.ADObject + "{id:u.id, domainid: $ScannerID})" +
                    " SET n: " + Types.User +
                    " REMOVE n: " + Types.Deleted +
                    " SET n.info = u.info" +
                    " SET n.description = u.description" +
                    " SET n.name = u.name" +
                    " SET n.dn = u.dn" +
                    " SET n.path = u.path" +
                    " SET n.type = u.type" +
                    " SET n.samaccountname = u.samaccountname" +
                    " SET n.primarygroupid = u.primarygroupid" +
                    " SET n.displayname = u.displayname" +
                    " SET n.manager = u.manager" +
                    " SET n.enabled = u.enabled" +
                    " SET n.userprincipalname = u.userprincipalname" +
                    " SET n.type = '" + Types.User + "'" +
                    " SET n.scope = 1" +
                    " SET n.domainid = $ScannerID" +
                    " SET n.lastscan = $ScanID" +
                    " RETURN n.name";
            }
        }


        public UsersCollector(PrincipalContext context)
        {
            this._context = context;
        }

        public NeoQueryData CollectData()
        {
            NeoQueryData querydata = new NeoQueryData();
            List<object> propertylist = new List<object>();
            querydata.Properties = propertylist;

            using (PrincipalSearcher principalSearcher = new PrincipalSearcher(new UserPrincipal(this._context)))
            {
                try
                {
                    DirectorySearcher searcher = principalSearcher.GetUnderlyingSearcher() as DirectorySearcher;
                    if (searcher != null)
                    {
                        searcher.Filter = ("(&(objectCategory=person)(objectClass=user))");
                        searcher.SearchScope = SearchScope.Subtree;
                        searcher.PropertiesToLoad.Add("canonicalName");
                        searcher.PropertiesToLoad.Add("cn");
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
                        searcher.PropertiesToLoad.Add("manager");

                        searcher.PageSize = 1000;
                        SearchResultCollection results = searcher.FindAll();
                        //Console.WriteLine("Found " + results.Count + " users in " + timer.ElapsedMilliseconds + "ms.");

                        foreach (SearchResult result in results)
                        {
                            string id = ADSearchResultConverter.GetSidAsString(result);
                            string dn = ADSearchResultConverter.GetSinglestringValue(result, "distinguishedname");

                            //find if the user is enabled. use bitwise comparison
                            int istate = ADSearchResultConverter.GetIntSingleValue(result, "useraccountcontrol");
                            bool enabled = (((UserAccountControlDefinitions)istate & UserAccountControlDefinitions.ACCOUNTDISABLE) == UserAccountControlDefinitions.ACCOUNTDISABLE) ? false : true;

                            propertylist.Add(new
                            {
                                name = ADSearchResultConverter.GetSinglestringValue(result, "Name"),
                                dn,
                                description = ADSearchResultConverter.GetSinglestringValue(result, "Description"),
                                id,
                                info = ADSearchResultConverter.GetSinglestringValue(result, "info"),
                                samaccountname = ADSearchResultConverter.GetSinglestringValue(result, "samaccountname"),
                                path = dn,
                                displayname = ADSearchResultConverter.GetSinglestringValue(result, "displayname"),
                                userprincipalname = ADSearchResultConverter.GetSinglestringValue(result, "userPrincipalName"),
                                enabled,
                                primarygroupid = ADSearchResultConverter.GetSinglestringValue(result, "primaryGroupID"),
                                manager = ADSearchResultConverter.GetSinglestringValue(result, "manager")
                            });
                        }
                    }
                    else
                    {
                        Program.ExitError("Error retrieving users from AD", ErrorCodes.UsersCollectorSearcherNull);
                    }
                }
                catch (Exception e)
                {
                    //timer.Stop();
                    Program.ExitError(e, "Error retrieving users from AD", ErrorCodes.UsersCollectorException);
                }
            }
            return querydata;
        }
    }
}
