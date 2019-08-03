using System.DirectoryServices.AccountManagement;

namespace Console.Auth.Directory
{
    public class DirectoryLogin: ILogin
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
                        this.Surname = user.Surname != null? user.Surname : string.Empty;
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
