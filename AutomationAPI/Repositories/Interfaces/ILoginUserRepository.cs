using AutomationAPI.Repositories.Models;

namespace AutomationAPI.Repositories.Interfaces
{
    public interface ILoginUserRepository
    {
        Task<int> CreateAsync(LoginUserRequestDto request);
        Task UpdateAsync(LoginUserRequestDto request);
        Task SoftDeleteAsync(int loginUserId, int? modifiedBy = null);
        Task HardDeleteAsync(int loginUserId);

        Task<IEnumerable<LoginUserModel>> GetByEnvironmentAsync(int environmentId);

        // The only method that ever returns a decrypted password.
        Task<LoginUserCredentials?> GetCredentialsAsync(int loginUserId);
    }
}
