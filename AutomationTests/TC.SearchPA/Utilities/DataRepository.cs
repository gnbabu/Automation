using Selenium.BaseComponents.Utilities;
using TC.PriorAuthSearch.Models;

namespace TC.PriorAuthSearch.Utilities
{
    public static class DataRepository
    {
        public static object GetAutomationData(string flowName)
        {
            return Selenium.BaseComponents.Utilities.AutomationDataRepository
                .GetAutomationData<Models.SearchPA>(flowName, "SearchPA");
        }
    }
}
