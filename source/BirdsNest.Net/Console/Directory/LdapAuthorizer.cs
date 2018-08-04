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
            try
            {
                PrincipalContext context = new PrincipalContext(ContextType.Domain, domainname);
                return context;
            }
            catch (Exception e)
            {
                return null;
            }
        }

        public static bool IsAuthenticated(PrincipalContext context, string username, string password)
        {
            try
            {
                return context.ValidateCredentials(username, password);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static UserPrincipal GetUserPrincipal (PrincipalContext context, string username)
        {
            try
            {
                // find a user
                return UserPrincipal.FindByIdentity(context, username);
            }
            catch (Exception e)
            {
                throw e;
            }
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
