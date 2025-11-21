using AutomationAPI.Repositories.Models;

namespace AutomationAPI.Repositories.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResponse> Login(LoginModel model);
        Task<bool> ForgotPassword(string email);

        Task<bool> Register(RegistrationModel model);
    }
}
