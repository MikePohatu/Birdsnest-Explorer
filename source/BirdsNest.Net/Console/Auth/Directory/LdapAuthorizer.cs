using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.DirectoryServices.AccountManagement;

namespace Console.Auth.Directory
{
    public static class LdapAuthorizer
    {
        public static PrincipalContext CreateContext(string domainname, string containerdn, bool ssl)
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
                    foreach (Principal p in group.Members)
                    {
                        if (p.Equals(user)) { return true; }
                    }
                }
                //looks like there is a bug in .Net where LDAPS context doesn't get passed through to all 
                //calls. Seems to be in ADStoreCtx.IsMemberOfInStore but not 100% sure. Falling back to non 
                //recursive iteration
            }
            catch { }

            return false;
        }
    }
}
