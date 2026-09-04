using Selenium.BaseComponents.Utilities;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using TC.PriorAuthoriztion.Utilities;
using System.Net.NetworkInformation;
using TC.PriorAuthoriztion.Models;

namespace TC.PriorAuthoriztion.Utilities
{
    public static class DataRepository
    {
       

        public static object GetAutomationData(string flowName)
        {

            APIGatway aPIGatway = new APIGatway();
            List<AutomationData> automationDatas = aPIGatway.GetAutomationData(flowName).Result;

            
            if (automationDatas != null && automationDatas.Count() > 0 && flowName.Equals("DentalPA"))
            {
                 DentalPA dentalPA = new DentalPA();

                foreach (AutomationData data in automationDatas)
                {
                    string key = data.SectionName;
                    List<AutomationContent> dt = data.automationContents;

                    if (key.Equals("DentalInformation"))
                        dentalPA.DentalInformation = Mapper.BindData<DentalInformation>(dt);

                    if (key.Equals("DentalRecipientInformation"))
                        dentalPA.DentalRecipientInformation = Mapper.BindData<DentalRecipientInformation>(dt);

                    if (key.Equals("DentalContactInformation"))
                        dentalPA.DentalContactInformation = Mapper.BindData<DentalContactInformation>(dt);

                    if (key.Equals("DentalServiceInformation"))
                        dentalPA.DentalServiceInformation = Mapper.BindData<DentalServiceInformation>(dt);

                    if (key.Equals("DentalServiceProviderInformation"))
                        dentalPA.DentalServiceProviderInformation = Mapper.BindData<DentalServiceProviderInformation>(dt);

                    if (key.Equals("DentalOrderingProviderInformation"))
                        dentalPA.DentalOrderingProviderInformation = Mapper.BindData<DentalOrderingProviderInformation>(dt);

                    if (key.Equals("DentalDiagnosisInformation"))
                        dentalPA.DentalDiagnosisInformation = Mapper.BindData<DentalDiagnosisInformation>(dt);

                    if (key.Equals("DentalServiceDetails"))
                        dentalPA.DentalServiceDetails = Mapper.BindData<DentalServiceDetails>(dt);

                    if (key.Equals("DentalProviderNotes"))
                        dentalPA.DentalProviderNotes = Mapper.BindData<DentalProviderNotes>(dt);

                    if (key.Equals("DentalAttachments"))
                        dentalPA.DentalAttachments = Mapper.BindData<DentalAttachments>(dt);
                }

                return dentalPA;
            }
      
            return null;
        }

    }
}