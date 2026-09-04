namespace Selenium.BaseComponents.Utilities
{
    public class AutomationData
    {
        public int Id { get; set; }
        public int SectionID { get; set; }
        public string SectionName { get; set; }
        public int FlowID { get; set; }

        public string TestContent { get; set; }

        public List<AutomationContent> automationContents { get; set; }

    }

    public class AutomationContent
    {
        public string FieldName { get; set; }

        public string FieldValue { get; set; }
    }
}
