using AutomationAPI.Repositories.Models;

namespace AutomationAPI.Repositories.Interfaces
{
    public interface IRecurringScheduleRepository
    {
        Task<int> CreateAsync(int assignmentId, string recurrenceType, string daysOfWeek, int? dayOfMonth, TimeSpan timeOfDay, string browser, int? loginUserId, DateTime? endDate, DateTime nextRunDate, string createdBy);
        Task<IEnumerable<RecurringScheduleModel>> GetAllAsync();
        Task SetActiveAsync(int recurringScheduleId, bool isActive, string pausedReason = null);
        Task DeleteAsync(int recurringScheduleId);
        Task<IEnumerable<RecurringScheduleDueModel>> GetDueAsync(DateTime now);
        Task MarkRunAsync(int recurringScheduleId, DateTime? nextRunDate, bool isActive, string pausedReason = null);
        Task<List<int>> GetEligibleTestCaseIdsAsync(int assignmentId);
        Task<IEnumerable<AssignmentOptionModel>> GetAssignmentOptionsAsync();
    }
}
