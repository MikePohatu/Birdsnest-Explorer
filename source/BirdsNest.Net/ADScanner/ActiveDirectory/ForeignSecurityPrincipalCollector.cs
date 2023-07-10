#region license
// Copyright (c) 2019-2023 "20Road"
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
    class ForeignSecurityPrincipalCollector : IDataCollector
    {
        private PrincipalContext _context;

        public string ProgressMessage { get { return "Creating foreign security principal nodes"; } }

        public string Query
        {
            get
            {
                return "UNWIND $Properties AS prop" +
                " MERGE (n:" + Types.ADObject + " {id:prop.id, domainid: $ScannerID})" +
                " SET n:" + Types.ForeignSecurityPrincipal +
                " REMOVE n:" + Types.Deleted +
                " SET n.name = prop.name" +
                " SET n.lastscan=$ScanID" +
                " SET n.scope = 1" +
                " SET n.dn = prop.dn" +
                " SET n.type = '" + Types.ForeignSecurityPrincipal + "'" +
                " RETURN n.name";
            }
        }


        public ForeignSecurityPrincipalCollector(PrincipalContext context)
        {
            this._context = context;
        }

        public NeoQueryData CollectData()
        {
            NeoQueryData querydata = new NeoQueryData();
            List<object> propertylist = new List<object>();


            try
            {
                using (PrincipalSearcher principalSearcher = new PrincipalSearcher(new UserPrincipal(this._context)))
                {
                    DirectorySearcher searcher = principalSearcher.GetUnderlyingSearcher() as DirectorySearcher;
                    if (searcher != null)
                    {
                        searcher.Filter = ("(&(objectCategory=foreignSecurityPrincipal))");
                        searcher.SearchScope = SearchScope.Subtree;
                        searcher.PropertiesToLoad.Add("cn");
                        searcher.PropertiesToLoad.Add("name");
                        searcher.PropertiesToLoad.Add("objectcategory");
                        searcher.PropertiesToLoad.Add("objectSid");
                        searcher.PropertiesToLoad.Add("distinguishedName");
                        searcher.PropertiesToLoad.Add("description");

                        searcher.PageSize = 1000;
                        SearchResultCollection results = searcher.FindAll();

                        foreach (SearchResult result in results)
                        {
                            propertylist.Add(new
                            {
                                name = ADSearchResultConverter.GetSinglestringValue(result, "Name"),
                                dn = ADSearchResultConverter.GetSinglestringValue(result, "distinguishedname"),
                                description = ADSearchResultConverter.GetSinglestringValue(result, "Description"),
                                id = ADSearchResultConverter.GetSidAsString(result)
                            });
                        }
                    }
                    else
                    {
                        Program.ExitError("Error retrieving foreign security principals from AD", ErrorCodes.ForeignSecurityPrincipalCollectorSearcherNull);
                    }
                }
            }
            catch (Exception e)
            {
                Program.ExitError(e, "Error retrieving foreign security principals from AD", ErrorCodes.ForeignSecurityPrincipalCollectorSearcherException);
            }

            querydata.Properties = propertylist;
            return querydata;
        }
    }
}
