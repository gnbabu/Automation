using Selenium.BaseComponents.Utilities;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace TC.SubmitClaims.Utilities
{
    public static class DataRepository
    {


        public static object GetAutomationData(string flowName)
        {

            APIGatway aPIGatway = new APIGatway();
            List<AutomationData> automationDatas = aPIGatway.GetAutomationData(flowName).Result;


            if (automationDatas != null && automationDatas.Count() > 0 && flowName.Equals("SearchPA"))
            {

            }

            return null;
        }

    }
}