using System.Security.Cryptography;
using System.Text;

namespace ApplicationsApi.Auth;

public class AdminStore
{
    private const int Iterations = 600_000;

    private readonly string _login;
    private readonly byte[] _salt;
    private readonly byte[] _hash;

    public AdminStore(string login, string password)
    {
        _login = login;
        _salt = RandomNumberGenerator.GetBytes(16);
        _hash = Hash(password, _salt);
    }

    public bool Verify(string? login, string? password)
    {
        
        var candidate = Hash(password ?? "", _salt);
        var passwordOk = CryptographicOperations.FixedTimeEquals(candidate, _hash);
        var loginOk = CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(login ?? ""), Encoding.UTF8.GetBytes(_login));

        return passwordOk && loginOk;
    }

    private static byte[] Hash(string password, byte[] salt) =>
        Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, HashAlgorithmName.SHA256, 32);
}