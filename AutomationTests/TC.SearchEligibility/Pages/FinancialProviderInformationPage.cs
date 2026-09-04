using OpenQA.Selenium;
using SdetToolbox.Pages;
using Selenium.BaseComponents.Pages;
using Selenium.BaseComponents.Utilities;
using SeleniumExtensions.Configurations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BasePageHelper = Selenium.BaseComponents.Utilities.PageHelper;

namespace TC.MemberEligibilitySearch.Pages
{
    public class FinancialProviderInformationPage : BasePage
    {
        IWebDriver webDriver;

        public FinancialProviderInformationPage(IWebDriver webDriver) : base(webDriver)
        {
            this.webDriver = webDriver;
            string url = this.webDriver.Url;
        }

        public IWebElement lblTitle
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id($"lblTitle")).Element;
            }
        }

        public IWebElement TxtMedicaid
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id($"ctl00_MainContent_txtMedicaidNumber")).Element;
            }
        }

        public IWebElement lnkBtnPriorAuth
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id($"ctl00_MainContent_lnkBtnPriorAuth")).Element;
            }
        }
        public void WaitUntilElementIsVisible()
        {
            BasePageHelper.WaitUntilElementIsVisible(webDriver, By.XPath("//*[@id='lblTitle']"), TimeoutConfiguration.Element);
        }
    }
}
