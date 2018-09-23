using System.DirectoryServices.AccountManagement;
using System;

namespace Console.Auth.Local
{
    public class LocalLogin : ILogin
    {
        public string ID { get; private set; }
        public string GivenName { get; private set; }
        public string Surname { get; private set; }
        public string Name { get; private set; }
        public bool IsUser { get; private set; } = false;
        public bool IsAdmin { get; private set; } = false;
        public bool IsAuthorised { get { return this.IsAdmin || this.IsUser; } }
        public bool IsAuthenticated { get; private set; } = false;
        public int TimeoutSeconds { get; private set; } = 900;

        public LocalLogin(LocalAuthConfiguration config, string username, string password)
        {
            this.TimeoutSeconds = config.TimeoutSeconds;

            using (var context = AccountAuthorizer.CreateLocalContext())
            {
                if (AccountAuthorizer.IsAuthenticated(context, username, password))
                {
                    this.IsAuthenticated = true;
                    using (UserPrincipal user = AccountAuthorizer.GetUserPrincipal(context, username))
                    {
                        this.GivenName = user.GivenName == null ? "" : user.GivenName;
                        this.Surname = user.Surname == null ? "" : user.Surname;
                        this.ID = user.Sid.Value;
                        this.Name = user.Name == null ? "Unknown" : user.Name;

                        foreach (string u in config.Users)
                        {
                            if (u.Equals(username,StringComparison.OrdinalIgnoreCase)) { this.IsUser = true; }
                        }

                        foreach (string u in config.AdminUsers)
                        {
                            if (u.Equals(username, StringComparison.OrdinalIgnoreCase)) { this.IsAdmin = true; }
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
