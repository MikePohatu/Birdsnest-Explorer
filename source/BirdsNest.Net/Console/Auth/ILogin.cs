namespace Console.Auth
{
    public interface ILogin
    {
        string ID { get; }
        string GivenName { get; }
        string Surname { get; }
        string Name { get; }
        bool IsUser { get; }
        bool IsAdmin { get; }
        bool IsAuthorised { get; }
        bool IsAuthenticated { get; }
        int TimeoutSeconds { get; }
    }
}
