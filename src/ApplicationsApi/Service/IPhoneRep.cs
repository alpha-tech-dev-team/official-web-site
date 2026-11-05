using ApplicationsApi.Models;

namespace ApplicationsApi.Services;

public interface IPhoneVerificationRepository
{
    Task AddAsync(PhoneVerification verification);
    Task<IReadOnlyList<PhoneVerification>> ListAsync();
}
