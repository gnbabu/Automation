namespace AutomationAPI.Repositories.Models
{
    public class TestDataFlowSchema
    {
        public string FlowName { get; set; } = "";
        public string ModelType { get; set; } = "";
        public List<TestDataSectionSchema> Sections { get; set; } = new();
        public List<string> Warnings { get; set; } = new();
        public List<TestDataNote> Notes { get; set; } = new();
    }

    public class TestDataNote
    {
        public string? Section { get; set; }
        public string? Field { get; set; }
        public string Message { get; set; } = "";
    }

    public class TestDataSectionSchema
    {
        public int SectionId { get; set; }
        public string SectionName { get; set; } = "";
        public bool IsList { get; set; }
        public List<TestDataFieldSchema> Fields { get; set; } = new();
        public List<Dictionary<string, string?>> DefaultRows { get; set; } = new();
    }

    public class TestDataFieldSchema
    {
        public string Name { get; set; } = "";
        public string Type { get; set; } = "text";
        public bool Nullable { get; set; }
        public List<string>? Options { get; set; }
        public long? Min { get; set; }
        public long? Max { get; set; }
    }

    public class TestDataTestCase
    {
        public int AssignmentTestCaseId { get; set; }
        public string LibraryName { get; set; } = "";
        public string ClassName { get; set; } = "";
        public string MethodName { get; set; } = "";
        public string? TestCaseId { get; set; }
        public string? TestCaseDescription { get; set; }
        public int? ReleaseId { get; set; }
        public string? FlowName { get; set; }
    }

    public class TestDataAssignment
    {
        public int AssignmentTestCaseId { get; set; }
        public string LibraryName { get; set; } = "";
        public string ClassName { get; set; } = "";
        public string MethodName { get; set; } = "";
        public string? TestCaseId { get; set; }
        public int AssignedUser { get; set; }
        public int? EnvironmentId { get; set; }
        public int? ReleaseId { get; set; }
        public string? ReleaseFolderPath { get; set; }
    }

    public class TestDataDocumentView
    {
        public long? Version { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public List<TestDataSectionRows> Sections { get; set; } = new();
    }

    public class TestDataSectionRows
    {
        public int SectionId { get; set; }
        public string SectionName { get; set; } = "";
        public List<Dictionary<string, string?>> Rows { get; set; } = new();
    }

    public class TestDataDocumentRecord
    {
        public int TestCaseDataId { get; set; }
        public string FlowName { get; set; } = "";
        public string DataJson { get; set; } = "{}";
        public long Version { get; set; }
        public DateTime? ModifiedOn { get; set; }
    }

    public class TestDataSectionSaveRequest
    {
        public int AssignmentTestCaseId { get; set; }
        public int EnvironmentId { get; set; }
        public int SectionId { get; set; }
        public List<Dictionary<string, string?>> Rows { get; set; } = new();
        public long? ExpectedVersion { get; set; }
    }

    public class TestDataCopyAllRequest
    {
        public int AssignmentTestCaseId { get; set; }
        public int EnvironmentId { get; set; }
        public string? FromLibrary { get; set; }
        public string? FromClass { get; set; }
        public string? FromMethod { get; set; }
        public int? FromEnvironmentId { get; set; }
        public long? ExpectedVersion { get; set; }
    }

    public class TestDataEffectiveDocument
    {
        public long? Version { get; set; }
        public System.Text.Json.Nodes.JsonObject Document { get; set; } = new();
        public List<string> SavedSections { get; set; } = new();
    }

    public class TestDataJsonSaveRequest
    {
        public int AssignmentTestCaseId { get; set; }
        public int EnvironmentId { get; set; }
        public string? SectionName { get; set; }
        public string Json { get; set; } = "";
        public long? ExpectedVersion { get; set; }
    }

    public class TestDataResetRequest
    {
        public int AssignmentTestCaseId { get; set; }
        public int EnvironmentId { get; set; }
        public string? SectionName { get; set; }
        public long? ExpectedVersion { get; set; }
    }

    public class TestDataCopySource
    {
        public string LibraryName { get; set; } = "";
        public string ClassName { get; set; } = "";
        public string MethodName { get; set; } = "";
        public string? TestCaseId { get; set; }
    }

    public class TestDataSaveResult
    {
        public int? TestCaseDataId { get; set; }
        public bool IsNew { get; set; }
        public bool IsConflict { get; set; }
        public long? Version { get; set; }
    }
}
