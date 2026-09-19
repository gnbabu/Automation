using OpenQA.Selenium.Support.UI;
using Selenium.BaseComponents.Pages;
using SeleniumExtensions.Extensions;
using Selenium.BaseComponents.Utilities;
using OpenQA.Selenium;

namespace TC.ProviderDataEntry.Pages.Registration
{
    public class MCPAffiliation : BasePage
    {
        IWebDriver webDriver;
        string RegID;
        public MCPAffiliation(IWebDriver webDriver, string RegID) : base(webDriver)
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
        #region MCP Affiliation
        public IWebElement? RegistrationTitle
        {
            get
            {
                return PageHelper.IsElementExist(webDriver, By.Id("ctl00_MainContent_ucRegistrationNavigation_lblTitle")) ?
                    webDriver.CreateSmartElement(By.Id("ctl00_MainContent_ucRegistrationNavigation_lblTitle")).Element : null;
            }
        }
        public IWebElement ManagedCarePlans_Yes
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id($"ctl00_MainContent_ucMCOAffiliation_{RegID}_rblIsContracting_0")).Element;
            }
        }
        public IWebElement ManagedCarePlans_No
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id($"ctl00_MainContent_ucMCOAffiliation_{RegID}_rblIsContracting_0")).Element;
            }
        }
        public IWebElement AmeriHealthCaritas
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id($"ctl00_MainContent_ucMCOAffiliation_{RegID}_chkPossibleParticipation_0")).Element;
            }
        }
        public IWebElement AnthemBlueCross
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id($"ctl00_MainContent_ucMCOAffiliation_{RegID}_chkPossibleParticipation_1")).Element;
            }
        }
        public IWebElement Aetna
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id($"ctl00_MainContent_ucMCOAffiliation_{RegID}_chkPossibleParticipation_2")).Element;
            }
        }
        public IWebElement Buckeye
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id($"ctl00_MainContent_ucMCOAffiliation_{RegID}_chkPossibleParticipation_3")).Element;
            }
        }
        public IWebElement CareSource
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id($"ctl00_MainContent_ucMCOAffiliation_{RegID}_chkPossibleParticipation_4")).Element;
            }
        }
        public IWebElement Humana
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id($"ctl00_MainContent_ucMCOAffiliation_{RegID}_chkPossibleParticipation_5")).Element;
            }
        }
        public IWebElement Molina
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id($"ctl00_MainContent_ucMCOAffiliation_{RegID}_chkPossibleParticipation_6")).Element;
            }
        }
        public IWebElement UnitedHealthCare
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id($"ctl00_MainContent_ucMCOAffiliation_{RegID}_chkPossibleParticipation_1")).Element;
            }
        }
        #endregion
    }
}
