using AutomationAPI.Repositories.Models;
using Microsoft.AspNetCore.Mvc;

namespace AutomationAPI.Repositories.Interfaces
{
    public interface IUserRepository
    {
        Task<IEnumerable<User>> GetAllUsersAsync();
        Task<User> GetUserByIdAsync(int userId);        
        Task<int> CreateUserAsync(User user);
        Task UpdateUserAsync(User user);
        Task DeleteUserAsync(int userId);
        Task ChangePasswordAsync(ChangePasswordRequest request);
        Task SetUserActiveStatusAsync(int userId, bool active);
        Task<IEnumerable<UserRole>> GetUserRolesAsync();
        Task<IEnumerable<User>> GetFilteredUsersAsync(UserFilter filters);
        Task<int> RegisterUserAsync(RegistrationModel model);
        Task<IEnumerable<UserStatus>> GetUserStatusesAsync();
        Task<IEnumerable<AppTimeZone>> GetTimeZonesAsync();
        Task<IEnumerable<PriorityStatus>> GetPriorityStatusesAsync();
        Task<User?> GetUserByEmailAsync(string email);
        Task<bool> ForgotPasswordAsync(string email, string token, DateTime expiry);
        Task<bool> ResetPasswordAsync(string token, string passwordHash);
        Task<User> GetUserByUsernameAsync(string username);
        Task UpdateLastLoginAsync(int? userId);
    }
}
