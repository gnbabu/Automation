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

        // The only methods that ever return a decrypted password.
        Task<LoginUserCredentials?> GetCredentialsAsync(int loginUserId);

        // Used only by BaseFeatureFixture.LoginByProfile's mid-test role-switch - a
        // genuinely different, unattended use case from the initial Run Now/Schedule
        // login (see usp_LoginUserResolveByRole).
        Task<LoginUserCredentials?> ResolveByRoleAsync(int environmentId, string userRole);
    }
}
