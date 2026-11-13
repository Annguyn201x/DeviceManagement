namespace manegment_device;

public interface ILogin_query
{
    public bool checking_user(string user_name, string password);
    public string GetUserRole(string user_name);
    public LoginResult Authenticate(string user_name, string password);
}

public sealed record LoginResult(bool UserExists, bool IsAuthenticated, string Role);
