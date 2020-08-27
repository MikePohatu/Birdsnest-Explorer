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
    public class GroupsCollector: IDataCollector
    {
        const string SCOPE_GLOBAL = "global";
        const string SCOPE_UNIVERSAL = "universal";
        const string SCOPE_DOMAIN_LOCAL = "domainlocal";
        const string TYPE_SECURITY = "security";
        const string TYPE_DISTRIBUTION = "distribution";

        private PrincipalContext _context;

        public List<object> GroupMemberships { get; } = new List<object>();

        public string ProgressMessage { get { return "Creating group nodes"; } }

        public string Query
        {
            get
            {
                return "UNWIND $Properties AS g" +
                " MERGE (n:" + Types.ADObject + " {id:g.id, domainid: $ScannerID})" +
                " SET n:" + Types.Group +
                " REMOVE n:" + Types.Deleted +
                " SET n.description = g.description" +
                " SET n.name = g.name" +
                " SET n.type = g.type" +
                " SET n.dn = g.dn" +
                " SET n.info = g.info" +
                " SET n.samaccountname = g.samaccountname" +
                " SET n.path = g.path" +
                " SET n.rid = g.rid" +
                " SET n.scope = 0" +
                " SET n.grouptype = g.grouptype" +
                " SET n.membercount = g.membercount" +
                " SET n.type ='" + Types.Group +"'" +
                " SET n.layout = 'mesh'" +
                " SET n.lastscan = $ScanID" +
                " RETURN n.name";
            }
        }


        public GroupsCollector(PrincipalContext context)
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
                    try
                    {
                        DirectorySearcher searcher = principalSearcher.GetUnderlyingSearcher() as DirectorySearcher;
                        if (searcher != null)
                        {
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
                            SearchResultCollection results = searcher.FindAll();

                            foreach (SearchResult result in results)
                            {
                                string id = ADSearchResultConverter.GetSidAsString(result);
                                string dn = ADSearchResultConverter.GetSinglestringValue(result, "distinguishedname");
                                var members = ADSearchResultConverter.GetStringList(result, "member");

                                propertylist.Add(new
                                {
                                    name = ADSearchResultConverter.GetSinglestringValue(result, "Name"),
                                    dn,
                                    description = ADSearchResultConverter.GetSinglestringValue(result, "Description"),
                                    id,
                                    info = ADSearchResultConverter.GetSinglestringValue(result, "info"),
                                    grouptype = GetTypeAndScope(ADSearchResultConverter.GetSinglestringValue(result, "grouptype")),
                                    samaccountname = ADSearchResultConverter.GetSinglestringValue(result, "samaccountname"),
                                    members,
                                    rid = ADSearchResultConverter.GetRidFromSid(id),
                                    path = dn,
                                    membercount = members.Count
                                });

                                foreach (string memberdn in members)
                                {
                                    this.GroupMemberships.Add(new { id, memberdn });
                                }
                            }
                        }
                        else
                        {
                            Program.ExitError("Error retrieving groups from AD", ErrorCodes.GroupsCollectorSearcherNull);
                        }
                    }
                    catch (Exception e)
                    {
                        Program.ExitError(e, "Error retrieving groups from AD", ErrorCodes.GroupsCollectorException);
                    }
                }
            }
            catch { }

            querydata.Properties = propertylist;
            return querydata;
        }

        public GroupMembershipsCollector GetMembershipsCollector()
        {
            return new GroupMembershipsCollector(this.GroupMemberships);
        }

        private string GetTypeAndScope(string grouptype)
        {
            switch (grouptype)
            {
                case "-2147483646":
                    return TYPE_SECURITY + "_" + SCOPE_GLOBAL;
                case "-2147483644":
                    return TYPE_SECURITY + "_" + SCOPE_DOMAIN_LOCAL;
                case "-2147483643":
                    return TYPE_SECURITY + "_" + SCOPE_DOMAIN_LOCAL;
                case "-2147483645":
                    return TYPE_SECURITY + "_" + SCOPE_DOMAIN_LOCAL;
                case "-2147483640":
                    return TYPE_SECURITY + "_" + SCOPE_UNIVERSAL;
                case "2":
                    return TYPE_DISTRIBUTION + "_" + SCOPE_GLOBAL;
                case "4":
                    return TYPE_DISTRIBUTION + "_" + SCOPE_DOMAIN_LOCAL;
                case "8":
                    return TYPE_DISTRIBUTION + "_" + SCOPE_UNIVERSAL;
                default:
                    return string.Empty;
            }
        }
    }
}
