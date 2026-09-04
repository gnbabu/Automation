using OpenQA.Selenium;
using SdetToolbox.Pages;
using Selenium.BaseComponents.Pages;
using Selenium.BaseComponents.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TC.ProviderDataEntry.Pages.Registration
{
    public class SubmissionConfirmation : BasePage
    {
        IWebDriver webDriver;

        public SubmissionConfirmation(IWebDriver webDriver) : base(webDriver)
        {
            this.webDriver = webDriver;
        }

      
        public IWebElement btnReturnToHome
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id($"ctl00_MainContent_btnReturn")).Element;
            }
        }

    }
}
