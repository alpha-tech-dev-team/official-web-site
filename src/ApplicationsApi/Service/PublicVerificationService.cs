using ApplicationsApi.Models;

namespace ApplicationsApi.Services;

public class PublicVerificationService(
    IApplicationRepository applications,
    IPhoneVerificationRepository phoneVerifications)
{
    public async Task<bool> IsVerifiedAsync(string? publicCode)
    {
        var code = Normalize(publicCode);
        if (code is null) return false;

        var application = await applications.GetByPublicCodeAsync(code);
        if (application is null) return false;

        if (application.Status is not (ApplicationStatus.Submitted or ApplicationStatus.Approved))
            return false;

        var confirmations = await phoneVerifications.ListByApplicationAsync(application.Id);

        return confirmations.Any(v =>
            v.RevokedAt is null &&
            string.Equals(v.Phone, application.Phone, StringComparison.Ordinal));
    }

    private static string? Normalize(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw)) return null;

        var code = raw.Trim().ToUpperInvariant();
        if (code.Length != CodeGenerator.PublicCodeLength) return null;

        return code.All(c => CodeGenerator.PublicCodeAlphabet.Contains(c)) ? code : null;
    }
}
