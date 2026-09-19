using TC.ProviderDataEntry.Models;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using Selenium.BaseComponents.Pages;
using SeleniumExtensions.Configurations;
using SeleniumExtensions.Extensions;
using Selenium.BaseComponents.Utilities;

namespace TC.ProviderDataEntry.Pages.Registration
{
    public class CorrespondenceAddress:BasePage
    {
        IWebDriver webDriver;
        string RegID;
        public CorrespondenceAddress(IWebDriver webDriver, string RegID) : base(webDriver)
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

        #region Billing & Payment Address

        public IWebElement? RegistrationTitle
        {
            get
            {
                return Selenium.BaseComponents.Utilities.PageHelper.IsElementExist(webDriver, By.Id("ctl00_MainContent_ucRegistrationNavigation_lblTitle")) ?
                    webDriver.CreateSmartElement(By.Id("ctl00_MainContent_ucRegistrationNavigation_lblTitle")).Element : null;
            }
        }
        public IWebElement FirstName
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id($"ctl00_MainContent_ucCorrespondenceAddress_{RegID}_ucAddress_txtFirst")).Element;
            }
        }
        public IWebElement MiddleName
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id($"ctl00_MainContent_ucCorrespondenceAddress_{RegID}_ucAddress_txtMiddle")).Element;
            }
        }
        public IWebElement LastName
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id($"ctl00_MainContent_ucCorrespondenceAddress_{RegID}_ucAddress_txtLast")).Element;
            }
        }
        public IWebElement SameLocation
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id($"ctl00_MainContent_ucCorrespondenceAddress_{RegID}_prov_Same")).Element;
            }
        }
        public IWebElement OverrideAddressValidation
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id($"ctl00_MainContent_ucCorrespondenceAddress_{RegID}_prov_override")).Element;
            }
        }
        
        public IWebElement Address1
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id($"ctl00_MainContent_ucCorrespondenceAddress_{RegID}_ucAddress_txtAddress1")).Element;
            }
        }
        public IWebElement Address2
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id($"ctl00_MainContent_ucCorrespondenceAddress_{RegID}_ucAddress_txtAddress2")).Element;
            }
        }
        public IWebElement City
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id($"ctl00_MainContent_ucCorrespondenceAddress_{RegID}_ucAddress_txtCity")).Element;
            }
        }
        public IWebElement ZipCode
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id($"ctl00_MainContent_ucCorrespondenceAddress_{RegID}_ucAddress_nbZipFirst5")).Element;
            }
        }
        public IWebElement ZipCodeExt
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id($"ctl00_MainContent_ucCorrespondenceAddress_{RegID}_ucAddress_nbZipLast4")).Element;
            }
        }
        public IWebElement Phone1
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id($"ctl00_MainContent_ucCorrespondenceAddress_{RegID}_ucAddress_txtPhoneNo1")).Element;
            }
        }
        public IWebElement Phone1Ext
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id($"ctl00_MainContent_ucCorrespondenceAddress_{RegID}_ucAddress_txtPhoneExt1")).Element;
            }
        }
        public IWebElement Phone2
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id($"ctl00_MainContent_ucCorrespondenceAddress_{RegID}_ucAddress_txtPhoneNo2")).Element;
            }
        }
        public IWebElement Phone2Ext
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id($"ctl00_MainContent_ucCorrespondenceAddress_{RegID}_ucAddress_txtPhoneExt2")).Element;
            }
        }
        public IWebElement EmailAddress1
        {
            get
            {
               return  webDriver.CreateSmartElement(By.Id($"ctl00_MainContent_ucCorrespondenceAddress_{RegID}_ucAddress_txtEmail1")).Element;
            }
        }

        public IWebElement AddressConfirmationbtn
        {
            get
            {
                return webDriver.WaitUntilElementIsVisibleAndReturn(By.Id($"ctl00_MainContent_ucCorrespondenceAddress_{RegID}_ucAddress_btnConfirmAddress"), TimeoutConfiguration.Page);
            }
        }

        public void SetState(string item)
        {
            IWebElement PracticeTypeElement = webDriver.CreateSmartElement(By.Id($"ctl00_MainContent_ucCorrespondenceAddress_{RegID}_ucAddress_ddlState")).Element;
            SelectElement PracticeTypeElementDropDown = new SelectElement(PracticeTypeElement);
            PracticeTypeElementDropDown.SelectByText(item);
            Thread.Sleep(2000);
        }
        public void SetCounty(string item)
        {
            IWebElement PracticeTypeElement = webDriver.CreateSmartElement(By.Id($"ctl00_MainContent_ucCorrespondenceAddress_{RegID}_ucAddress_ddlCounty")).Element;
            SelectElement PracticeTypeElementDropDown = new SelectElement(PracticeTypeElement);
            PracticeTypeElementDropDown.SelectByText(item);
        }

        public IWebElement SameAsPracticeLocation
        {
            get
            {
                return webDriver.WaitUntilElementIsVisibleAndReturn(By.Id($"ctl00_MainContent_ucCorrespondenceAddress_{RegID}_prov_Same"), TimeoutConfiguration.Page);
            }
        }

        #endregion

        #region Functions

        public void SetCorrespnodencAe(CorrespondenceAddressDTO info)
        {
            if (!string.IsNullOrEmpty(info.FirstName))
                FirstName.Set(info.FirstName, true);

            if (!string.IsNullOrEmpty(info.MiddleName))
                MiddleName.Set(info.MiddleName, true);

            if (!string.IsNullOrEmpty(info.LastName))
                LastName.Set(info.LastName, true);

            if (!string.IsNullOrEmpty(info.Address1))
                Address1.Set(info.Address1, true);

            if (!string.IsNullOrEmpty(info.Address2))
                Address2.Set(info.Address2, true);

            if (!string.IsNullOrEmpty(info.City))
                City.Set(info.City, true);

            if (!string.IsNullOrEmpty(info.State))
                SetState(info.State);
            Thread.Sleep(3000);

           if (!string.IsNullOrEmpty(info.Zipcode))
                ZipCode.Set(info.Zipcode, true);

            if (!string.IsNullOrEmpty(info.ZipcodeExt))
                ZipCodeExt.Set(info.ZipcodeExt, true);

            Thread.Sleep(1000);
            if (!string.IsNullOrEmpty(info.Phone1))
                Phone1.SendKeys(info.Phone1);

            Thread.Sleep(1000);
            if (!string.IsNullOrEmpty(info.Phone1Ext))
                Phone1Ext.Set(info.Phone1Ext, true);

            if (!string.IsNullOrEmpty(info.Phone2))
                Phone2.Set(info.Phone2, true);

            if (!string.IsNullOrEmpty(info.Phone2Ext))
                Phone2Ext.Set(info.Phone2Ext, true);

            Thread.Sleep(1000);
            if (!string.IsNullOrEmpty(info.EmailAddress1))
                EmailAddress1.SendKeys(info.EmailAddress1);
            Thread.Sleep(1000);

            if (!string.IsNullOrEmpty(info.County))
                SetCounty(info.County);
        }

        #endregion
    }
}
