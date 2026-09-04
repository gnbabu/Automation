using System;
using OpenQA.Selenium.Support.UI;
using Selenium.BaseComponents.Pages;
using SeleniumExtensions.Extensions;
using Selenium.BaseComponents.Utilities;
using OpenQA.Selenium;
using TC.ProviderDataEntry.Models;
using SeleniumExtensions.Configurations;
using Selenium.BaseComponents.Data;

namespace TC.ProviderDataEntry.Pages.Registration
{
 
    public class MyQueue: BasePage
    {
        IWebDriver webDriver;
        string RegID;

        public MyQueue(IWebDriver webDriver, string RegID) : base(webDriver)
        {

            this.webDriver = webDriver;

            string url = this.webDriver.Url;

            if (!string.IsNullOrEmpty(RegID))
            {
                this.RegID = RegID;
            }
            else
            {
                var regIDTag = url.Split('?').Last();
                var regID = regIDTag.Split('=').Last();
                this.RegID = regID;
            }
        }

        public IWebElement QueuePageHeader
        {
            get
            {
                return webDriver.CreateSmartElement(By.XPath($"//*[@id='ctl00_pnlPageHeader']/div/h1")).Element;
            }
        }

        public IWebElement SidebarMenu
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id($"menu")).Element;
            }
        }

        public IWebElement ProviderSearch
        {
            get
            {
                return webDriver.CreateSmartElement(By.XPath($"//a[@title='Provider Search']")).Element;
            }
        }

        public void WaitUntilElementIsVisible()
        {
            PageHelper.WaitUntilElementIsVisible(webDriver, By.XPath("//*[@id='ctl00_pnlPageHeader']/div/h1"), TimeoutConfiguration.Element);
        }
    }
}
