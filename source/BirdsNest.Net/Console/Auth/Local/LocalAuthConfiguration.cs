using System.Collections.Generic;

namespace Console.Auth.Local
{
    public class LocalAuthConfiguration : IAuthConfiguration
    {
        public string Name { get; set; }
        public List<string> AdminUsers { get; set; } = new List<string>();
        public List<string> Users { get; set; } = new List<string>();
        public int TimeoutSeconds { get; set; } = 900;

        public ILogin GetLogin(string username, string password)
        {
            return new LocalLogin(this, username, password);
        }
    }
}
