using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace manegment_device;

/// <summary>Authenticates a per-Windows-user account without requiring MySQL.</summary>
internal static class OfflineAccountStore
{
    private static readonly string AccountPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "ManagementDevice", "offline-account.json");

    public static LoginResult? Authenticate(string username, string password)
    {
        try
        {
            if (!File.Exists(AccountPath)) return null;
            var account = JsonSerializer.Deserialize<OfflineAccount>(File.ReadAllText(AccountPath));
            if (account is null || !string.Equals(account.Username, username, StringComparison.OrdinalIgnoreCase)) return null;

            var salt = Convert.FromBase64String(account.Salt);
            var expected = Convert.FromBase64String(account.PasswordHash);
            var actual = Rfc2898DeriveBytes.Pbkdf2(password, salt, account.Iterations, HashAlgorithmName.SHA256, expected.Length);
            var matches = CryptographicOperations.FixedTimeEquals(actual, expected);
            return new LoginResult(true, matches, account.Role);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or JsonException or FormatException or ArgumentException or CryptographicException)
        {
            // A damaged local account file should not prevent normal MySQL login.
            return null;
        }
    }

    private sealed class OfflineAccount
    {
        public OfflineAccount() { }
        public string Username { get; set; } = "";
        public string Salt { get; set; } = "";
        public string PasswordHash { get; set; } = "";
        public int Iterations { get; set; }
        public string Role { get; set; } = "member";
    }
}
