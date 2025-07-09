using AutomationAPI.Repositories.Models;

namespace AutomationAPI.Repositories.Interfaces
{
    public interface IAutomationRepository
    {

        Task<IEnumerable<AutomationFlow>> GetAutomationFlowNamesAsync();
        Task<IEnumerable<AutomationDataSection>> GetAutomationDataSectionsAsync(string flowName = null);
        Task<AutomationData> GetAutomationDataAsync(int sectionId, int userId);
        Task<IEnumerable<AutomationData>> GetAutomationDataByFlowNameAsync(string flowName);
        Task<int> InsertAutomationDataAsync(AutomationDataRequest automationDataRequest);
        Task UpdateAutomationDataAsync(AutomationDataRequest automationDataRequest);
        Task DeleteAutomationDataAsync(int sectionId);
        Task<int> InsertAutomationDataSectionAsync(AutomationDataSectionRequest request);
        Task UpdateAutomationDataSectionAsync(AutomationDataSectionRequest request);
        Task DeleteAutomationDataSectionAsync(int sectionId);
    }
}
