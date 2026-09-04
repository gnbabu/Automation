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

 

namespace TC.PriorAuthoriztion.Pages
{
    public class SearchPriorAuthorizationPage : BasePage
    {
        IWebDriver webDriver;

        public SearchPriorAuthorizationPage(IWebDriver webDriver) : base(webDriver)
        {
            this.webDriver = webDriver;
            string url = this.webDriver.Url;
        }
 

        public IWebElement lnkBtnSubmitPriorAuth
        {
            get
            {
                return webDriver.CreateSmartElement(By.XPath($"//span[text()='Submit PA']")).Element;
            }
        }
        public void WaitUntilElementIsVisible()
        {
            BasePageHelper.WaitUntilElementIsVisible(webDriver, By.XPath("//*[@id='ctl00_MainContent_ucRegProgressBar_lblPRONPI2']"), TimeoutConfiguration.Element);
        }
    }
}

