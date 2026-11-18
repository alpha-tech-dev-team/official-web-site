using System.Security.Cryptography;

namespace ApplicationsApi.Services;

public static class CodeGenerator
{
    public static string NewPublicCode() =>
        Convert.ToBase64String(RandomNumberGenerator.GetBytes(16))
            .TrimEnd('=').Replace('+', '-').Replace('/', '_');
}