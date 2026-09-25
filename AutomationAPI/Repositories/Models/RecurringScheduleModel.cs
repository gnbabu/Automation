namespace AutomationAPI.Repositories.Models
{
    public class RecurringScheduleModel
    {
        public int RecurringScheduleId { get; set; }
        public int AssignmentId { get; set; }
        public string AssignmentName { get; set; }
        public string Environment { get; set; }
        public string ReleaseName { get; set; }
        public string ReleaseLifecycle { get; set; }
        public string RecurrenceType { get; set; }
        public string DaysOfWeek { get; set; }
        public int? DayOfMonth { get; set; }
        public TimeSpan TimeOfDay { get; set; }
        public string Browser { get; set; }
        public int? LoginUserId { get; set; }
        public bool IsActive { get; set; }
        public string PausedReason { get; set; }
        public DateTime? EndDate { get; set; }
        public DateTime NextRunDate { get; set; }
        public DateTime? LastRunDate { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
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
