using System.Collections.Concurrent;
using ApplicationsApi.Models;

namespace ApplicationsApi.Services;

public class InMemoryPhoneVerificationRepository : IPhoneVerificationRepository
{
    private readonly ConcurrentDictionary<Guid, PhoneVerification> _items = new();

    public Task AddAsync(PhoneVerification verification)
    {
        _items[verification.Id] = verification;
        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<PhoneVerification>> ListByApplicationAsync(Guid applicationId) =>
        Task.FromResult<IReadOnlyList<PhoneVerification>>(
            _items.Values
                .Where(x => x.ApplicationId == applicationId)
                .OrderByDescending(x => x.VerifiedAt)
                .ToList());

    public Task<IReadOnlyList<PhoneVerification>> ListAsync() =>
        Task.FromResult<IReadOnlyList<PhoneVerification>>(
            _items.Values.OrderBy(x => x.VerifiedAt).ToList());
}
