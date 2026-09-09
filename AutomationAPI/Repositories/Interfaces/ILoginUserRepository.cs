using AutomationAPI.Repositories.Models;

namespace AutomationAPI.Repositories.Interfaces
{
    public interface ILoginUserRepository
    {
        Task<int> CreateAsync(LoginUserRequestDto request);

        // Self-service ownership: each returns false (0 rows affected) instead of
        // throwing when the caller's portalUserId doesn't own the row - lets the
        // controller distinguish "not found" vs "not yours" and return 403.
        Task<bool> UpdateAsync(LoginUserRequestDto request, int portalUserId);
        Task<bool> SoftDeleteAsync(int loginUserId, int portalUserId, int? modifiedBy = null);
        Task<bool> HardDeleteAsync(int loginUserId, int portalUserId);

        // Unfiltered - every login user for the environment, any owner. Used only by
        // Run Now/Schedule's dropdown resolution before this self-service change; kept
        // as-is/unused-but-available rather than removed (see AGENTS.md).
        Task<IEnumerable<LoginUserModel>> GetByEnvironmentAsync(int environmentId);

        // Ownership-filtered - used only by the self-service Credential Configuration
        // screen and (after this change) Run Now/Schedule's own dropdown resolution.
        Task<IEnumerable<LoginUserModel>> GetByEnvironmentAndPortalUserAsync(int environmentId, int portalUserId);

        // The only methods that ever return a decrypted password.
        Task<LoginUserCredentials?> GetCredentialsAsync(int loginUserId);

        // Used only by BaseFeatureFixture.LoginByProfile's mid-test role-switch - a
        // genuinely different, unattended use case from the initial Run Now/Schedule
        // login (see usp_LoginUserResolveByRole).
        Task<LoginUserCredentials?> ResolveByRoleAsync(int environmentId, string userRole);
    }
}
