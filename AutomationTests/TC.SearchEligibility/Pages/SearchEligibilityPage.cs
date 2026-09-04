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

namespace TC.MemberEligibilitySearch.Pages
{
    public class SearchEligibility : BasePage
    {
        IWebDriver webDriver;
        public SearchEligibility(IWebDriver webDriver) : base(webDriver)
        {
            this.webDriver = webDriver;
            string url = this.webDriver.Url;
        }

        public IWebElement lnkBtnSearchEligibility
        {
            get
            {
                return webDriver.CreateSmartElement(By.XPath($"//span[text()='Search Eligibility']")).Element;
            }
        }
        public void WaitUntilElementIsVisible()
        {
            BasePageHelper.WaitUntilElementIsVisible(webDriver, By.XPath("//*[@id='eligibilitysearch']"), TimeoutConfiguration.Element);
        }
        public IWebElement txtMedicaidBillingNumber
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id("ctl00_MainContent_ucRecipientEligibilitySearch_txtMedicaidBillingNumber")).Element;
            }
        }

        public IWebElement txtBirthDate
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id("ctl00_MainContent_ucRecipientEligibilitySearch_txtBirthDate")).Element;
            }
        }
        public IWebElement txtFromDos
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id("ctl00_MainContent_ucRecipientEligibilitySearch_txtFromDos")).Element;
            }
        }
        public IWebElement txtToDos
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id("ctl00_MainContent_ucRecipientEligibilitySearch_txtToDos")).Element;
            }
        }
        public IWebElement txtProcedureCode
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id("ctl00_MainContent_ucRecipientEligibilitySearch_txtProcedureCode")).Element;
            }
        }
        public IWebElement txtSSN
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id("ctl00_MainContent_ucRecipientEligibilitySearch_txtSSN")).Element;
            }
        }
        public IWebElement lblErrorMsg
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id("ctl00_MainContent_ucRecipientEligibilitySearch_lblErrorMsg")).Element;
            }
        }

        public IWebElement btnSearch
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id("ctl00_MainContent_ucRecipientEligibilitySearch_btnSearch")).Element;
            }
        }
        public IWebElement btnClear
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id("ctl00_MainContent_ucRecipientEligibilitySearch_btnClear")).Element;
            }
        }
        public IWebElement txtRecinfoMedicaidbillNumber
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id("ctl00_MainContent_ucRecipientEligibilitySearch_txtRecinfoMedicaidbillNumber")).Element;
            }
        }
        public IWebElement txtFirstName
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id("ctl00_MainContent_ucRecipientEligibilitySearch_txtFirstName")).Element;
            }
        }
        public IWebElement txtLast
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id("ctl00_MainContent_ucRecipientEligibilitySearch_txtLast")).Element;
            }
        }
        public IWebElement txtDOB
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id("ctl00_MainContent_ucRecipientEligibilitySearch_txtDOB")).Element;
            }
        }
       
    }

}
