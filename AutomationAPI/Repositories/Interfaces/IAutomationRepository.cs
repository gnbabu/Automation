using AutomationAPI.Repositories.Models;

namespace AutomationAPI.Repositories.Interfaces
{
    public interface IAutomationRepository
    {

        Task<IEnumerable<AutomationFlow>> GetAutomationFlowNamesAsync();
        Task<IEnumerable<AutomationDataSection>> GetAutomationDataSectionsAsync(string flowName = null);
        Task<AutomationData> GetAutomationDataAsync(int sectionId, int userId, int environmentId);

        // Added for audit-log instrumentation - UpdateAutomationDataAsync's own request
        // only ever carries Id + the new TestContent (never SectionId/UserId/
        // EnvironmentId), so there was no way to know which section/user/environment a
        // given update belongs to, or to diff old vs new TestContent, without this.
        Task<AutomationData?> GetAutomationDataByIdAsync(int id);
        Task<IEnumerable<AutomationData>> GetAutomationDataByFlowNameAsync(string flowName);
        Task<int> InsertAutomationDataAsync(AutomationDataRequest automationDataRequest);
        Task UpdateAutomationDataAsync(AutomationDataRequest automationDataRequest);
        Task DeleteAutomationDataAsync(int sectionId);
        Task<int> InsertAutomationDataSectionAsync(AutomationDataSectionRequest request);
        Task UpdateAutomationDataSectionAsync(AutomationDataSectionRequest request);
        Task DeleteAutomationDataSectionAsync(int sectionId, bool cascade = false);
    }
}
