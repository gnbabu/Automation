namespace AutomationAPI.Repositories.Models
{
    public class TestCaseAssignmentDeleteRequest
    {
        public IEnumerable<int>? UserIds { get; set; }      // Optional list of UserIds
        public string? LibraryName { get; set; }           // Optional Library filter
        public string? ClassName { get; set; }             // Optional Class filter
        public string? MethodName { get; set; }            // Optional Method filter
    }
}
