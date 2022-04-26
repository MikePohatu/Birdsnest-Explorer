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
using Novell.Directory.Ldap;
using System;

namespace Console.Auth.LDAP
{
    public class LdapLogin : ILogin
    {
        public string CN { get; private set; } = string.Empty;
        public string ID { get; private set; } = string.Empty;
        public string GivenName { get; private set; } = string.Empty;
        public string Surname { get; private set; } = string.Empty;
        public string Name { get; private set; } = string.Empty;
        public bool IsUser { get; private set; } = false;
        public bool IsAdmin { get; private set; } = false;
        public bool IsAuthorised { get { return this.IsAdmin || this.IsUser; } }
        public bool IsAuthenticated { get; private set; } = false;
        public int TimeoutSeconds { get; private set; } = 900;

        public LdapLogin(LdapConfiguration config, string username, string password)
        {
            this.TimeoutSeconds = config.TimeoutSeconds;

            using (var cn = new LdapConnection())
            {
                // connect
                try
                {
                    string server = string.IsNullOrWhiteSpace(config.Server) ? config.Domain : config.Server;

                    cn.Connect(server, config.Port);
                    // bind with an username and password
                    // this how you can verify the password of an user
                    cn.Bind(config.BindUser, config.BindPassword);

                    string searchBase = config.SearchBase;
                    string searchFilter = string.Empty;
                    if (username.Contains("@"))
                    {
                        searchFilter = $"(userPrincipalName=" + username + ")";
                    }
                    else
                    {
                        searchFilter = $"(samaccountname=" + username + ")";
                    }

                    string[] attrs = new string[] { "cn", "userPrincipalName", "givenname", "samaccountname",
                        "displayname", "givenName", "sn", "objectSid", "memberOf" };

                    try
                    {
                        ILdapSearchResults results = cn.Search(config.SearchBase, LdapConnection.ScopeSub,
                            searchFilter, attrs, false);
                        string[] groups = null;

                        while (results.HasMore())
                        {
                            LdapEntry nextEntry = null;
                            try
                            {
                                nextEntry = results.Next();
                            }
                            catch
                            {
                                continue;
                            }

                            // Get the attribute set of the entry
                            LdapAttributeSet attributeSet = nextEntry.GetAttributeSet();

                            this.CN = attributeSet.GetAttribute("cn")?.StringValue;
                            this.ID = attributeSet.GetAttribute("objectSid")?.StringValue;
                            this.GivenName = attributeSet.GetAttribute("givenname")?.StringValue;
                            this.Surname = attributeSet.GetAttribute("sn")?.StringValue;
                            this.Name = attributeSet.GetAttribute("displayname")?.StringValue;
                            groups = attributeSet.GetAttribute("memberOf")?.StringValueArray;

                            if (groups != null)
                            {
                                foreach (string group in groups)
                                {
                                    if (group.Equals(config.AdminGroupDN, StringComparison.OrdinalIgnoreCase))
                                    {
                                        this.IsAdmin = true;
                                    }
                                    if (group.Equals(config.UserGroupDN, StringComparison.OrdinalIgnoreCase))
                                    {
                                        this.IsUser = true;
                                    }
                                }

                            }
                        }

                        cn.Bind(this.CN, password);

                        this.IsAuthenticated = true;
                        cn.Disconnect();
                    }
                    catch
                    {
                        this.IsAuthenticated = false;
                        return;
                    }
                }
                catch
                {
                    this.IsAuthenticated = false;
                }
            }
        }

    }
}
