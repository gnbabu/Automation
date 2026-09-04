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
    public class GroupReview : BasePage
    {
        IWebDriver webDriver;
        string RegID;

        public GroupReview(IWebDriver webDriver, string RegID) : base(webDriver)
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

        public IWebElement txtRegID
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id($"ctl00_MainContent_ucGroupReview_txtRegID")).Element;
            }
        }

        public IWebElement Search
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id($"ctl00_MainContent_ucGroupReview_btnSearch")).Element;
            }
        }

        public IWebElement ProviderSearch
        {
            get
            {
                return webDriver.CreateSmartElement(By.XPath($"//a[@title='Provider Search']")).Element;
            }
        }
       
        public void WaitUntilRowCountIsVisible()
        {
            PageHelper.WaitUntilElementIsVisible(webDriver, By.Id("ctl00_MainContent_ucGroupReview_lnkRowCount"), TimeoutConfiguration.Element);
        }

        public IWebElement Review
        {
            get
            {
                return webDriver.CreateSmartElement(By.XPath($"//table[@id='ctl00_MainContent_ucGroupReview_gvProviders']//a[text()='Review']")).Element;
            }
        }
        public IWebElement SidebarMenu
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id($"menu")).Element;
            }
        }

    }
}
