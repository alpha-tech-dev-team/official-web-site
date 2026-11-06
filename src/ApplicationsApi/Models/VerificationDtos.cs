namespace ApplicationsApi.Models;

public record StartVerificationRequest(Guid ApplicationId, string? Phone);

public record StartVerificationResponse(Guid SessionId, string Link, DateTime ExpiresAt);

public record VerificationStatusResponse(string Status, string? Reason, DateTime ExpiresAt);

public record PublicVerificationResponse(bool Verified);
