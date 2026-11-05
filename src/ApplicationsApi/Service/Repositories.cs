using System.Collections.Concurrent;
using ApplicationsApi.Models;

namespace ApplicationsApi.Services;

public interface IApplicationRepository
{
    Task<Application?> GetAsync(Guid id);
    Task AddAsync(Application application);
    Task UpdateAsync(Application application);
    Task<string> NextNumberAsync();
    Task<IReadOnlyList<Application>> ListAsync();
}

public class InMemoryApplicationRepository : IApplicationRepository
{
    private readonly ConcurrentDictionary<Guid, Application> _items = new();

    public Task<Application?> GetAsync(Guid id) =>
        Task.FromResult(_items.TryGetValue(id, out var application) ? application : null);

    public Task AddAsync(Application application)
    {
        _items[application.Id] = application;
        return Task.CompletedTask;
    }

    public Task UpdateAsync(Application application)
    {
        _items[application.Id] = application;
        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<Application>> ListAsync() =>
        Task.FromResult<IReadOnlyList<Application>>(
            _items.Values.OrderBy(x => x.CreatedAt).ToList());

    public Task<string> NextNumberAsync()
    {
        var year = DateTime.UtcNow.Year;
        var next = _items.Values.Count(x => x.Number is not null) + 1;
        return Task.FromResult($"{year}-{next:D6}");
    }
}

public interface IVerificationSessionRepository
{
    Task AddAsync(VerificationSession session);
    Task UpdateAsync(VerificationSession session);
    Task<VerificationSession?> GetAsync(Guid id);
    Task<VerificationSession?> GetByTokenHashAsync(string tokenHash);
    Task<VerificationSession?> GetActiveByChatAsync(long chatId);
    Task ExpirePendingAsync(Guid applicationId);
    Task<IReadOnlyList<VerificationSession>> ListAsync();
}

public class InMemoryVerificationSessionRepository : IVerificationSessionRepository
{
    private readonly ConcurrentDictionary<Guid, VerificationSession> _items = new();

    public Task AddAsync(VerificationSession session)
    {
        _items[session.Id] = session;
        return Task.CompletedTask;
    }

    public Task UpdateAsync(VerificationSession session)
    {
        _items[session.Id] = session;
        return Task.CompletedTask;
    }

    public Task<VerificationSession?> GetAsync(Guid id) =>
        Task.FromResult<VerificationSession?>(_items.TryGetValue(id, out var session) ? session : null);

    public Task<VerificationSession?> GetByTokenHashAsync(string tokenHash) =>
        Task.FromResult(_items.Values.FirstOrDefault(x => x.TokenHash == tokenHash));

    public Task<VerificationSession?> GetActiveByChatAsync(long chatId) =>
        Task.FromResult(_items.Values
            .Where(x => x.TelegramChatId == chatId
                        && x.Status == VerificationStatus.Pending
                        && x.ExpiresAt > DateTime.UtcNow)
            .OrderByDescending(x => x.CreatedAt)
            .FirstOrDefault());

    public Task ExpirePendingAsync(Guid applicationId)
    {
        foreach (var session in _items.Values.Where(x =>
                     x.ApplicationId == applicationId && x.Status == VerificationStatus.Pending))
        {
            session.Status = VerificationStatus.Expired;
        }
        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<VerificationSession>> ListAsync() =>
        Task.FromResult<IReadOnlyList<VerificationSession>>(
            _items.Values.OrderBy(x => x.CreatedAt).ToList());
}
