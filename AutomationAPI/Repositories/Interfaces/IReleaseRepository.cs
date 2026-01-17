using AutomationAPI.Repositories.Models;

namespace AutomationAPI.Repositories.Interfaces
{
    public interface IReleaseRepository
    {
        Task<int> CreateAsync(ReleaseRequestDto releaseRequest);
        Task UpdateAsync(ReleaseRequestDto releaseRequest);
        Task<IEnumerable<ReleaseModel>> GetAllAsync();
        Task<ReleaseModel> GetByIdAsync(int releaseId);
        Task SignOffAsync(int releaseId, string signedOffBy);
    }
}
