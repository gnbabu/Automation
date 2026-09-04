using TC.ProviderDataEntry.Models;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using Selenium.BaseComponents.Pages;
using SeleniumExtensions.Configurations;
using SeleniumExtensions.Extensions;
using Selenium.BaseComponents.Utilities;

namespace TC.ProviderDataEntry.Pages.Registration
{
    public class Address1099:BasePage
    {
        IWebDriver webDriver;
        string RegID;
        public Address1099(IWebDriver webDriver, string RegID) : base(webDriver)
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

        #region Address 1099

        public IWebElement? RegistrationTitle
        {
            get
            {
                return Selenium.BaseComponents.Utilities.PageHelper.IsElementExist(webDriver, By.Id("ctl00_MainContent_ucRegistrationNavigation_lblTitle")) ?
                    webDriver.CreateSmartElement(By.Id("ctl00_MainContent_ucRegistrationNavigation_lblTitle")).Element : null;
            }
        }
        public IWebElement Name
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id($"ctl00_MainContent_ucForm1099Address_{RegID}_ucAddress_txt1099FormName")).Element;
            }
        }

        public IWebElement Address1
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id($"ctl00_MainContent_ucForm1099Address_{RegID}_ucAddress_txtAddress1")).Element;
            }
        }
        public IWebElement Address2
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id($"ctl00_MainContent_ucForm1099Address_{RegID}_ucAddress_txtAddress2")).Element;
            }
        }
        public IWebElement City
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id($"ctl00_MainContent_ucForm1099Address_{RegID}_ucAddress_txtCity")).Element;
            }
        }
        public IWebElement ZipCode
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id($"ctl00_MainContent_ucForm1099Address_{RegID}_ucAddress_nbZipFirst5")).Element;
            }
        }
        public IWebElement ZipCodeExt
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id($"ctl00_MainContent_ucForm1099Address_{RegID}_ucAddress_nbZipLast4")).Element;
            }
        }
        public IWebElement Phone1
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id($"ctl00_MainContent_ucForm1099Address_{RegID}_ucAddress_txtPhoneNo1")).Element;
            }
        }
        public IWebElement Phone1Ext
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id($"ctl00_MainContent_ucForm1099Address_{RegID}_ucAddress_txtPhoneExt1")).Element;
            }
        }
        public IWebElement Phone2
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id($"ctl00_MainContent_ucForm1099Address_{RegID}_ucAddress_txtPhoneNo2")).Element;
            }
        }
        public IWebElement Phone2Ext
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id($"ctl00_MainContent_ucForm1099Address_{RegID}_ucAddress_txtPhoneExt2")).Element;
            }
        }
        public IWebElement SameBillingLocation
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id($"ctl00_MainContent_ucForm1099Address_{RegID}_prov_billing")).Element;
            }
        }

        public IWebElement Email
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id($"ctl00_MainContent_ucForm1099Address_{RegID}_ucAddress_txtEmail1")).Element;
            }
        }

        public IWebElement SamePracticeLocation
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id($"ctl00_MainContent_ucForm1099Address_{RegID}_prov_Same")).Element;
            }
        }


        public IWebElement AddressConfirmationbtn
        {
            get
            {
                return webDriver.WaitUntilElementIsVisibleAndReturn(By.Id($"ctl00_MainContent_ucForm1099Address_{RegID}_ucAddress_btnConfirmAddress"), TimeoutConfiguration.Page);
            }
        }
        public IWebElement AddressConfirmationbtnCancel
        {
            get
            {
                return webDriver.WaitUntilElementIsVisibleAndReturn(By.Id($"ctl00_MainContent_ucForm1099Address_{RegID}_ucAddress_btnCancelAddressCorrection"), TimeoutConfiguration.Page);
            }
        }
       

        public void SetState(string item)
        {
            IWebElement PracticeTypeElement = webDriver.CreateSmartElement(By.Id($"ctl00_MainContent_ucForm1099Address_{RegID}_ucAddress_ddlState")).Element;
            SelectElement PracticeTypeElementDropDown = new SelectElement(PracticeTypeElement);
            PracticeTypeElementDropDown.SelectByText(item);
        }
        public void SetCounty(string item)
        {
            IWebElement PracticeTypeElement = webDriver.CreateSmartElement(By.Id($"ctl00_MainContent_ucForm1099Address_{RegID}_ucAddress_ddlCounty")).Element;
            SelectElement PracticeTypeElementDropDown = new SelectElement(PracticeTypeElement);
            PracticeTypeElementDropDown.SelectByText(item);
        }

        public IWebElement SelectTaxExempt(string taxExmpt)
        {
            switch (taxExmpt)
            {
                case "Yes":
                    return webDriver.CreateSmartElement(By.Id($"ctl00_MainContent_ucForm1099Address_{RegID}_rblTaxExempt_0")).Element;
                    break;
                case "No":
                    return webDriver.CreateSmartElement(By.Id($"ctl00_MainContent_ucForm1099Address_{RegID}_rblTaxExempt_1")).Element;
                    break;
            }
            return null;
        }

        public IWebElement SelectW9Form(string w9Form)
        {
            switch (w9Form)
            {
                case "Yes":
                    return webDriver.CreateSmartElement(By.Id($"ctl00_MainContent_ucForm1099Address_{RegID}_rbFormW9_0")).Element;
                    break;
                case "No":
                    return webDriver.CreateSmartElement(By.Id($"ctl00_MainContent_ucForm1099Address_{RegID}_rbFormW9_1")).Element;
                    break;
            }
            return null;
        }

        public IWebElement SelectForm147(string form147)
        {
            switch (form147)
            {
                case "Yes":
                    return webDriver.CreateSmartElement(By.Id($"ctl00_MainContent_ucForm1099Address_{RegID}_rblForm147_0")).Element;
                    break;
                case "No":
                    return webDriver.CreateSmartElement(By.Id($"ctl00_MainContent_ucForm1099Address_{RegID}_rblForm147_1")).Element;
                    break;
            }
            return null;
        }

        #endregion

        public void SetAddress1099Information(Adress1099 info)
        {
            if (!string.IsNullOrEmpty(info.Name))
                Name.Set(info.Name, true);

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

            if (!string.IsNullOrEmpty(info.Phone1))
                Phone1.Set(info.Phone1, true);

            if (!string.IsNullOrEmpty(info.Phone1Ext))
                Phone1Ext.Set(info.Phone1Ext, true);

            if (!string.IsNullOrEmpty(info.Phone2))
                Phone2.Set(info.Phone2, true);

            if (!string.IsNullOrEmpty(info.Phone2Ext))
                Phone2Ext.Set(info.Phone2Ext, true);

            if (!string.IsNullOrEmpty(info.Email))
                Email.Set(info.Email, true);

            if (!string.IsNullOrEmpty(info.TaxExempt))
               SelectTaxExempt(info.TaxExempt).Click();

            if (!string.IsNullOrEmpty(info.W9Form))
                SelectW9Form(info.W9Form).Click();

            if (!string.IsNullOrEmpty(info.Form147))
                SelectForm147(info.Form147).Click();

            if (!string.IsNullOrEmpty(info.Zipcode))
                ZipCode.Set(info.Zipcode, true);

            if (!string.IsNullOrEmpty(info.County))
                SetCounty(info.County);
        }
    }
}
