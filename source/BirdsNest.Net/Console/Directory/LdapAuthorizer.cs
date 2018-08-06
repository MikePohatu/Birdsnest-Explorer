using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.DirectoryServices.AccountManagement;

namespace Console.Directory
{
    public static class LdapAuthorizer
    {
        public static PrincipalContext CreateContext(string domainname)
        {
            PrincipalContext context = new PrincipalContext(ContextType.Domain, domainname);
            return context;
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
            // find the group in question
            GroupPrincipal group = GroupPrincipal.FindByIdentity(context, groupname);

            if (user != null && user.IsMemberOf(group))
            {
                return true;
            }
            return false;
        }
    }
}
