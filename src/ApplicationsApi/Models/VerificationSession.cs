namespace ApplicationsApi.Models;

public enum VerificationStatus
{
    Pending,
    Verified,
    Failed,
    Expired
}

public class VerificationSession
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public Guid ApplicationId { get; init; }
    public string TokenHash { get; init; } = "";
    public string Phone { get; init; } = "";
    public long? TelegramChatId { get; set; }
    public VerificationStatus Status { get; set; } = VerificationStatus.Pending;
    public string? Reason { get; set; }
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
    public DateTime ExpiresAt { get; init; }
}
