using AutomationAPI.Repositories.Models;
using Microsoft.AspNetCore.Mvc;

namespace AutomationAPI.Repositories.Interfaces
{
    public interface IUserRepository
    {
        Task<IEnumerable<User>> GetAllUsersAsync();
        Task<User> GetUserByIdAsync(int userId);
        Task<User> ValidateUserByUsernameAndPasswordAsync(string username, string password);
        Task<int> CreateUserAsync(User user);
        Task UpdateUserAsync(User user);
        Task DeleteUserAsync(int userId);
        Task ChangePasswordAsync(ChangePasswordRequest request);
        Task SetUserActiveStatusAsync(int userId, bool active);
        Task<IEnumerable<UserRole>> GetUserRoles();
        Task<IEnumerable<User>> GetFilteredUsersAsync(UserFilter filters);
    }
}
