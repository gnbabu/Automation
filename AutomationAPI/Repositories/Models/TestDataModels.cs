namespace AutomationAPI.Repositories.Models
{
    // Structure of a test-data model class (e.g. TestDataModels.SubmitPAProfessional),
    // read by reflection from the Release folder - see TestDataSchemaReader.
    public class TestDataFlowSchema
    {
        public string FlowName { get; set; } = "";
        public string ModelType { get; set; } = "";
        public List<TestDataSectionSchema> Sections { get; set; } = new();
        // Properties that couldn't be turned into a section/field (e.g. nested classes
        // deeper than one level) - surfaced in the UI instead of silently dropped.
        public List<string> Warnings { get; set; } = new();
    }

    public class TestDataSectionSchema
    {
        public int SectionId { get; set; }
        public string SectionName { get; set; } = "";
        // True for List<T> properties - the section holds one row per item.
        public bool IsList { get; set; }
        public List<TestDataFieldSchema> Fields { get; set; } = new();
        // Values the model class itself initializes (property initializers, e.g.
        // `= "Professional"` or a list initialized with sample rows) - shown in the
        // Portal when nothing is saved for the test case yet, and used by test runs
        // for any section the test case hasn't saved.
        public List<Dictionary<string, string?>> DefaultRows { get; set; } = new();
    }

    public class TestDataFieldSchema
    {
        public string Name { get; set; } = "";
        // text | integer | decimal | date | datetime | boolean | enum
        public string Type { get; set; } = "text";
        public bool Nullable { get; set; }
        public List<string>? Options { get; set; }
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
        // Short model class name (the flow), or null when the test has no data model.
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

    // A test case's saved data as the Portal edits it: one entry per section saved in
    // the document (sections missing here aren't saved - the model's defaults apply).
    public class TestDataDocumentView
    {
        // Version of the stored document; sent back on save so a save based on a stale
        // copy is rejected instead of overwriting someone else's change.
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

    // The stored document row (aut.TestCaseData).
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
        // Version the Portal loaded (null when nothing was saved yet).
        public long? ExpectedVersion { get; set; }
    }

    // "Copy all": replace the target test case's whole document (current user, target
    // environment) with a source document - either another test case of the same model
    // (From* key) or the same test case in another environment (FromEnvironmentId).
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
        // True when the stored document changed since the caller loaded it - nothing
        // was written.
        public bool IsConflict { get; set; }
        public long? Version { get; set; }
    }
}
