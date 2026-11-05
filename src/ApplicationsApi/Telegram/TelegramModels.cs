namespace ApplicationsApi.Telegram;

public record TgResponse<T>(bool Ok, T? Result, string? Description);
public record TgUpdate(long UpdateId, TgMessage? Message);
public record TgMessage(long MessageId, TgUser? From, TgChat Chat, string? Text, TgContact? Contact);
public record TgUser(long Id, bool IsBot);
public record TgChat(long Id, string Type);
public record TgContact(string PhoneNumber, long? UserId);

public sealed class TelegramOptions(string token)
{
    public string Token { get; } = token;
}
