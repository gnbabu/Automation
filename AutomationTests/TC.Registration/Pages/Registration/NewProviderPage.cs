using TC.ProviderDataEntry.Models;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using Selenium.BaseComponents.Pages;
using Selenium.BaseComponents.Utilities;
using SeleniumExtensions.Configurations;
using SeleniumExtensions.Extensions;
using static SeleniumExtras.WaitHelpers.ExpectedConditions;
using PageHelper = SdetToolbox.Pages.PageHelper;

namespace TC.ProviderDataEntry.Pages.Registration
{
    public class NewProviderPage : BasePage
    {
        public NewProviderPage(IWebDriver webDriver) : base(webDriver)
        {

        }

        public SmartElement NewProviderBtn
        {
            get
            {
                return WebDriver.CreateSmartElement(By.XPath("//button[@id='ctl00_MainContent_divNewProvider']"));
            }
        }

        public SmartElement StandardType
        {
            get
            {
                return WebDriver.CreateSmartElement(By.XPath("//button[@id='ctl00_MainContent_rptApplication_ctl00_SelectAppType']"));
            }
        }

        public SmartElement IndividualType
        {
            get
            {
                return WebDriver.CreateSmartElement(By.XPath("//button[@id='ctl00_MainContent_rptCategory_ctl00_btnCat']"));
            }
        }

        public bool isNewProviderloaded()
        {
            var element = WebDriver.CreateSmartElement(By.XPath("//*[@id='ctl00_MainContent_div10DayMessage']"));
            return element.Element != null && element.Element.Displayed;
        }

        #region New provider Information

        public IWebElement Firstname
        {
            get
            {
                return WebDriver.CreateSmartElement(By.Id("ctl00_MainContent_txtFirstName")).Element;
            }
        }

        public IWebElement MiddleName
        {
            get
            {
                return WebDriver.CreateSmartElement(By.Id("ctl00_MainContent_txtMI")).Element;
            }
        }


        public IWebElement LastName
        {
            get
            {
                return WebDriver.CreateSmartElement(By.Id("ctl00_MainContent_txtLastName")).Element;
            }
        }


        public IWebElement TaxID
        {
            get
            {
                return WebDriver.CreateSmartElement(By.Id("ctl00_MainContent_txtTaxID")).Element;
            }
        }

        public IWebElement NPI
        {
            get
            {
                return WebDriver.CreateSmartElement(By.Id("ctl00_MainContent_txtNPI")).Element;
            }
        }

        public IWebElement DateofBirth
        {
            get
            {
                return WebDriver.CreateSmartElement(By.Id("ctl00_MainContent_txtBirthDate")).Element;
            }
        }

        public IWebElement ZipCode
        {
            get
            {
                return WebDriver.CreateSmartElement(By.Id("ctl00_MainContent_txtZipCode")).Element;
            }
        }

        public IWebElement ZipCodeExtension
        {
            get
            {
                return WebDriver.CreateSmartElement(By.Id("ctl00_MainContent_txtZipCodeExt")).Element;
            }
        }

        public IWebElement ProviderType
        {
            get
            {
                return WebDriver.CreateSmartElement(By.XPath("//*[@id='ctl00_MainContent_ddlProviderType']")).Element;
            }
        }

        public IWebElement Taxonomy
        {
            get
            {
                return WebDriver.CreateSmartElement(By.Id("ctl00_MainContent_ddlTaxonomyNPPES")).Element;
            }
        }

        public IWebElement ProviderSavebtn
        {
            get
            {
                return WebDriver.CreateSmartElement(By.Id("ctl00_MainContent_btnSave")).Element;
            }
        }


        public SmartElement Gender(string gender)
        {
            string xpath = gender switch
            {
                "Female" => "//*[@id=\"ctl00_MainContent_ddlGender\"]/option[2]",
                "Male" => "//*[@id=\"ctl00_MainContent_ddlGender\"]/option[3]",
                "unknown" => "//*[@id=\"ctl00_MainContent_ddlGender\"]/option[4]",
                "Undisclosed" => "//*[@id=\"ctl00_MainContent_ddlGender\"]/option[5]",
                "Unspecified" => "//*[@id=\"ctl00_MainContent_ddlGender\"]/option[2]",
                _ => "//*[@id=\"ctl00_MainContent_ddlGender\"]/option[2]"
            };
            return WebDriver.CreateSmartElement(By.XPath(xpath));
        }

        public void WaitUntilTaxonomyVisible()
        {
            PageHelper.WaitUntilElementIsVisible(WebDriver, By.Id("ctl00_MainContent_ddlTaxonomyNPPES"), TimeoutConfiguration.Element);
        }

        public void WaitUntilLoaderDisappear()
        {
            PageHelper.WaitUntilElementNotAvailable(WebDriver, By.Id("ctl00_MainContent_imgSaving"), TimeoutConfiguration.Element);
        }

        public void WaitUntilImgProgressDisappear()
        {
            PageHelper.WaitUntilElementNotAvailable(WebDriver, By.Id("imgProgress"), TimeoutConfiguration.Element);
        }


        public void SetProviderType(string item)
        {
            SelectElement PracticeTypeElementDropDown = new SelectElement(ProviderType);
            PracticeTypeElementDropDown.SelectByValue("64");
        }

        public void SetTaxonomy(string item)
        {
            SelectElement PracticeTypeElementDropDown = new SelectElement(Taxonomy);
            PracticeTypeElementDropDown.SelectByIndex(Convert.ToInt16(item));
        }

        #endregion

        public void SetNewProviderPage(NewProviderDTO info)
        {

            SetProviderType(info.ProviderType);

            if (!string.IsNullOrEmpty(info.FirstName))
                Firstname.Set(info.FirstName, true);

            if (!string.IsNullOrEmpty(info.MiddleName))
                MiddleName.Set(info.MiddleName, true);

            if (!string.IsNullOrEmpty(info.LastName))
                LastName.Set(info.LastName, true);

            if (!string.IsNullOrEmpty(info.TaxID))
                TaxID.Set(info.TaxID, true);

            if (!string.IsNullOrEmpty(info.NPI))
                NPI.Set(info.NPI, true);

            Gender(info.Gender).Element.Click();

            if (!string.IsNullOrEmpty(info.DateofBirth))
                DateofBirth.Set(info.DateofBirth, true);

            if (!string.IsNullOrEmpty(info.Zipcode))
            {
                if (PageHelper.IsElementExist(WebDriver, By.Id("ctl00_MainContent_txtZipCode")))
                {
                    ZipCode.Set(info.Zipcode, true);
                }
            }

            //if (!string.IsNullOrEmpty(info.ZipcodeExt))
            //{
            //    if (PageHelper.IsElementExist(WebDriver, By.Id("ctl00_MainContent_txtZipCodeExt")))
            //    {
            //        ZipCodeExtension.Set(info.ZipcodeExt, true);
            //    }
            //}


            //if (!string.IsNullOrEmpty(info.ZipcodeExt))
            //    ZipCodeExtension.Set(info.ZipcodeExt, true);

            if (!string.IsNullOrEmpty(info.FirstName))
                Firstname.Set(info.FirstName, true);

        }

    }
}
