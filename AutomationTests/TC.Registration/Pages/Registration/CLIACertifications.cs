using OpenQA.Selenium.Support.UI;
using Selenium.BaseComponents.Pages;
using SeleniumExtensions.Extensions;
using Selenium.BaseComponents.Utilities;
using OpenQA.Selenium;

namespace TC.ProviderDataEntry.Pages.Registration
{
    public class CLIACertifications : BasePage
    {
        IWebDriver webDriver;
        string RegID;
        public CLIACertifications(IWebDriver webDriver, string RegID) : base(webDriver)
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

        #region CLIA Certifications
        public IWebElement? RegistrationTitle
        {
            get
            {
                return Selenium.BaseComponents.Utilities.PageHelper.IsElementExist(webDriver, By.Id("ctl00_MainContent_ucRegistrationNavigation_lblTitle")) ?
                    webDriver.CreateSmartElement(By.Id("ctl00_MainContent_ucRegistrationNavigation_lblTitle")).Element : null;
            }
        }
        public IWebElement AddCLIACertification
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id($"ctl00_MainContent_ucCLIACertification_{RegID}_btnAddCertSecondGrid")).Element;
            }
        }
        public IWebElement CLIANumber
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id($"ctl00_MainContent_ucCLIACertification_{RegID}_prov_Number")).Element;
            }
        }
        public IWebElement CLIACertificationType
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id($"ctl00_MainContent_ucCLIACertification_{RegID}_ddlCliaCertType")).Element;
            }
        }
        public IWebElement CLIAEffectiveDate
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id($"ctl00_MainContent_ucCLIACertification_{RegID}_prov_Start")).Element;
            }
        }
        public IWebElement CLIAExpirationDate
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id($"ctl00_MainContent_ucCLIACertification_{RegID}_prov_End")).Element;
            }
        }
        public IWebElement SaveButton
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id("ctl00_MainContent_ucRegistrationNavigation_btnSave")).Element;
            }
        }
        #endregion
    }
}
