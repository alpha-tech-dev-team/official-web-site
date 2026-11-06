using System.Security.Cryptography;
using System.Text;

namespace ApplicationsApi.Services;

public static class CodeGenerator
{
    public const int PublicCodeLength = 16;
    public const string PublicCodeAlphabet = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";

    public static string NewToken() =>
        Convert.ToBase64String(RandomNumberGenerator.GetBytes(24))
            .Replace('+', '-')
            .Replace('/', '_')
            .TrimEnd('=');

    public static string HashToken(string token) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token)));

    public static string NewPublicCode()
    {
        var bytes = RandomNumberGenerator.GetBytes(PublicCodeLength);

        return new string(bytes.Select(b => PublicCodeAlphabet[b % PublicCodeAlphabet.Length]).ToArray());
    }
}
