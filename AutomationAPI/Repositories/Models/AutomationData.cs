namespace AutomationAPI.Repositories.Models
{
    public class AutomationData
    {
        public int Id { get; set; }
        public int SectionId { get; set; }
        public string? TestContent { get; set; }
        public string? SectionName { get; set; }
        public int? UserId { get; set; }
        public int? EnvironmentId { get; set; }
        public List<AutomationContentItem>? AutomationContents { get; set; }
    }

    public class AutomationContentItem
    {
        public string? FieldName { get; set; }
        public string? FieldValue { get; set; }
    }
}
