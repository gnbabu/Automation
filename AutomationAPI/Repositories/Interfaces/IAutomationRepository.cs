using AutomationAPI.Repositories.Models;

namespace AutomationAPI.Repositories.Interfaces
{
    public interface IAutomationRepository
    {
        Task<IEnumerable<AutomationDataSection>> GetAutomationDataSectionsAsync(string flowName = null);
        Task<IEnumerable<AutomationFlow>> GetAutomationFlowNamesAsync();
        Task<IEnumerable<AutomationData>> GetAutomationDataAsync(int sectionId);
        Task<IEnumerable<AutomationData>> GetAutomationDataByFlowNameAsync(string flowName);
        Task<int> InsertAutomationDataAsync(AutomationDataRequest automationDataRequest);
        Task UpdateAutomationDataAsync(AutomationDataRequest automationDataRequest);
        Task DeleteAutomationDataAsync(int sectionId);
        Task<int> InsertAutomationDataSectionAsync(AutomationDataSectionRequest request);
        Task UpdateAutomationDataSectionAsync(AutomationDataSectionRequest request);
        Task DeleteAutomationDataSectionAsync(int sectionId);

    }
}
