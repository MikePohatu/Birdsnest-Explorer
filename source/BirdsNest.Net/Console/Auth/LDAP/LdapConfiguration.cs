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
using System.Security;

namespace Console.Auth.LDAP
{
    public class LdapConfiguration : IAuthConfiguration
    {
        private SecureString bindpw; //bind pw might need to sit in memory for

        public string Domain { get; set; }
        public string Server { get; set; }
        public int Port { get; set; } = 389;
        public string Name { get; set; }
        public string AdminGroupDN { get; set; }
        public string UserGroupDN { get; set; }
        public string SearchBase { get; set; }
        public bool SSL { get; set; } = false;
        public int TimeoutSeconds { get; set; } = 900;
        public string BindUser { get; set; } //needs to be in UPN format e.g. user@domain.local
        public string BindPassword
        {
            get { return SecureStringConverter.ToString(this.bindpw); }
            set { this.bindpw = SecureStringConverter.ToSecureString(value); }
        }

        public ILogin GetLogin(string username, string password)
        {
            return new LdapLogin(this, username, password);
        }
    }
}
