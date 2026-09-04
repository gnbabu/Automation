using Selenium.BaseComponents.Utilities;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Net.NetworkInformation;
 

namespace TC.MemberEligibilitySearch.Utilities
{
    public static class DataRepository
    {

        public static object GetAutomationData(string flowName)
        {

            APIGatway aPIGatway = new APIGatway();
            List<AutomationData> automationDatas = aPIGatway.GetAutomationData(flowName).Result;

            if (automationDatas != null && automationDatas.Count() > 0 && flowName.Equals("SearchMemberEligiblity"))
            {
                Models.SearchEligibility searchEligibility = new Models.SearchEligibility();

                foreach (AutomationData data in automationDatas)
                {
                    string key = data.SectionName;
                    List<AutomationContent> dt = data.automationContents;

                    if (key.Equals("SearchMemberEligiblity"))
                        searchEligibility = Mapper.BindData<Models.SearchEligibility>(dt);
                }

                return searchEligibility;
            }
            return null;
        }

    }
}