namespace AutomationAPI.Repositories.Models
{
    public class RecurringScheduleModel
    {
        public int RecurringScheduleId { get; set; }
        public int AssignmentId { get; set; }
        public string AssignmentName { get; set; }
        public string Environment { get; set; }
        public int? EnvironmentId { get; set; }
        public string ReleaseName { get; set; }
        public string ReleaseLifecycle { get; set; }
        public string RecurrenceType { get; set; }
        public string DaysOfWeek { get; set; }
        public int? DayOfMonth { get; set; }
        public TimeSpan TimeOfDay { get; set; }
        public string Browser { get; set; }
        public int? LoginUserId { get; set; }
        public string LoginUserRole { get; set; }
        public string LoginUserName { get; set; }
        public bool IsActive { get; set; }
        public string PausedReason { get; set; }
        public DateTime? EndDate { get; set; }
        public DateTime NextRunDate { get; set; }
        public DateTime? LastRunDate { get; set; }
        public int RunCount { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
    }

    // aut.RecurringScheduleRunHistory - one row per RecurringScheduleWorker firing
    // attempt. TestCasesQueuedCount/Passed/Failed/SkippedCount reflect a snapshot taken at
    // firing time (see aut.RecurringScheduleRunHistoryTestCase) so a later firing re-running
    // the same test case can't corrupt an earlier firing's recorded outcome.
    public class RecurringScheduleRunHistoryModel
    {
        public long RunHistoryId { get; set; }
        public int RecurringScheduleId { get; set; }
        public DateTime RunDate { get; set; }
        public string Result { get; set; }
        public string Detail { get; set; }
        public int TestCasesQueuedCount { get; set; }
        public string ResolutionStatus { get; set; }
        public int? PassedCount { get; set; }
        public int? FailedCount { get; set; }
        public int? SkippedCount { get; set; }
        public DateTime? ResolvedOn { get; set; }
    }

    public class RecurringScheduleDueModel
    {
        public int RecurringScheduleId { get; set; }
        public int AssignmentId { get; set; }
        public string AssignmentName { get; set; }
        public string ReleaseName { get; set; }
        public string ReleaseLifecycle { get; set; }
        public string RecurrenceType { get; set; }
        public string DaysOfWeek { get; set; }
        public int? DayOfMonth { get; set; }
        public TimeSpan TimeOfDay { get; set; }
        public string Browser { get; set; }
        public int? LoginUserId { get; set; }
        public DateTime? EndDate { get; set; }
    }

    public class AssignmentOptionModel
    {
        public int AssignmentId { get; set; }
        public string AssignmentName { get; set; }
        public string Environment { get; set; }
        public int? EnvironmentId { get; set; }
        public int ReleaseId { get; set; }
        public string ReleaseName { get; set; }
        public string ReleaseLifecycle { get; set; }
    }
}
