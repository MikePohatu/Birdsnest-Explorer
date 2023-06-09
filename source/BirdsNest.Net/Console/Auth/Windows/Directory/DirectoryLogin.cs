﻿#region license
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
using System.DirectoryServices.AccountManagement;

#pragma warning disable CA1416
namespace Console.Auth.Windows.Directory
{
    public class DirectoryLogin : ILogin
    {
        public string ID { get; private set; } = string.Empty;
        public string GivenName { get; private set; } = string.Empty;
        public string Surname { get; private set; } = string.Empty;
        public string Name { get; private set; } = string.Empty;
        public bool IsUser { get; private set; } = false;
        public bool IsAdmin { get; private set; } = false;
        public bool IsAuthorised { get { return this.IsAdmin || this.IsUser; } }
        public bool IsAuthenticated { get; private set; } = false;
        public int TimeoutSeconds { get; private set; } = 900;

        public DirectoryLogin(DirectoryConfiguration config, string username, string password)
        {
            this.TimeoutSeconds = config.TimeoutSeconds;

            using (var context = AccountAuthorizer.CreateLdapContext(config.Domain, config.ContainerDN, config.SSL))
            {
                if (AccountAuthorizer.IsAuthenticated(context, username, password))
                {
                    this.IsAuthenticated = true;
                    using (UserPrincipal user = AccountAuthorizer.GetUserPrincipal(context, username))
                    {
                        this.GivenName = user.GivenName != null ? user.GivenName : string.Empty;
                        this.Surname = user.Surname != null ? user.Surname : string.Empty;
                        this.ID = user.Sid.Value;
                        this.Name = user.UserPrincipalName;

                        if (AccountAuthorizer.IsMemberOf(context, user, config.UserGroup))
                        {
                            this.IsUser = true;
                        }

                        if (AccountAuthorizer.IsMemberOf(context, user, config.AdminGroup))
                        {
                            this.IsAdmin = true;
                        }
                    }
                }
                else
                {
                    this.IsAuthenticated = false;
                }
            }
        }

    }
}
