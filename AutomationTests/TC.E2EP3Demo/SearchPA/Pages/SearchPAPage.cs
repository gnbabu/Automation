 
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

namespace TC.PriorAuthSearch.Pages
{
    public class SearchPAPage : BasePage
    {
        IWebDriver webDriver;
        public SearchPAPage(IWebDriver webDriver) : base(webDriver)
        {
            this.webDriver = webDriver;
            string url = this.webDriver.Url;
        }

        public IWebElement lnkBtnSearchPA
        {
            get
            {
                return webDriver.CreateSmartElement(By.XPath($"//span[text()='Search PA']")).Element;
            }
        }
        public void WaitUntilElementIsVisible()
        {
            BasePageHelper.WaitUntilElementIsVisible(webDriver, By.XPath("//*[@id='priorauthSearch']"), TimeoutConfiguration.Element);
        }
        public IWebElement ddlStatus
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id("ctl00_MainContent_SearchPriorAuthorization_ddlStatus")).Element;
            }
        }
        public IWebElement ddlPayerName
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id("ctl00_MainContent_SearchPriorAuthorization_ddlPayerName")).Element;
            }
        }
        public IWebElement ddlAssignment
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id("ctl00_MainContent_SearchPriorAuthorization_ddlAssignment")).Element;
            }
        }
        public IWebElement txtPriorAuthNumber
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id("ctl00_MainContent_SearchPriorAuthorization_txtPriorAuthNumber")).Element;
            }
        }

        public IWebElement txtSubmissiondate
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id("ctl00_MainContent_SearchPriorAuthorization_txtSubmissiondate")).Element;
            }
        }

 
        public IWebElement txtPatientTrackingNumber
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id("ctl00_MainContent_SearchPriorAuthorization_txtPatientTrackingNumber")).Element;
            }
        }
        public IWebElement txtMedicaidBillingNumber
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id("ctl00_MainContent_SearchPriorAuthorization_txtMedicaidBillingNumber")).Element;
            }
        }
        public IWebElement txtICDCode
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id("ctl00_MainContent_SearchPriorAuthorization_txtICDCode")).Element;
            }
        }

        public IWebElement txtProcedureCode
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id("ctl00_MainContent_SearchPriorAuthorization_txtProcedureCode")).Element;
            }
        }
        public IWebElement txtRevenuecode
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id("ctl00_MainContent_SearchPriorAuthorization_txtRevenuecode")).Element;
            }
        }
        public IWebElement txtBirthDate
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id("ctl00_MainContent_SearchPriorAuthorization_txtBirthDate")).Element;
            }
        }
        public IWebElement txtDiagnoisCode
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id("ctl00_MainContent_SearchPriorAuthorization_txtDiagnoisCode")).Element;
            }
        }
        public IWebElement txtorderProvnpi
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id("ctl00_MainContent_SearchPriorAuthorization_txtorderProvnpi")).Element;
            }
        }
        public IWebElement txtPAEffDate
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id("ctl00_MainContent_SearchPriorAuthorization_txtPAEffDate")).Element;
            }
        }
        public IWebElement txtPAExpDate
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id("ctl00_MainContent_SearchPriorAuthorization_txtPAExpDate")).Element;
            }
        }
        public IWebElement btnSearch
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id("ctl00_MainContent_SearchPriorAuthorization_btnSearch")).Element;
            }
        }
        public IWebElement btnClear
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id("ctl00_MainContent_SearchPriorAuthorization_btnClear")).Element;
            }
        }

        public void SetPayerName(string payerType)
        {
            if (!string.IsNullOrEmpty(payerType))
            {
                SelectElement assignedType = new SelectElement(ddlPayerName);
                assignedType.SelectByText(payerType);
            }
        }
        public void SetSearchPAStatus(string status)
        {
            if (!string.IsNullOrEmpty(status))
            {
                SelectElement assignedType = new SelectElement(ddlStatus);
                assignedType.SelectByText(status);
            }
        }
        public void SetAssignment(string assignmentType)
        {
            if (!string.IsNullOrEmpty(assignmentType))
            {
                SelectElement assignedType = new SelectElement(ddlAssignment);
                assignedType.SelectByText(assignmentType);
            }
        }
    }

}
