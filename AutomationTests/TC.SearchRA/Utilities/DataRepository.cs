namespace TC.SearchRA.Utilities
{
    public static class DataRepository
    {
        public static object GetAutomationData(string flowName)
        {
            return Selenium.BaseComponents.Utilities.AutomationDataRepository
                .GetAutomationData<Models.SearchRA>(flowName, "SearchRAParams");
        }
    }
}
