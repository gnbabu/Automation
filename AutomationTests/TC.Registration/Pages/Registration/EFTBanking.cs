using OpenQA.Selenium.Support.UI;
using Selenium.BaseComponents.Pages;
using SeleniumExtensions.Extensions;
using Selenium.BaseComponents.Utilities;
using OpenQA.Selenium;
using TC.ProviderDataEntry.Models;
using SeleniumExtensions.Configurations;
using System.Reflection.Emit;

namespace TC.ProviderDataEntry.Pages.Registration
{
    public class EFTBanking : BasePage
    {
        IWebDriver webDriver;
        string RegID;
        WebDriverWait wait;
        public EFTBanking(IWebDriver webDriver, string RegID) : base(webDriver)
        {
            wait = new WebDriverWait(webDriver, TimeoutConfiguration.Element);

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
        #region EFT Banking
        public IWebElement? RegistrationTitle
        {
            get
            {
                return PageHelper.IsElementExist(webDriver, By.Id("ctl00_MainContent_ucRegistrationNavigation_lblTitle")) ?
                    webDriver.CreateSmartElement(By.Id("ctl00_MainContent_ucRegistrationNavigation_lblTitle")).Element : null;
            }
        }

        public IWebElement SupplementalPoolPayments_Yes
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id($"ctl00_MainContent_ucACHAuthorization_{RegID}_rblINTEND_TO_RECEIVE_MCC_0")).Element;
            }
        }
        public IWebElement SupplementalPoolPayments_No
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id($"ctl00_MainContent_ucACHAuthorization_{RegID}_rblINTEND_TO_RECEIVE_MCC_1")).Element;
            }
        }
        public IWebElement BankIsOutsideOfUnitedStates
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id($"ctl00_MainContent_ucACHAuthorization_{RegID}_chkBankInUS")).Element;
            }
        }
        public IWebElement AddBankingInformation
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id($"ctl00_MainContent_ucACHAuthorization_{RegID}_btnAddBankingInfo")).Element;
            }
        }
        public IWebElement FinancialInstitutionName
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id($"ctl00_MainContent_ucACHAuthorization_{RegID}_ucBankingInfo_txtBankName")).Element;
            }
        }
        public IWebElement FinancialInstitutionRoutingNumber
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id($"ctl00_MainContent_ucACHAuthorization_{RegID}_ucBankingInfo_nbABANumber")).Element;
            }
        }
        public IWebElement ConfirmFinancialInstitutionRoutingNumber
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id($"ctl00_MainContent_ucACHAuthorization_{RegID}_ucBankingInfo_nbConfirmABANumber")).Element;
            }
        }
        public IWebElement AccountNumber
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id($"ctl00_MainContent_ucACHAuthorization_{RegID}_ucBankingInfo_nbAccountNumber")).Element;
            }
        }
        public IWebElement ConfirmAccountNumber
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id($"ctl00_MainContent_ucACHAuthorization_{RegID}_ucBankingInfo_nbConfirmAccountNumber")).Element;
            }
        }
        public IWebElement CheckingType
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id($"ctl00_MainContent_ucACHAuthorization_{RegID}_ucBankingInfo_rblCheckingSavings_0")).Element;
            }
        }
        public IWebElement SavingType
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id($"ctl00_MainContent_ucACHAuthorization_{RegID}_ucBankingInfo_rblCheckingSavings_1")).Element;
            }
        }
        public IWebElement BankingInformation_Save
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id($"ctl00_MainContent_ucACHAuthorization_{RegID}_btnSave")).Element;
            }
        }
        public IWebElement BankingInformation_Cancel
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id($"ctl00_MainContent_ucACHAuthorization_{RegID}_btnCancel")).Element;
            }
        }
        public IWebElement AddEFTContact
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id($"ctl00_MainContent_ucACHAuthorization_{RegID}_btnAddEftContact")).Element;
            }
        }
        public IWebElement ProviderContactFirstName
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id($"ctl00_MainContent_ucACHAuthorization_{RegID}_ucEftContact_txtEFTContactFirstName")).Element;
            }
        }
        public IWebElement MiddleName
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id($"ctl00_MainContent_ucACHAuthorization_{RegID}_ucEftContact_txtEFTContactMiddleName")).Element;
            }
        }
        public IWebElement LastName
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id($"ctl00_MainContent_ucACHAuthorization_{RegID}_ucEftContact_txtEFTContactLastName")).Element;
            }
        }
        public IWebElement PhoneNumber
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id($"ctl00_MainContent_ucACHAuthorization_{RegID}_ucEftContact_txtPhoneNo")).Element;
            }
        }
        public IWebElement Extension
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id($"ctl00_MainContent_ucACHAuthorization_{RegID}_ucEftContact_txtPhoneExt")).Element;
            }
        }
        public IWebElement EmailAddress
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id($"ctl00_MainContent_ucACHAuthorization_{RegID}_ucEftContact_txtEmail")).Element;
            }
        }
        public IWebElement FaxNumber
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id($"ctl00_MainContent_ucACHAuthorization_{RegID}_ucEftContact_txtFaxNumber")).Element;
            }
        }
        public IWebElement EFTInformation_Save
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id($"ctl00_MainContent_ucACHAuthorization_{RegID}_btnSave")).Element;
            }
        }
        public IWebElement EFTInformation_Cancel
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id($"ctl00_MainContent_ucACHAuthorization_{RegID}_btnCancel")).Element;
            }
        }
        public IWebElement ConfirmInformation
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id($"ctl00_MainContent_ucACHAuthorization_{RegID}_chkConfirm")).Element;
            }
        }
        #endregion

        #region Functions
        public IWebElement getSuplementPoolPayments(string value)
        {
            return value == "Yes" ? SupplementalPoolPayments_Yes : SupplementalPoolPayments_No;
        }

        public IWebElement getAccountType(string value)
        {
            return value == "Checking" ? CheckingType : SavingType;
        }

        public void SelectEFTBankingOptions(EFTBankingDTO info)
        {
            if (!string.IsNullOrEmpty(info.SupplementalPoolPayments))
            {
                if (info.SupplementalPoolPayments == "Yes")
                {
                    getSuplementPoolPayments(info.SupplementalPoolPayments).Click();

                    if (Convert.ToBoolean(info.BankingOutsideOfUnitedStates))
                        BankIsOutsideOfUnitedStates.Click();
                }
                else
                {
                    getSuplementPoolPayments(info.SupplementalPoolPayments).Click();
                }
            }

         }

        public void SetBankingInformation(EFTBankingDTO info)
        {
            if (!string.IsNullOrEmpty(info.FinancialInstitutionName))
                FinancialInstitutionName.Set(info.FinancialInstitutionName, true);

            if (!string.IsNullOrEmpty(info.FinancialInstitutionRoutingNumber))
                FinancialInstitutionRoutingNumber.Set(info.FinancialInstitutionRoutingNumber, true);

            if (!string.IsNullOrEmpty(info.ConfirmFinancialInstitutionRoutingNumber))
                ConfirmFinancialInstitutionRoutingNumber.Set(info.ConfirmFinancialInstitutionRoutingNumber, true);

            if (!string.IsNullOrEmpty(info.AccountNumber))
                AccountNumber.Set(info.AccountNumber, true);

            if (!string.IsNullOrEmpty(info.ConfirmAccountNumber))
                ConfirmAccountNumber.Set(info.ConfirmAccountNumber, true);

            if (!string.IsNullOrEmpty(info.AccountType))
                getAccountType(info.AccountType).Click();
            wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementToBeClickable(BankingInformation_Save)).Click();
           
        }

        public void SetEFTInformation(EFTBankingDTO info)
        {
            if (!string.IsNullOrEmpty(info.ProviderContactFirstName))
                ProviderContactFirstName.Set(info.ProviderContactFirstName, true);

            if (!string.IsNullOrEmpty(info.MiddleName))
                MiddleName.Set(info.MiddleName, true);

            if (!string.IsNullOrEmpty(info.LastName))
                LastName.Set(info.LastName, true);

            if (!string.IsNullOrEmpty(info.PhoneNumber))
                PhoneNumber.Set(info.PhoneNumber, true);

            if (!string.IsNullOrEmpty(info.Extension))
                Extension.Set(info.Extension, true);

            if (!string.IsNullOrEmpty(info.EmailAddress))
                EmailAddress.Set(info.EmailAddress, true);

            if (!string.IsNullOrEmpty(info.FaxNumber))
                FaxNumber.Set(info.FaxNumber, true);

            wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementToBeClickable(EFTInformation_Save)).Click();
        }
        #endregion
    }
}
