namespace AutomationAPI.Repositories.Models
{
    public class ReleaseModel
    {
        public int ReleaseId { get; set; }
        public string ReleaseName { get; set; }
        public string Version { get; set; }
        public int? EnvironmentId { get; set; }
        public string EnvironmentName { get; set; }
        public string Description { get; set; }
        public string ReleaseFolderPath { get; set; }

        public string ReleaseLifecycle { get; set; }
        public bool IsActive { get; set; }

        public string SignOffStatus { get; set; }
        public string SignedOffBy { get; set; }
        public DateTime? SignedOffOn { get; set; }

        public DateTime CreatedOn { get; set; }
        public string CreatedBy { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public string ActivatedBy { get; set; }
        public DateTime? ActivatedOn { get; set; }

        // Cheap, filesystem-based readiness indicator for list views (file count only,
        // no reflection). Populated by the controller after fetching from the database,
        // since DLL content is filesystem state, not database state.
        public int DllFileCount { get; set; }
        public bool FolderReady { get; set; }

        // Test summary (from AssignedTestCases via assignments on ReleaseId) - despite the
        // name, TotalTests here is really "total *assigned* test cases", not the total
        // test cases discoverable in the Release's DLLs. Kept as-is for backward
        // compatibility (Passed/Failed/Skipped/Running only make sense for
        // assigned+executed tests anyway); TotalDiscoveredTests below is the real total.
        public int TotalTests { get; set; }
        public int PassedTests { get; set; }
        public int FailedTests { get; set; }
        public int SkippedTests { get; set; }
        public int RunningTests { get; set; }

        // Real total test cases discoverable across every DLL in the Release's folder,
        // regardless of assignment - filesystem/reflection state, not database state, so
        // populated by the controller the same way DllFileCount/FolderReady are.
        public int TotalDiscoveredTests { get; set; }
    }

    public class ReleaseRequestDto
    {
        public int? ReleaseId { get; set; }

        // Required for both create & update (validated in the controller)
        public string? ReleaseName { get; set; }

        // Required for create (business uniqueness = Name + Version + Environment)
        public string? Version { get; set; }

        // Required for create; references existing Environment Management record
        public int? EnvironmentId { get; set; }

        // Optional
        public string? Description { get; set; }

        // Used only for update (ignored during create)
        public string? ReleaseLifecycle { get; set; }

        // Used only for update (ignored during create)
        public bool? IsActive { get; set; }

        // Audit (username)
        public string? CreatedBy { get; set; }
        public string? ModifiedBy { get; set; }
    }

    public class ReleaseSignOffRequestDto
    {
        // "Approved" or "Rejected"
        public string? SignOffStatus { get; set; }
        public string? SignOffBy { get; set; }
        public string? Comments { get; set; }
    }

    public class ReleaseActivateRequestDto
    {
        public string? ActivatedBy { get; set; }
    }
}
