using TC.ProviderDataEntry.Models;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using Selenium.BaseComponents.Pages;
using SeleniumExtensions.Configurations;
using SeleniumExtensions.Extensions;
using Selenium.BaseComponents.Utilities;

namespace TC.ProviderDataEntry.Pages.Registration
{
    public class ProfessionalLicense :BasePage
    {
        IWebDriver webDriver;
        string RegID;
        public ProfessionalLicense(IWebDriver webDriver, string RegID) : base(webDriver)
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

        #region Professional License

        public IWebElement? RegistrationTitle
        {
            get
            {
                return Selenium.BaseComponents.Utilities.PageHelper.IsElementExist(webDriver, By.Id("ctl00_MainContent_ucRegistrationNavigation_lblTitle")) ?
                    webDriver.CreateSmartElement(By.Id("ctl00_MainContent_ucRegistrationNavigation_lblTitle")).Element : null;
            }
        }
        public IWebElement AddLicenseBtn
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id($"ctl00_MainContent_ucLicenses_{RegID}_btnAddLicenses")).Element;
            }
        }
        public IWebElement LicenseState
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id($"ctl00_MainContent_ucLicenses_{RegID}_ddlLicenseState")).Element;
            }
        }

        public IWebElement LicenseBoard
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id($"ctl00_MainContent_ucLicenses_{RegID}_ddlBoardName")).Element;
            }
        }

        public IWebElement LicenseNumber
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id($"ctl00_MainContent_ucLicenses_{RegID}_prov_Number")).Element;
            }
        }
        public IWebElement LicenseEffectiveDate
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id($"ctl00_MainContent_ucLicenses_{RegID}_prov_Effective")).Element;
            }
        }
        public IWebElement LicenseExpirationDate
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id($"ctl00_MainContent_ucLicenses_{RegID}_prov_End")).Element;
            }
        }
        public IWebElement LicenseAddress1
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id($"ctl00_MainContent_ucLicenses_{RegID}_ucLicenseAddress_txtAddress1")).Element;
            }
        }
        public IWebElement LicenseAddress2
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id($"ctl00_MainContent_ucLicenses_{RegID}_ucLicenseAddress_txtAddress2")).Element;
            }
        }
        public IWebElement LicenseCity
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id($"ctl00_MainContent_ucLicenses_{RegID}_ucLicenseAddress_txtCity")).Element;
            }
        }
        public IWebElement State
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id($"ctl00_MainContent_ucLicenses_{RegID}_ucLicenseAddress_ddlState")).Element;
            }
        }
        public IWebElement LicenseCountry
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id($"ctl00_MainContent_ucLicenses_{RegID}_ucLicenseAddress_ddlCounty")).Element;
            }
        }
        public IWebElement LicenseZip
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id($"ctl00_MainContent_ucLicenses_{RegID}_ucLicenseAddress_nbZipFirst5")).Element;
            }
        }
        public IWebElement EndorsementNumber
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id($"ctl00_MainContent_ucLicenses_{RegID}_ucSpecialtyFocus_tbEndorsemnetNumber")).Element;
            }
        }
        public IWebElement EndorsementStatus
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id($"ctl00_MainContent_ucLicenses_{RegID}_ucSpecialtyFocus_tbEndorsemnetStatus")).Element;
            }
        }
        public IWebElement EndorsementFocus
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id($"ctl00_MainContent_ucLicenses_{RegID}_ucSpecialtyFocus_tbFocus")).Element;
            }
        }
        public IWebElement EndorsementSpecality
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id($"ctl00_MainContent_ucLicenses_{RegID}_ucSpecialtyFocus_tbSpecialty")).Element;
            }
        }
        public IWebElement CertifyingOrganization
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id($"ctl00_MainContent_ucLicenses_{RegID}_ucSpecialtyFocus_tbCertifyingOrg")).Element;
            }
        }
        public IWebElement CertificationDate
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id($"ctl00_MainContent_ucLicenses_{RegID}_ucSpecialtyFocus_tbCertificationDate")).Element;
            }
        }
        public IWebElement CertificationExpiration
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id($"ctl00_MainContent_ucLicenses_{RegID}_ucSpecialtyFocus_tbCertificationEndDate")).Element;
            }
        }
        #endregion
        #region Functions
        public void SelectPickListvalue(IWebElement element,String item)
        {
            SelectElement dropDownElement = new SelectElement(element);
            dropDownElement.SelectByText(item);
        }

        public void SetProfessionalLicense(ProfessionalLicenses info)
        {
            PageHelper.WaitUntilDocumentIsReady(webDriver, TimeoutConfiguration.Page);

            if (!string.IsNullOrEmpty(info.State))
                SelectPickListvalue(LicenseState, info.State);

            if (!string.IsNullOrEmpty(info.LicenseNumber))
                LicenseNumber.Set(info.LicenseNumber, true);

            if (!string.IsNullOrEmpty(info.EffectiveDate))
                LicenseEffectiveDate.Set(info.EffectiveDate, true);

            if (!string.IsNullOrEmpty(info.ExpirationDate))
                LicenseExpirationDate.Set(info.ExpirationDate);

            if (!string.IsNullOrEmpty(info.LicenseBoardName))
                SelectPickListvalue(LicenseBoard, info.LicenseBoardName);

        }
        #endregion
    }
}
