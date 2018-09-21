namespace Console.Auth.Directory
{
    public class DirectoryConfiguration: IAuthConfiguration
    {
        public string Domain { get; set; }
        public string Name { get; set; }
        public string AdminGroup { get; set; }
        public string UserGroup { get; set; }
        public string ContainerDN { get; set; }
        public bool SSL { get; set; } = false;
        public int TimeoutSeconds { get; set; } = 900;

        public ILogin GetLogin(string username, string password)
        {
            return new DirectoryLogin(this, username, password);
        }
    }
}
