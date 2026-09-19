using OpenQA.Selenium.Support.UI;
using Selenium.BaseComponents.Pages;
using SeleniumExtensions.Extensions;
using Selenium.BaseComponents.Utilities;
using OpenQA.Selenium;
using TC.ProviderDataEntry.Models;
using System.Security.Cryptography;

namespace TC.ProviderDataEntry.Pages.Registration
{
    public class ProfessionalLiabilityInsurancePage : BasePage
    {
        IWebDriver webDriver;
        string RegID;
        public ProfessionalLiabilityInsurancePage(IWebDriver webDriver, string RegID) : base(webDriver)
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
        #region Professional Liability Insurance
        public IWebElement? RegistrationTitle
        {
            get
            {
                return PageHelper.IsElementExist(webDriver, By.Id("ctl00_MainContent_ucRegistrationNavigation_lblTitle")) ?
                    webDriver.CreateSmartElement(By.Id("ctl00_MainContent_ucRegistrationNavigation_lblTitle")).Element : null;
            }
        }
        public IWebElement AddNewInsurance
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id($"ctl00_MainContent_ucInsurance_{RegID}_btnAddWorkItem")).Element;
            }
        }
        public IWebElement MalPracticeInsurance_Yes
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id($"ctl00_MainContent_ucInsurance_{RegID}_rblMalpracticeInsurace_0")).Element;
            }
        }
        public IWebElement MalPracticeInsurance_No
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id($"ctl00_MainContent_ucInsurance_{RegID}_rblMalpracticeInsurace_1")).Element;
            }
        }
        public IWebElement SelfInsured
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id($"ctl00_MainContent_ucInsurance_{RegID}_ddlSelfInsured")).Element;
            }
        }
        public IWebElement PolicyNumber
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id($"ctl00_MainContent_ucInsurance_{RegID}_txtPolicyNumber")).Element;
            }
        }
        public IWebElement EffectiveDate
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id($"ctl00_MainContent_ucInsurance_{RegID}_txtEffectiveDate")).Element;
            }
        }
        public IWebElement OriginalEffectiveDate
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id($"ctl00_MainContent_ucInsurance_{RegID}_txtOrginaEffDate")).Element;
            }
        }
        public IWebElement ExpirationDate
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id($"ctl00_MainContent_ucInsurance_{RegID}_txtExpirationDate")).Element;
            }
        }
        public IWebElement TypeOfCoverage
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id($"ctl00_MainContent_ucInsurance_{RegID}_ddlTypeofCoverage")).Element;
            }
        }
        public IWebElement UnlimitedCoverage
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id($"ctl00_MainContent_ucInsurance_{RegID}_ddlIsUnlimitedCoverage")).Element;
            }
        }
        public IWebElement TailCoverage
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id($"ctl00_MainContent_ucInsurance_{RegID}_ddlIsPolicyTailCoverageInclude")).Element;
            }
        }
        public IWebElement CarrierName
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id($"ctl00_MainContent_ucInsurance_{RegID}_txtCarrierName")).Element;
            }
        }
        public IWebElement Address1
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id($"ctl00_MainContent_ucInsurance_{RegID}_ucAddress_txtAddress1")).Element;
            }
        }
        public IWebElement Address2
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id($"ctl00_MainContent_ucInsurance_{RegID}_ucAddress_txtAddress2")).Element;
            }
        }
        public IWebElement City
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id($"ctl00_MainContent_ucInsurance_{RegID}_ucAddress_txtCity")).Element;
            }
        }
        public IWebElement State
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id($"ctl00_MainContent_ucInsurance_{RegID}_ucAddress_ddlState")).Element;
            }
        }
        public IWebElement County
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id($"ctl00_MainContent_ucInsurance_{RegID}_ucAddress_ddlCounty")).Element;
            }
        }
        public IWebElement Zip
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id($"ctl00_MainContent_ucInsurance_{RegID}_ucAddress_nbZipFirst5")).Element;
            }
        }
        public IWebElement PolicyHolder
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id($"ctl00_MainContent_ucInsurance_{RegID}_txtPolicyHolder")).Element;
            }
        }
        public IWebElement AmountPerOccurence
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id($"ctl00_MainContent_ucInsurance_{RegID}_txtAmtPerOccurrence")).Element;
            }
        }
        public IWebElement AmountPerAggregate
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id($"ctl00_MainContent_ucInsurance_{RegID}_txtAmtPerAggregate")).Element;
            }
        }
        public IWebElement MalpracticeReason
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id($"ctl00_MainContent_ucInsurance_{RegID}_txtMalpracticeReason")).Element;
            }
        }
        #endregion

        #region Functions
        public void SelectValuefromDropDown(IWebElement element,String value)
        {
            SelectElement dropDownElement = new SelectElement(element);
            dropDownElement.SelectByText(value);
        }
        public IWebElement geCarrytMalPracticeInsurance(String value)
        {
            return value == "Yes" ? MalPracticeInsurance_Yes : MalPracticeInsurance_No;
        }

        public void SetProfessionalLiabilityInsurance(ProfessionalLiabilityInsurance info)
        {
            if (!string.IsNullOrEmpty(info.MalpracticeInsurance))
                geCarrytMalPracticeInsurance(info.MalpracticeInsurance).Click();
            if (info.MalpracticeInsurance == "Yes")
            {
                if (!string.IsNullOrEmpty(info.SelfInsurance))
                    SelectValuefromDropDown(SelfInsured,info.SelfInsurance);

                if (!string.IsNullOrEmpty(info.PolicyNumber))
                    PolicyNumber.Set(info.PolicyNumber,true);

                if (!string.IsNullOrEmpty(info.EffectiveDate))
                    EffectiveDate.Set(info.EffectiveDate, true);

                if (!string.IsNullOrEmpty(info.OrifinalEffectiveDate))
                    OriginalEffectiveDate.Set(info.OrifinalEffectiveDate, true);

                if (!string.IsNullOrEmpty(info.ExpirationDate))
                    ExpirationDate.Set(info.ExpirationDate, true);

                if (!string.IsNullOrEmpty(info.TypeOfCoverage))
                    SelectValuefromDropDown(TypeOfCoverage,info.TypeOfCoverage);

                if (!string.IsNullOrEmpty(info.UnlimitedCoverage))
                    SelectValuefromDropDown(UnlimitedCoverage, info.UnlimitedCoverage);

                if (!string.IsNullOrEmpty(info.TailCoverage))
                    SelectValuefromDropDown(TailCoverage, info.TailCoverage);

                if (!string.IsNullOrEmpty(info.SelfInsuredName))
                    CarrierName.Set(info.SelfInsuredName, true);

                if (!string.IsNullOrEmpty(info.CarrierAddress1))
                    Address1.Set(info.CarrierAddress1, true);

                if (!string.IsNullOrEmpty(info.CarrierAddress2))
                    Address2.Set(info.CarrierAddress2, true);

                if (!string.IsNullOrEmpty(info.City))
                    City.Set(info.City, true);

                if (!string.IsNullOrEmpty(info.State))
                    SelectValuefromDropDown(State, info.State);

                if (!string.IsNullOrEmpty(info.Zip))
                    Zip.Set(info.Zip, true);

                if (!string.IsNullOrEmpty(info.CoverageAmountPerAggregrate))
                    AmountPerAggregate.Set(info.CoverageAmountPerAggregrate, true);

                if (!string.IsNullOrEmpty(info.PolicyHolder))
                    PolicyHolder.Set(info.PolicyHolder, true);

                if (!string.IsNullOrEmpty(info.CoverageAmountPerOccurance))
                    AmountPerOccurence.Set(info.CoverageAmountPerOccurance, true);

                if (!string.IsNullOrEmpty(info.County))
                    SelectValuefromDropDown(County, info.County);
            }
            else if (info.MalpracticeInsurance == "No")
            {
                if (!string.IsNullOrEmpty(info.ExplanationMalPraticeInsurance))
                    MalpracticeReason.Set(info.ExplanationMalPraticeInsurance, true);
            }
        }
        #endregion
    }
}
