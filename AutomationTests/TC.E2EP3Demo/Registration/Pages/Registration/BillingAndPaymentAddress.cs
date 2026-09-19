using OpenQA.Selenium;
using Selenium.BaseComponents.Pages;
using SeleniumExtensions.Extensions;
using Selenium.BaseComponents.Utilities;
using TC.ProviderDataEntry.Models;
using OpenQA.Selenium.Support.UI;
using SeleniumExtensions.Configurations;

namespace TC.ProviderDataEntry.Pages.Registration
{
    public class BillingAndPaymentAddress : BasePage
    {
        IWebDriver webDriver;
        string RegID;
        public BillingAndPaymentAddress(IWebDriver webDriver, string RegID) : base(webDriver)
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
        public IWebElement PrimaryContactFirstName
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id($"ctl00_MainContent_ucBillingPaymentAddress_{RegID}_ucAddress_txtFirst")).Element;
            }
        }
        public IWebElement PrimaryContactLastName
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id($"ctl00_MainContent_ucBillingPaymentAddress_{RegID}_ucAddress_txtLast")).Element;
            }
        }
        public IWebElement Title
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id($"ctl00_MainContent_ucBillingPaymentAddress_{RegID}_ucAddress_txtTitle")).Element;
            }
        }
        public IWebElement Address1
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id($"ctl00_MainContent_ucBillingPaymentAddress_{RegID}_ucAddress_txtAddress1")).Element;
            }
        }
        public IWebElement Address2
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id($"ctl00_MainContent_ucBillingPaymentAddress_{RegID}_ucAddress_txtAddress2")).Element;
            }
        }
        public IWebElement City
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id($"ctl00_MainContent_ucBillingPaymentAddress_{RegID}_ucAddress_txtCity")).Element;
            }
        }
        public IWebElement ZipCode
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id($"ctl00_MainContent_ucBillingPaymentAddress_{RegID}_ucAddress_nbZipFirst5")).Element;
            }
        }
        public IWebElement ZipCodeExt
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id($"ctl00_MainContent_ucBillingPaymentAddress_{RegID}_ucAddress_nbZipLast4")).Element;
            }
        }
        public IWebElement Phone1
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id($"ctl00_MainContent_ucBillingPaymentAddress_{RegID}_ucAddress_txtPhoneNo1")).Element;
            }
        }
        public IWebElement Phone1Ext
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id($"ctl00_MainContent_ucBillingPaymentAddress_{RegID}_ucAddress_txtPhoneExt1")).Element;
            }
        }
        public IWebElement Phone2
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id($"ctl00_MainContent_ucBillingPaymentAddress_{RegID}_ucAddress_txtPhoneNo2")).Element;
            }
        }
        public IWebElement Phone2Ext
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id($"ctl00_MainContent_ucBillingPaymentAddress_{RegID}_ucAddress_txtPhoneExt2")).Element;
            }
        }
        public IWebElement EmailAddress1
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id($"ctl00_MainContent_ucBillingPaymentAddress_{RegID}_ucAddress_txtEmail1")).Element;
            }
        }
        public IWebElement AddressConfirmationbtn
        {
            get
            {
                return webDriver.WaitUntilElementIsVisibleAndReturn(By.Id($"ctl00_MainContent_ucBillingPaymentAddress_{RegID}_ucAddress_btnConfirmAddress"), TimeoutConfiguration.Page);
            }
        }

        public IWebElement SameAsPracticeLocation
        {
            get
            {
                return webDriver.WaitUntilElementIsVisibleAndReturn(By.Id($"ctl00_MainContent_ucBillingPaymentAddress_{RegID}_prov_Same"), TimeoutConfiguration.Page);
            }
        }



        public void SetState(string item)
        {
            IWebElement PracticeTypeElement = webDriver.CreateSmartElement(By.Id($"ctl00_MainContent_ucBillingPaymentAddress_{RegID}_ucAddress_ddlState")).Element;
            SelectElement PracticeTypeElementDropDown = new SelectElement(PracticeTypeElement);
            PracticeTypeElementDropDown.SelectByText(item);
        }
        public void SetCounty(string item)
        {
            IWebElement PracticeTypeElement = webDriver.CreateSmartElement(By.Id($"ctl00_MainContent_ucBillingPaymentAddress_{RegID}_ucAddress_ddlCounty")).Element;
            SelectElement PracticeTypeElementDropDown = new SelectElement(PracticeTypeElement);
            PracticeTypeElementDropDown.SelectByText(item);
        }


        #endregion

        #region Functions

        public void SetBillingAddressInformation(BillingAndPaymentAddressDTO info)
        {
            if (!string.IsNullOrEmpty(info.primaryContactFirstName))
                PrimaryContactFirstName.Set(info.primaryContactFirstName);
            if (!string.IsNullOrEmpty(info.primaryContactLastName))
                PrimaryContactLastName.Set(info.primaryContactLastName);

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

            if (!string.IsNullOrEmpty(info.Phone1Ext))
                Phone1Ext.Set(info.Phone1Ext, true);

            if (!string.IsNullOrEmpty(info.Phone2))
                Phone2.Set(info.Phone2, true);

            Thread.Sleep(1000);
            if (!string.IsNullOrEmpty(info.Phone1))
                Phone1.SendKeys(info.Phone1);

            Thread.Sleep(1000);

            if (!string.IsNullOrEmpty(info.Phone2Ext))
                Phone2Ext.Set(info.Phone2Ext, true);

            Thread.Sleep(1000);
            if (!string.IsNullOrEmpty(info.EmailAddress1))
                EmailAddress1.SendKeys(info.EmailAddress1);
            Thread.Sleep(1000);

            if (!string.IsNullOrEmpty(info.Zipcode))
                ZipCode.Set(info.Zipcode, true);

            if (!string.IsNullOrEmpty(info.County))
                SetCounty(info.County);
        }

        #endregion
    }
}
