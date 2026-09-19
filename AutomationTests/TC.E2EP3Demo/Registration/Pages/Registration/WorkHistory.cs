using OpenQA.Selenium.Support.UI;
using Selenium.BaseComponents.Pages;
using SeleniumExtensions.Extensions;
using Selenium.BaseComponents.Utilities;
using OpenQA.Selenium;
using TC.ProviderDataEntry.Models;

namespace TC.ProviderDataEntry.Pages.Registration
{
    public class WorkHistory : BasePage
    {
        IWebDriver webDriver;
        string RegID;
        public WorkHistory(IWebDriver webDriver, string RegID) : base(webDriver)
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

        #region Work History
        public IWebElement? RegistrationTitle
        {
            get
            {
                return PageHelper.IsElementExist(webDriver, By.Id("ctl00_MainContent_ucRegistrationNavigation_lblTitle")) ?
                    webDriver.CreateSmartElement(By.Id("ctl00_MainContent_ucRegistrationNavigation_lblTitle")).Element : null;
            }
        }

        public IWebElement AddWorkHistory
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id($"ctl00_MainContent_ucWorkHistoryDetails_{RegID}_btnAddWorkItem")).Element;
            }
        }
        public IWebElement CurrentEmployer
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id($"ctl00_MainContent_ucWorkHistoryDetails_{RegID}_chkIsCurrentEmployer")).Element;
            }
        }
        public IWebElement EmployerName
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id($"ctl00_MainContent_ucWorkHistoryDetails_{RegID}_tbOrgName")).Element;
            }
        }
        public IWebElement StartDate
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id($"ctl00_MainContent_ucWorkHistoryDetails_{RegID}_tbFrom")).Element;
            }
        }
        public IWebElement EndDate
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id($"ctl00_MainContent_ucWorkHistoryDetails_{RegID}_txtEndDate")).Element;
            }
        }
        public IWebElement OrganizationName
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id($"ctl00_MainContent_ucWorkHistoryDetails_{RegID}_ucAddress_txtOrgName")).Element;
            }
        }
        public IWebElement Address1
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id($"ctl00_MainContent_ucWorkHistoryDetails_{RegID}_ucAddress_txtAddress1")).Element;
            }
        }
        public IWebElement Address2
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id($"ctl00_MainContent_ucWorkHistoryDetails_{RegID}_ucAddress_txtAddress2")).Element;
            }
        }
        public IWebElement City
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id($"ctl00_MainContent_ucWorkHistoryDetails_{RegID}_ucAddress_txtCity")).Element;
            }
        }
        public IWebElement State
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id($"ctl00_MainContent_ucWorkHistoryDetails_{RegID}_ucAddress_ddlState")).Element;
            }
        }
        public IWebElement County
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id($"ctl00_MainContent_ucWorkHistoryDetails_{RegID}_ucAddress_ddlCounty")).Element;
            }
        }
        public IWebElement PhoneExtension
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id($"ctl00_MainContent_ucWorkHistoryDetails_{RegID}_ucAddress_txtPhoneExt1")).Element;
            }
        }
        public IWebElement ZipCode
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id($"ctl00_MainContent_ucWorkHistoryDetails_{RegID}_ucAddress_nbZipFirst5")).Element;
            }
        }
        public IWebElement PhoneNumber
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id($"ctl00_MainContent_ucWorkHistoryDetails_{RegID}_ucAddress_txtPhoneNo1")).Element;
            }
        }
        public IWebElement FaxNumber1
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id($"ctl00_MainContent_ucWorkHistoryDetails_{RegID}_ucAddress_txtFaxNo1")).Element;
            }
        }
        public IWebElement ContactName
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id($"ctl00_MainContent_ucWorkHistoryDetails_{RegID}_ucAddress_txtContact")).Element;
            }
        }
        public IWebElement EmailAddress1
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id($"ctl00_MainContent_ucWorkHistoryDetails_{RegID}_ucAddress_txtEmail1")).Element;
            }
        }
        public IWebElement EmailAddress2
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id($"ctl00_MainContent_ucWorkHistoryDetails_{RegID}_ucAddress_txtEmail2")).Element;
            }
        }
        public IWebElement AdditionalInformation
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id($"ctl00_MainContent_ucWorkHistoryDetails_{RegID}_tbAdditional")).Element;
            }
        }
        public IWebElement ReasonForDeparture
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id($"ctl00_MainContent_ucWorkHistoryDetails_{RegID}_tbReasonForDepart")).Element;
            }
        }
        public IWebElement MilitaryReserve
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id($"ctl00_MainContent_ucWorkHistoryDetails_{RegID}_ddlmiltaryreserve")).Element;
            }
        }
        public IWebElement AddGapHistory
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id($"ctl00_MainContent_ucWorkHistoryDetails_{RegID}_imgAddGap")).Element;
            }
        }
        public IWebElement GapStartDate
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id($"ctl00_MainContent_ucWorkHistoryDetails_{RegID}_tbGapStartDate")).Element;
            }
        }
        public IWebElement GapEndDate
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id($"ctl00_MainContent_ucWorkHistoryDetails_{RegID}_tbGapEndDate")).Element;
            }
        }
        public IWebElement ReasonForGap
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id($"ctl00_MainContent_ucWorkHistoryDetails_{RegID}_tbGapText")).Element;
            }
        }
        #endregion

        #region Work history
        public void SelectDropDownValue(IWebElement element, String value)
        {
            SelectElement dropDownElement = new SelectElement(element);
            dropDownElement.SelectByText(value);
        }

        public void SetWorkHistory(WorkHistoryDTO info)
        {
            if (Convert.ToBoolean(info.currentEmployer))
                CurrentEmployer.Click();

            if (!string.IsNullOrEmpty(info.EmployerName))
                EmployerName.Set(info.EmployerName, true);

            if (!string.IsNullOrEmpty(info.StartDate))
                StartDate.Set(info.StartDate, true);

            if (!string.IsNullOrEmpty(info.OrganizationName))
                OrganizationName.Set(info.OrganizationName, true);

            if (!string.IsNullOrEmpty(info.Address1))
                Address1.SendKeys(info.Address1);

            if (!string.IsNullOrEmpty(info.Address2))
                Address2.Set(info.Address2, true);

            if (!string.IsNullOrEmpty(info.City))
                City.Set(info.City, true);

            if (!string.IsNullOrEmpty(info.State))
                SelectDropDownValue(State, info.State);

            if (!string.IsNullOrEmpty(info.ZipCode))
                ZipCode.Set(info.ZipCode, true);

            if (!string.IsNullOrEmpty(info.Phone))
                PhoneNumber.Set(info.Phone, true);

            if (!string.IsNullOrEmpty(info.PhoneExtension))
                PhoneExtension.Set(info.PhoneExtension, true);

            if (!string.IsNullOrEmpty(info.EmailAddress1))
                EmailAddress1.SendKeys(info.EmailAddress1);

            if (!string.IsNullOrEmpty(info.MilitaryReserve))
                SelectDropDownValue(MilitaryReserve,info.MilitaryReserve);

            if (!string.IsNullOrEmpty(info.EmployerName))
                EmployerName.Set(info.EmployerName, true);

            if (!string.IsNullOrEmpty(info.ZipCode))
                ZipCode.Set(info.ZipCode, true);

            if (!string.IsNullOrEmpty(info.EmailAddress1))
                EmailAddress1.Set(info.EmailAddress1,true);
        }

        public void SetGapHistoryDetails(WorkHistoryDTO info)
        {
            if (!string.IsNullOrEmpty(info.GapStartDate))
                GapStartDate.Set(info.GapStartDate, true);

            if (!string.IsNullOrEmpty(info.GapEndDate))
                GapEndDate.Set(info.GapEndDate, true);

            if (!string.IsNullOrEmpty(info.ReasonForGap))
                ReasonForGap.Set(info.ReasonForGap, true);

        }
        #endregion
    }
}
