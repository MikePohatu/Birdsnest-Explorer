namespace Console.Auth
{
    public interface IAuthConfiguration
    {
        string Name { get; }
        ILogin GetLogin(string username, string password);
    }
}
