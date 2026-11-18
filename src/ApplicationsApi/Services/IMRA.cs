using System.Collections.Concurrent;
using ApplicationsApi.Models;

namespace ApplicationsApi.Services;

public class IMRA : IApplicationRepository
{
    private readonly ConcurrentDictionary<Guid, Application> _items = new();
    private int _counter;

    public Task<Application> AddAsync(Application app)
    {
        _items[app.Id] = app;
        return Task.FromResult(app);
    }

    public Task<Application?> GetAsync(Guid id) =>
        Task.FromResult<Application?>(_items.TryGetValue(id, out var a) ? a : null);

    public Task<IReadOnlyList<Application>> ListAsync() =>
        Task.FromResult<IReadOnlyList<Application>>(_items.Values.OrderBy(a => a.CreatedAt).ToList());

    public Task UpdateAsync(Application app)
    {
        _items[app.Id] = app;
        return Task.CompletedTask;
    }

    public Task<string> NextNumberAsync() =>
        Task.FromResult($"{DateTime.UtcNow.Year}-{Interlocked.Increment(ref _counter):D6}");
}