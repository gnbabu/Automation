using OpenQA.Selenium.Support.UI;
using Selenium.BaseComponents.Pages;
using SeleniumExtensions.Extensions;
using Selenium.BaseComponents.Utilities;
using OpenQA.Selenium;

namespace TC.ProviderDataEntry.Pages.Registration
{
    public class StateCDSNumber : BasePage
    {
        IWebDriver webDriver;
        string RegID;
        public StateCDSNumber(IWebDriver webDriver, string RegID) : base(webDriver)
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
        #region State CDS Number
        public IWebElement? RegistrationTitle
        {
            get
            {
                return PageHelper.IsElementExist(webDriver, By.Id("ctl00_MainContent_ucRegistrationNavigation_lblTitle")) ?
                    webDriver.CreateSmartElement(By.Id("ctl00_MainContent_ucRegistrationNavigation_lblTitle")).Element : null;
            }
        }
        public IWebElement AddStateCDSNumber
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id($"ctl00_MainContent_ucCDS_{RegID}_btnAddCDS")).Element;
            }
        }
        public IWebElement CDSNumber
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id($"ctl00_MainContent_ucCDS_{RegID}_txtSTATE_CDS_Number")).Element;
            }
        }
        public IWebElement State
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id($"ctl00_MainContent_ucCDS_{RegID}_ddlState")).Element;
            }
        }
        public IWebElement DateIssued
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id($"ctl00_MainContent_ucCDS_{RegID}_txtDateIssued")).Element;
            }
        }
        public IWebElement ExpirationDate
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id($"ctl00_MainContent_ucCDS_{RegID}_txtExpirationDate")).Element;
            }
        }
        public IWebElement FileUpload
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id($"ctl00_MainContent_ucCDS_{RegID}_233_RadAsyncUpload1file0")).Element;
            }
        }
        #endregion
    }
}
