using AutomationAPI.Repositories.Models;

namespace AutomationAPI.Repositories.Interfaces
{
    public interface IEnvironmentRepository
    {
        Task<int> CreateAsync(EnvironmentRequestDto request);
        Task UpdateAsync(EnvironmentRequestDto request);
        
        Task<IEnumerable<EnvironmentModel>> GetAllAsync();
        Task<EnvironmentModel> GetByIdAsync(int environmentId);

        Task SoftDeleteAsync(int environmentId);
        Task HardDeleteAsync(int environmentId);
    }
}
