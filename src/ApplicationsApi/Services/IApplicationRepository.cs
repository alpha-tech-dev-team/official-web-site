using ApplicationsApi.Models;

namespace ApplicationsApi.Services;

public interface IApplicationRepository
{
    Task<Application> AddAsync(Application app);
    Task<Application?> GetAsync(Guid id);
    Task<IReadOnlyList<Application>> ListAsync();
    Task UpdateAsync(Application app);
    Task<string> NextNumberAsync();
}