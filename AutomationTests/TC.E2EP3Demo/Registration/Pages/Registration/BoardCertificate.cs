using TC.ProviderDataEntry.Models;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using Selenium.BaseComponents.Pages;
using SeleniumExtensions.Extensions;
using Selenium.BaseComponents.Utilities;
using SeleniumExtensions.Configurations;

namespace TC.ProviderDataEntry.Pages.Registration
{
    public class BoardCertificate :BasePage
    {
        IWebDriver webDriver;
        string RegID;
        public BoardCertificate(IWebDriver webDriver, string RegID) : base(webDriver)
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

        #region Board Certificate
        public IWebElement? RegistrationTitle
        {
            get
            {
                return Selenium.BaseComponents.Utilities.PageHelper.IsElementExist(webDriver, By.Id("ctl00_MainContent_ucRegistrationNavigation_lblTitle")) ?
                    webDriver.CreateSmartElement(By.Id("ctl00_MainContent_ucRegistrationNavigation_lblTitle")).Element : null;
            }
        }
        public IWebElement AddBoardCertificate
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id($"ctl00_MainContent_ucBoardCertification_{RegID}_btnAddBoardCertification")).Element;
            }
        }
        public IWebElement BoardCertificateNo
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id($"ctl00_MainContent_ucBoardCertification_{RegID}_rblBoardCertified_0")).Element;
            }
        }
        public IWebElement BoardCertificateYes
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id($"ctl00_MainContent_ucBoardCertification_{RegID}_rblBoardCertified_1")).Element;
            }
        }
        public IWebElement BoardCertification
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id($"ctl00_MainContent_ucBoardCertification_{RegID}_ddlBoardCertification")).Element;
            }
        }
        public IWebElement BoardSpecialty
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id($"ctl00_MainContent_ucBoardCertification_{RegID}_ddlBoardSpecialty")).Element;
            }
        }
        public IWebElement CertificationNumber
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id($"ctl00_MainContent_ucBoardCertification_{RegID}_txtCertificationNumber")).Element;
            }
        }
        public IWebElement EffectiveDate
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id($"ctl00_MainContent_ucBoardCertification_{RegID}_txtEffectiveDate")).Element;
            }
        }
        public IWebElement ExpirationDate
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id($"ctl00_MainContent_ucBoardCertification_{RegID}_txtExpirationDate")).Element;
            }
        }
        #endregion
    }
}
