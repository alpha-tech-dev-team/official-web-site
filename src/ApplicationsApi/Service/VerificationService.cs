using ApplicationsApi.Models;

namespace ApplicationsApi.Services;

public enum StartOutcome
{
    Started,
    InvalidPhone,
    ApplicationNotFound,
    ApplicationNotAllowed,
    PhoneMismatch
}

public enum ContactOutcome
{
    Verified,
    NoActiveSession,
    NotOwnContact,
    InvalidPhone,
    PhoneMismatch,
    ApplicationChanged
}

public record StartedVerification(Guid SessionId, string Token, DateTime ExpiresAt);

public record StartResult(StartOutcome Outcome, StartedVerification? Verification = null);

public class VerificationService(
    IApplicationRepository applications,
    IVerificationSessionRepository sessions,
    IPhoneVerificationRepository phoneVerifications)
{
    public static readonly TimeSpan SessionLifetime = TimeSpan.FromMinutes(10);

    public async Task<StartResult> StartAsync(Guid applicationId, string? rawPhone)
    {
        var phone = Phone.Normalize(rawPhone);
        if (phone is null)
            return new(StartOutcome.InvalidPhone);

        var application = await applications.GetAsync(applicationId);
        if (application is null)
            return new(StartOutcome.ApplicationNotFound);
        if (application.Status is ApplicationStatus.Cancelled or ApplicationStatus.Rejected)
            return new(StartOutcome.ApplicationNotAllowed);

        if (application.Phone != phone)
            return new(StartOutcome.PhoneMismatch);

        await sessions.ExpirePendingAsync(applicationId);

        var token = CodeGenerator.NewToken();
        var session = new VerificationSession
        {
            ApplicationId = applicationId,
            TokenHash = CodeGenerator.HashToken(token),
            Phone = phone,
            ExpiresAt = DateTime.UtcNow + SessionLifetime
        };
        await sessions.AddAsync(session);

        return new(StartOutcome.Started, new StartedVerification(session.Id, token, session.ExpiresAt));
    }

    public async Task<bool> AttachChatAsync(string token, long chatId)
    {
        var session = await sessions.GetByTokenHashAsync(CodeGenerator.HashToken(token));
        if (session is null || !IsActive(session))
            return false;

        if (session.TelegramChatId is not null && session.TelegramChatId != chatId)
            return false;

        session.TelegramChatId = chatId;
        await sessions.UpdateAsync(session);
        return true;
    }

    public async Task<ContactOutcome> CompleteWithContactAsync(
        long chatId, long fromUserId, long? contactUserId, string? contactPhone)
    {
        var session = await sessions.GetActiveByChatAsync(chatId);
        if (session is null)
            return ContactOutcome.NoActiveSession;

        if (contactUserId != fromUserId)
            return ContactOutcome.NotOwnContact;

        var phone = Phone.Normalize(contactPhone);
        if (phone is null)
            return await FailAsync(session, "Принимаются только белорусские мобильные номера",
                ContactOutcome.InvalidPhone);

        if (phone != session.Phone)
            return await FailAsync(session, "Номер в Telegram не совпал с номером в заявке",
                ContactOutcome.PhoneMismatch);

        var application = await applications.GetAsync(session.ApplicationId);
        if (application is null || application.Phone != session.Phone)
            return await FailAsync(session, "Номер в заявке изменился", ContactOutcome.ApplicationChanged);

        session.Status = VerificationStatus.Verified;
        await sessions.UpdateAsync(session);

        await phoneVerifications.AddAsync(new PhoneVerification
        {
            ApplicationId = session.ApplicationId,
            Phone = session.Phone,
            TelegramUserId = fromUserId
        });

        return ContactOutcome.Verified;
    }

    private static bool IsActive(VerificationSession s) =>
        s.Status == VerificationStatus.Pending && s.ExpiresAt > DateTime.UtcNow;

    private async Task<ContactOutcome> FailAsync(VerificationSession session, string reason, ContactOutcome outcome)
    {
        session.Status = VerificationStatus.Failed;
        session.Reason = reason;
        await sessions.UpdateAsync(session);
        return outcome;
    }
}
