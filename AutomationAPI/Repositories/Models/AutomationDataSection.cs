namespace AutomationAPI.Repositories.Models
{
    public class AutomationDataSection
    {
        public int? SectionId { get; set; }
        public string? SectionName { get; set; }
        public string? FlowName { get; set; }
        // Created from a test-data model in code ([TestDataFlow]) - read-only in Flow &
        // Section Management, since renaming it would break the mapping to the model.
        public bool IsFromModel { get; set; }
    }
}
