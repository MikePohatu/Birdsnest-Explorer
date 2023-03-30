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
    public class ComputersCollector : IDataCollector
    {
        private PrincipalContext _context;

        public string ProgressMessage { get { return "Creating computer nodes"; } }

        public string Query
        {
            get
            {
                return "UNWIND $Properties AS c" +
                    " MERGE (n:" + Types.ADObject + "{id:c.id, domainid: $ScannerID})" +
                    " SET n:" + Types.Device +
                    " SET n:" + Types.Computer +
                    " REMOVE n:" + Types.Deleted +
                    " SET n.info = c.info" +
                    " SET n.description = c.description" +
                    " SET n.name = c.name" +
                    " SET n.path = c.path" +
                    " SET n.dn = c.dn" +
                    " SET n.samaccountname = c.samaccountname" +
                    " SET n.primarygroupid = c.primarygroupid" +
                    " SET n.operatingsystem = c.operatingsystem" +
                    " SET n.operatingsystemversion = c.operatingsystemversion" +
                    " SET n.enabled = c.enabled" +
                    " SET n.type = '" + Types.Computer + "'" +
                    " SET n.lastscan = $ScanID" +
                    " RETURN n.name";
            }
        }


        public ComputersCollector(PrincipalContext context)
        {
            this._context = context;
        }

        public NeoQueryData CollectData()
        {
            NeoQueryData querydata = new NeoQueryData();
            List<object> propertylist = new List<object>();
            querydata.Properties = propertylist;

            try
            {
                using (PrincipalSearcher principalSearcher = new PrincipalSearcher(new UserPrincipal(this._context)))
                {
                    DirectorySearcher searcher = principalSearcher.GetUnderlyingSearcher() as DirectorySearcher;
                    if (searcher != null)
                    {
                        searcher.Filter = ("(&(objectCategory=computer))");
                        searcher.SearchScope = SearchScope.Subtree;
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
                        SearchResultCollection results = searcher.FindAll();


                        foreach (SearchResult result in results)
                        {
                            string id = ADSearchResultConverter.GetSidAsString(result);
                            string dn = ADSearchResultConverter.GetSinglestringValue(result, "distinguishedname");

                            //find if the computer is enabled. use bitwise comparison
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
                                operatingsystem = ADSearchResultConverter.GetSinglestringValue(result, "operatingsystem"),
                                operatingsystemversion = ADSearchResultConverter.GetSinglestringValue(result, "operatingsystemversion"),
                                enabled,
                                primarygroupid = ADSearchResultConverter.GetSinglestringValue(result, "primaryGroupID")
                            });
                        }
                    }
                    else
                    {
                        Program.ExitError("Error retrieving computers from AD", ErrorCodes.ComputersCollectorSearcherNull);
                    }


                }
            }
            catch (Exception e)
            {
                //timer.Stop();
                Program.ExitError(e, "Error retrieving computers from AD", ErrorCodes.ComputersCollectorSearcherException);
            }
            return querydata;
        }
    }
}
