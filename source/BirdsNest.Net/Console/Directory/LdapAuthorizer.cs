using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Novell.Directory.Ldap;

namespace Console.Directory
{
    public class LdapAuthorizer
    {
        public bool IsAuthenticated(string domainname, string username, string password, bool ssl)
        {
            string userDn = $"{username}@{domainname}";
            try
            {
                using (var connection = new LdapConnection { SecureSocketLayer = ssl })
                {
                    connection.Connect(domainname, ssl? LdapConnection.DEFAULT_SSL_PORT : LdapConnection.DEFAULT_PORT);
                    connection.Bind(userDn, password);
                    if (connection.Bound)
                        return true;
                }
            }
            catch (LdapException ex)
            {
                // Log exception
            }
            return false;
        }
    }
}
