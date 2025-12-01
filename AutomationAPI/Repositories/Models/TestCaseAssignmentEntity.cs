namespace AutomationAPI.Repositories.Models
{
    public class TestCaseAssignmentEntity
    {
        public int AssignmentId { get; set; }
        public string AssignmentName { get; set; }
        public string AssignmentStatus { get; set; }
        public int AssignedUser { get; set; }
        public string ReleaseName { get; set; }
        public string Environment { get; set; }
        public DateTime AssignedDate { get; set; }
        public int AssignedBy { get; set; }
        public DateTime LastUpdatedDate { get; set; }

        // Optional (from joined User table)
        public string AssignedUserName { get; set; }
        public string AssignedByUserName { get; set; }
    }

    public class AssignedTestCase
    {
        public int AssignmentTestCaseId { get; set; }
        public int AssignmentId { get; set; }
        public string TestCaseId { get; set; }
        public string TestCaseDescription { get; set; }
        public string TestCaseStatus { get; set; }
        public string ClassName { get; set; }
        public string LibraryName { get; set; }
        public string MethodName { get; set; }
        public string Priority { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public double? Duration { get; set; }
        public string ErrorMessage { get; set; }
        public int AssignedUserId { get; set; }
        public string AssignedUserName { get; set; }
        public string Environment { get; set; }
        
    }

    public class AssignmentCreateUpdateRequest
    {
        public int AssignedUser { get; set; }
        public string AssignmentStatus { get; set; }
        public string ReleaseName { get; set; }
        public string Environment { get; set; }
        public int AssignedBy { get; set; }
        public IEnumerable<TestCaseRequestModel> TestCases { get; set; }
    }
    public class TestCaseRequestModel
    {
        public string TestCaseId { get; set; }
        public string TestCaseDescription { get; set; }
        public string TestCaseStatus { get; set; }
        public string ClassName { get; set; }
        public string LibraryName { get; set; }
        public string MethodName { get; set; }
        public string Priority { get; set; }
    }


    public class AssignedTestCaseStatusUpdate
    {
        public int AssignmentTestCaseId { get; set; }
        public string? TestCaseStatus { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public double? Duration { get; set; }
        public string? ErrorMessage { get; set; }
    }


}
