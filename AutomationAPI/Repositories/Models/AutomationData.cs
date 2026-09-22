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

        // Parsed from TestContent's JSON (see AutomationDataHelper.ParseAutomationContents)
        // for GetAutomationDataByFlowNameAsync specifically - the Selenium test side's own
        // client-model (Selenium.BaseComponents.Utilities.AutomationData) has a real
        // `List<AutomationContent> automationContents` field it reads directly
        // (AutomationDataRepository.GetAutomationData<T>/every TC.*'s own DataRepository
        // call `Mapper.BindData<T>(data.automationContents)`), but this field was never
        // actually populated server-side - every one of those calls threw
        // "System.ArgumentNullException: Value cannot be null. (Parameter 'source')" the
        // moment a real queued test run actually exercised this path end-to-end for the
        // first time (confirmed - see AGENTS.md). ASP.NET Core's default camelCase JSON
        // naming policy serializes this as "automationContents", matching the client
        // model's field name exactly with no changes needed on that side.
        public List<AutomationContentItem>? AutomationContents { get; set; }
    }

    public class AutomationContentItem
    {
        public string? FieldName { get; set; }
        public string? FieldValue { get; set; }
    }
}
