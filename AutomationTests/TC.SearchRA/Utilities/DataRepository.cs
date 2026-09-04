using Selenium.BaseComponents.Utilities;

namespace TC.SearchRA.Utilities
{
    public static class DataRepository
    {
     

        public static object GetAutomationData(string flowName)
        {

            APIGatway aPIGatway = new APIGatway();
            List<AutomationData> automationDatas = aPIGatway.GetAutomationData(flowName).Result;

          
            if (automationDatas != null && automationDatas.Count() > 0 && flowName.Equals("SearchRA"))
            {
                Models.SearchRA SearchRA = new Models.SearchRA();

                foreach (AutomationData data in automationDatas)
                {
                    string key = data.SectionName;
                    List<AutomationContent> dt = data.automationContents;

                    if (key.Equals("SearchRAParams"))
                        SearchRA = Mapper.BindData<Models.SearchRA>(dt);
                }

                return SearchRA;
            }
           
            return null;
        }

    }
}