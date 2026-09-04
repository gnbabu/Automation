 
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
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

namespace TC.SearchRA.Pages
{
    public class SearchRAPage : BasePage
    {
        IWebDriver webDriver;
        public SearchRAPage(IWebDriver webDriver) : base(webDriver)
        {
            this.webDriver = webDriver;
            string url = this.webDriver.Url;
        }

        public IWebElement lnkBtnSearchRA
        {
            get
            {
                return webDriver.CreateSmartElement(By.XPath($"//span[text()='Search-RA']")).Element;
            }
        }
        public void WaitUntilElementIsVisible()
        {
            BasePageHelper.WaitUntilElementIsVisible(webDriver, By.XPath("//*[@id='ctl00_MainContent_ERemittanceAdvice_sepInstructions']"), TimeoutConfiguration.Element);
        }
        public IWebElement ddlPrimaryDestinationPayer
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id("ctl00_MainContent_ERemittanceAdvice_ddlPrimaryDestinationPayer")).Element;
            }
        }

        public IWebElement txtRANumber
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id("ctl00_MainContent_ERemittanceAdvice_txtRANumber")).Element;
            }
        }
        public IWebElement txtICN
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id("ctl00_MainContent_ERemittanceAdvice_txtICN")).Element;
            }
        }
        public IWebElement txtDateAvailableFrom
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id("ctl00_MainContent_ERemittanceAdvice_txtDateAvailableFrom")).Element;
            }
        }
        public IWebElement txtDateAvailableTo
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id("ctl00_MainContent_ERemittanceAdvice_txtDateAvailableTo")).Element;
            }
        }
        public IWebElement btnSearch
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id("ctl00_MainContent_ERemittanceAdvice_btnSearch")).Element;
            }
        }
        public IWebElement btnClear
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id("ctl00_MainContent_ERemittanceAdvice_btnClear")).Element;
            }
        }

        public void SetPrimaryDestinationPayer(string payerType)
        {
            if (!string.IsNullOrEmpty(payerType))
            {
                SelectElement assignedType = new SelectElement(ddlPrimaryDestinationPayer);
                assignedType.SelectByText(payerType);
            }
        }


    }

}
