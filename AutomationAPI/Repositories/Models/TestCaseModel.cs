namespace AutomationAPI.Repositories.Models
{
    public class TestCaseModel
    {
        public string LibraryName { get; set; }
        public string ClassName { get; set; }
        public string MethodName { get; set; }
        public string Description { get; set; }
        public string Priority { get; set; }
        public string TestCaseId { get; set; }
        public List<string> AssignedUsers { get; set; } = new();
    }
}
