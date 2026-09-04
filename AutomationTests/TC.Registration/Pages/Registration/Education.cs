using OpenQA.Selenium.Support.UI;
using Selenium.BaseComponents.Pages;
using SeleniumExtensions.Extensions;
using Selenium.BaseComponents.Utilities;
using OpenQA.Selenium;
using TC.ProviderDataEntry.Models;

namespace TC.ProviderDataEntry.Pages.Registration
{
    public class Education : BasePage
    {
        IWebDriver webDriver;
        string RegID;
        public Education(IWebDriver webDriver, string RegID) : base(webDriver)
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
        #region Education
        public IWebElement? RegistrationTitle
        {
            get
            {
                return PageHelper.IsElementExist(webDriver, By.Id("ctl00_MainContent_ucRegistrationNavigation_lblTitle")) ?
                    webDriver.CreateSmartElement(By.Id("ctl00_MainContent_ucRegistrationNavigation_lblTitle")).Element : null;
            }
        }

        public IWebElement AddEducation
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id($"ctl00_MainContent_ucEducation_{RegID}_btnAddEducationItem")).Element;
            }
        }
        public IWebElement EducationType
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id($"ctl00_MainContent_ucEducation_{RegID}_ddlEducationType")).Element;
            }
        }
        public IWebElement NameOfSchool
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id($"ctl00_MainContent_ucEducation_{RegID}_tbSchool")).Element;
            }
        }
        public IWebElement StartDate
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id($"ctl00_MainContent_ucEducation_{RegID}_tbStartYear")).Element;
            }
        }
        public IWebElement EndDate
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id($"ctl00_MainContent_ucEducation_{RegID}_tbEndYear")).Element;
            }
        }
        public IWebElement CertificateAwarded
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id($"ctl00_MainContent_ucEducation_{RegID}_ddlDegreeAward")).Element;
            }
        }
        public IWebElement Speciality
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id($"ctl00_MainContent_ucEducation_{RegID}_ddlSpeciality")).Element;
            }
        }
        public IWebElement Address1
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id($"ctl00_MainContent_ucEducation_{RegID}_tbAddress1")).Element;
            }
        }
        public IWebElement Address2
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id($"ctl00_MainContent_ucEducation_{RegID}_tbAddress2")).Element;
            }
        }
        public IWebElement City
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id($"ctl00_MainContent_ucEducation_{RegID}_tbCity")).Element;
            }
        }
        public IWebElement State
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id($"ctl00_MainContent_ucEducation_{RegID}_ddlState")).Element;
            }
        }
        public IWebElement Country
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id($"ctl00_MainContent_ucEducation_{RegID}_ddlCountry")).Element;
            }
        }
        public IWebElement ZipCode
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id($"ctl00_MainContent_ucEducation_{RegID}_tbZipCode")).Element;
            }
        }
        public IWebElement PhoneNumber
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id($"ctl00_MainContent_ucEducation_{RegID}_tbPhone")).Element;
            }
        }
        public IWebElement Fax
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id($"ctl00_MainContent_ucEducation_{RegID}_tbFax")).Element;
            }
        }
        public IWebElement AdditionalInformation
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id($"ctl00_MainContent_ucEducation_{RegID}_tbAdditional")).Element;
            }
        }
        #endregion

        #region Education
        public void SelectDropDownValue(IWebElement element,String value)
        {
            SelectElement dropDownElement = new SelectElement(element);
            dropDownElement.SelectByText(value);
        }

        public void SetEducationDetails(EducationDTO info)
        {
            if (!string.IsNullOrEmpty(info.EducationType))
                SelectDropDownValue(EducationType, info.EducationType);

            if (!string.IsNullOrEmpty(info.NameOfSchool))
                NameOfSchool.Set(info.NameOfSchool,true);

            if (!string.IsNullOrEmpty(info.StartDate))
                StartDate.Set(info.StartDate, true);

            if (!string.IsNullOrEmpty(info.EndDate))
                EndDate.Set(info.EndDate, true);

            if (!string.IsNullOrEmpty(info.CertificateAwarded))
                SelectDropDownValue(CertificateAwarded, info.CertificateAwarded);

            if (!string.IsNullOrEmpty(info.Address1))
                Address1.SendKeys(info.Address1);

            if (!string.IsNullOrEmpty(info.Address2))
                Address2.Set(info.Address2, true);

            if (!string.IsNullOrEmpty(info.City))
                City.Set(info.City, true);

            if (!string.IsNullOrEmpty(info.State))
                SelectDropDownValue(State,info.State);

            if (!string.IsNullOrEmpty(info.ZipCode))
                ZipCode.Set(info.ZipCode, true);

            if (!string.IsNullOrEmpty(info.PhoneNumber))
                PhoneNumber.Set(info.PhoneNumber, true);

            if (!string.IsNullOrEmpty(info.Country))
                SelectDropDownValue(Country, info.Country);

            if (!string.IsNullOrEmpty(info.Address1))
                Address1.SendKeys(info.Address1);

            if (!string.IsNullOrEmpty(info.Address2))
                Address2.Set(info.Address2, true);

            if (!string.IsNullOrEmpty(info.NameOfSchool))
                NameOfSchool.Set(info.NameOfSchool, true);

            if (!string.IsNullOrEmpty(info.ZipCode))
                ZipCode.Set(info.ZipCode, true);

        }
        #endregion

    }
}
