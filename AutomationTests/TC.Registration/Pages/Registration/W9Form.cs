using OpenQA.Selenium.Support.UI;
using Selenium.BaseComponents.Pages;
using SeleniumExtensions.Extensions;
using Selenium.BaseComponents.Utilities;
using OpenQA.Selenium;
using TC.ProviderDataEntry.Models;
using SeleniumExtensions.Configurations;

namespace TC.ProviderDataEntry.Pages.Registration
{
    public class W9Form : BasePage
    {
        IWebDriver webDriver;
        string RegID;
        public W9Form(IWebDriver webDriver, string RegID) : base(webDriver)
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
        public IWebElement? RegistrationTitle
        {
            get
            {
                return PageHelper.IsElementExist(webDriver, By.Id("ctl00_MainContent_ucRegistrationNavigation_lblTitle")) ?
                    webDriver.CreateSmartElement(By.Id("ctl00_MainContent_ucRegistrationNavigation_lblTitle")).Element : null;
            }
        }

        public IWebElement IndividualProprietor
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id($"ctl00_MainContent_ucTaxInfo_{RegID}_rptTaxClassification_ctl01_rbTaxClass")).Element;
            }
        }
        public IWebElement C_Corporation
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id($"ctl00_MainContent_ucTaxInfo_{RegID}_rptTaxClassification_ctl02_rbTaxClass")).Element;
            }
        }
        public IWebElement S_Corporation
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id($"ctl00_MainContent_ucTaxInfo_{RegID}_rptTaxClassification_ctl03_rbTaxClass")).Element;
            }
        }
        public IWebElement PartnerShip
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id($"ctl00_MainContent_ucTaxInfo_{RegID}_rptTaxClassification_ctl04_rbTaxClass")).Element;
            }
        }
        public IWebElement TrustOrEstate
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id($"ctl00_MainContent_ucTaxInfo_{RegID}_rptTaxClassification_ctl05_rbTaxClass")).Element;
            }
        }
        public IWebElement LimitedLiabilityC_Corporation
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id($"ctl00_MainContent_ucTaxInfo_{RegID}_rptTaxClassification_ctl06_rbTaxClass")).Element;
            }
        }
        public IWebElement LimitedLiabilityS_Corporation
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id($"ctl00_MainContent_ucTaxInfo_{RegID}_rptTaxClassification_ctl07_rbTaxClass")).Element;
            }
        }
        public IWebElement LimitedLiabilityPartnerShip
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id($"ctl00_MainContent_ucTaxInfo_{RegID}_rptTaxClassification_ctl08_rbTaxClass")).Element;
            }
        }
        public IWebElement Other
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id($"ctl00_MainContent_ucTaxInfo_{RegID}_rptTaxClassification_ctl09_rbTaxClass")).Element;
            }
        }
        public IWebElement W9
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id($"ctl00_MainContent_ucTaxInfo_{RegID}_rbIndicate_0")).Element;
            }
        }
        public IWebElement Form149
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id($"ctl00_MainContent_ucTaxInfo_{RegID}_rbIndicate_1")).Element;
            }
        }
        public IWebElement FileUpload
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id($"ctl00_MainContent_ucTaxInfo_{RegID}_174_RadAsyncUpload1file0")).Element;
            }
        }
        public IWebElement FileDownload
        {
            get
            {
                return webDriver.CreateSmartElement(By.XPath("//a[contains(@id,'_LnkButtonDownload')]")).Element;
            }
        }

        //Temporary Navigation
        public IWebElement ETFBankingImage
        {
            get
            {
                return webDriver.CreateSmartElement(By.XPath("//span[text()='EFT Banking']/ancestor::span[1]/img")).Element;
            }
        }
        public IWebElement PopUp_Confirmation
        {
            get
            {
                return webDriver.CreateSmartElement(By.XPath("//span[text()='Ok']/ancestor::button")).Element;
            }
        }
        public IWebElement getFormType(string value)
        {
            return value == "W9" ? W9 : Form149;
        }

        public void SetW9FormDetails(W9FormDTO form)
        {
            switch (form.CategoryType)
            {
                case "Individual/sole proprietor":
                    IndividualProprietor.Click();
                    break;
                case "C Corporation":
                    C_Corporation.Click();
                    break;
                case "S Corporation":
                    S_Corporation.Click();
                    break;
                case "Partnership":
                    PartnerShip.Click(); break;
                case "Trust/Estate":
                    TrustOrEstate.Click(); break;
                case "Limited Liability C Corporation":
                    LimitedLiabilityC_Corporation.Click(); break;
                case ",Limited Liability S Corporation":
                    LimitedLiabilityS_Corporation.Click(); break;
                case "Limited Liability Partnership":
                    LimitedLiabilityPartnerShip.Click(); break;
                case "Other":
                    Other.Click(); break;
            }

            Thread.Sleep(3000);

            if (!string.IsNullOrEmpty(form.FormType))
                getFormType(form.FormType).Click();

            Thread.Sleep(3000);

            if (!string.IsNullOrEmpty(form.FilePath))
            {
                FileUpload.SendKeys(form.FilePath);
            }
            Console.WriteLine("Form Is uploaded Succesfully");
            Thread.Sleep(3000);
        }

        public void WaitUntilImgProgressDisappear()
        {
            PageHelper.WaitUntilElementNotAvailable(WebDriver, By.Id("imgProgress"), TimeoutConfiguration.Element);
        }
    }
}
