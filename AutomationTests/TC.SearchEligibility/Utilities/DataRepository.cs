namespace TC.MemberEligibilitySearch.Utilities
{
    public static class DataRepository
    {
        public static object GetAutomationData(string flowName)
        {
            return Selenium.BaseComponents.Utilities.AutomationDataRepository
                .GetAutomationData<Models.SearchEligibility>(flowName, "SearchMemberEligiblity");
        }
    }
}
