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
using System.DirectoryServices.AccountManagement;

namespace Console.Auth.Windows
{
    public static class AccountAuthorizer
    {
        public static PrincipalContext CreateLdapContext(string domainname, string containerdn, bool ssl)
        {
            PrincipalContext context;
            if (ssl == true)
            {
                ContextOptions options = ContextOptions.Negotiate | ContextOptions.SecureSocketLayer;
                context = new PrincipalContext(ContextType.Domain,domainname,containerdn, options);
            }
            else
            {
                context = new PrincipalContext(ContextType.Domain, domainname, containerdn);
            }
            
            return context;
        }

        public static PrincipalContext CreateLocalContext()
        {
            return new PrincipalContext(ContextType.Machine);
        }

        public static bool IsAuthenticated(PrincipalContext context, string username, string password)
        {
            return context.ValidateCredentials(username, password);
        }

        public static UserPrincipal GetUserPrincipal (PrincipalContext context, string username)
        {
            return UserPrincipal.FindByIdentity(context, username);
        }


        public static bool IsMemberOf(PrincipalContext context, UserPrincipal user, string groupname)
        {
            try
            {
                using (GroupPrincipal group = GroupPrincipal.FindByIdentity(context, groupname))
                {
                    //looks like there is a bug in .Net where LDAPS context doesn't get passed through to all 
                    //calls. Seems to be in ADStoreCtx.IsMemberOfInStore but not 100% sure. Falling back to non 
                    //recursive iteration
                    foreach (Principal p in group.Members)
                    {
                        if (p.Equals(user)) { return true; }
                    }
                }
            }
            catch { }

            return false;
        }
    }
}
