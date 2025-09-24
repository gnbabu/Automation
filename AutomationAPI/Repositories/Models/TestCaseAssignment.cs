namespace AutomationAPI.Repositories.Models
{
    public class TestCaseAssignment
    {
        public int AssignmentId { get; set; }
        public int UserId { get; set; }
        public string LibraryName { get; set; } = string.Empty;
        public string ClassName { get; set; } = string.Empty;
        public string MethodName { get; set; } = string.Empty;
        public DateTime AssignedOn { get; set; }
        public int? AssignedBy { get; set; }
    }
}
