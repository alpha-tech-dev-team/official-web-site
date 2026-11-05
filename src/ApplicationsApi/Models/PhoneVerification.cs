namespace ApplicationsApi.Models;

public class PhoneVerification
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public Guid ApplicationId { get; init; }
    public string Phone { get; init; } = "";
    public long TelegramUserId { get; init; }
    public string Method { get; init; } = "telegram_bot";
    public DateTime VerifiedAt { get; init; } = DateTime.UtcNow;
    public DateTime? RevokedAt { get; set; }
}
