using Selenium.BaseComponents.Utilities;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using TC.PriorAuthSearch.Utilities;
using System.Net.NetworkInformation;
using TC.PriorAuthSearch.Models;

namespace TC.PriorAuthSearch.Utilities
{
    public static class DataRepository
    {
       

        public static object GetAutomationData(string flowName)
        {

            APIGatway aPIGatway = new APIGatway();
            List<AutomationData> automationDatas = aPIGatway.GetAutomationData(flowName).Result;


            if (automationDatas != null && automationDatas.Count() > 0 && flowName.Equals("SearchPA"))
            {
                Models.SearchPA SearchPA = new Models.SearchPA();

                foreach (AutomationData data in automationDatas)
                {
                    string key = data.SectionName;
                    List<AutomationContent> dt = data.automationContents;

                    if (key.Equals("SearchPA"))
                        SearchPA = Mapper.BindData<Models.SearchPA>(dt);
                }

                return SearchPA;
            }

            return null;
        }

    }
}