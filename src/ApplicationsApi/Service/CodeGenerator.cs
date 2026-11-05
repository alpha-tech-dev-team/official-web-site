using System.Security.Cryptography;
using System.Text;

namespace ApplicationsApi.Services;

public static class CodeGenerator
{
    public static string NewToken() =>
        Convert.ToBase64String(RandomNumberGenerator.GetBytes(24))
            .Replace('+', '-')
            .Replace('/', '_')
            .TrimEnd('=');

    public static string HashToken(string token) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token)));

    public static string NewPublicCode()
    {
        const string alphabet = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
        var bytes = RandomNumberGenerator.GetBytes(8);

        return new string(bytes.Select(b => alphabet[b % alphabet.Length]).ToArray());
    }
}
