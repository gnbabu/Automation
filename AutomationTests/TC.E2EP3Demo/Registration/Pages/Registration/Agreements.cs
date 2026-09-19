using System;
using OpenQA.Selenium.Support.UI;
using Selenium.BaseComponents.Pages;
using SeleniumExtensions.Extensions;
using Selenium.BaseComponents.Utilities;
using OpenQA.Selenium;
using TC.ProviderDataEntry.Models;
using SeleniumExtensions.Configurations;
using Selenium.BaseComponents.Data;

namespace TC.ProviderDataEntry.Pages.Registration
{
    public class Agreements : BasePage
    {
        IWebDriver webDriver;
        string RegID;
        WebDriverWait wait;
        public Agreements(IWebDriver webDriver, string RegID) : base(webDriver)
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

        #region Agreements
        public IWebElement? RegistrationTitle
        {
            get
            {
                return PageHelper.IsElementExist(webDriver, By.Id("ctl00_MainContent_ucRegistrationNavigation_lblTitle")) ?
                    webDriver.CreateSmartElement(By.Id("ctl00_MainContent_ucRegistrationNavigation_lblTitle")).Element : null;
            }
        }

        public IWebElement IntialTermsAndConditions
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id($"ctl00_MainContent_ucAgreements_{RegID}_chkPA01")).Element;
            }
        }
        public IWebElement FullTermsAndConditions
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id($"ctl00_MainContent_ucAgreements_{RegID}_chkPA02")).Element;
            }
        }
        public IWebElement MedicaidPrioviderProvisionCheck
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id($"ctl00_MainContent_ucAgreements_{RegID}_chkPA03")).Element;
            }
        }
        public IWebElement Voluntarilysurrendered_No
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id($"ctl00_MainContent_ucAgreements_{RegID}_rptAgreementQuestions_ctl01_rblConfirmQuestion_0")).Element;
            }
        }
        public IWebElement Voluntarilysurrendered_Yes
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id($"ctl00_MainContent_ucAgreements_{RegID}_rptAgreementQuestions_ctl01_rblConfirmQuestion_1")).Element;
            }
        }
        public IWebElement Voluntarilysurrendered_Comments
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id($"ctl00_MainContent_ucAgreements_{RegID}_rptAgreementQuestions_ctl01_txtResponseComment")).Element;
            }
        }
        public IWebElement InVoluntarilySuspended_Yes
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id($"ctl00_MainContent_ucAgreements_{RegID}_rptAgreementQuestions_ctl02_rblConfirmQuestion_1")).Element;
            }
        }
        public IWebElement InVoluntarilySuspended_No
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id($"ctl00_MainContent_ucAgreements_{RegID}_rptAgreementQuestions_ctl02_rblConfirmQuestion_0")).Element;
            }
        }
        public IWebElement InVoluntarilySuspended_Comments
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id($"ctl00_MainContent_ucAgreements_{RegID}_rptAgreementQuestions_ctl02_txtResponseComment")).Element;
            }
        }
        public IWebElement Resignfromaninternship_Yes
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id($"ctl00_MainContent_ucAgreements_{RegID}_rptAgreementQuestions_ctl03_rblConfirmQuestion_1")).Element;
            }
        }
        public IWebElement Resignfromaninternship_No
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id($"ctl00_MainContent_ucAgreements_{RegID}_rptAgreementQuestions_ctl03_rblConfirmQuestion_0")).Element;
            }
        }
        public IWebElement Resignfromaninternship_Comments
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id($"ctl00_MainContent_ucAgreements_{RegID}_rptAgreementQuestions_ctl03_txtResponseComment")).Element;
            }
        }
        public IWebElement InsuranceCancelledOrSuspended_Yes
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id($"ctl00_MainContent_ucAgreements_{RegID}_rptAgreementQuestions_ctl04_rblConfirmQuestion_1")).Element;
            }
        }
        public IWebElement InsuranceCancelledOrSuspended_No
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id($"ctl00_MainContent_ucAgreements_{RegID}_rptAgreementQuestions_ctl04_rblConfirmQuestion_0")).Element;
            }
        }
        public IWebElement InsuranceCancelledOrSuspended_Comments
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id($"ctl00_MainContent_ucAgreements_{RegID}_rptAgreementQuestions_ctl04_txtResponseComment")).Element;
            }
        }
        public IWebElement InfoReportedToNPDB_Yes
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id($"ctl00_MainContent_ucAgreements_{RegID}_rptAgreementQuestions_ctl05_rblConfirmQuestion_1")).Element;
            }
        }
        public IWebElement InfoReportedToNPDB_No
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id($"ctl00_MainContent_ucAgreements_{RegID}_rptAgreementQuestions_ctl05_rblConfirmQuestion_0")).Element;
            }
        }
        public IWebElement InfoReportedToNPDB_Comments
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id($"ctl00_MainContent_ucAgreements_{RegID}_rptAgreementQuestions_ctl05_txtResponseComment")).Element;
            }
        }
        public IWebElement DirectOrIndirectOwnerShip_Yes
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id($"ctl00_MainContent_ucAgreements_{RegID}_rptIndividualQuestions_ctl01_rblConfirmIndividualQuestion_1")).Element;
            }
        }
        public IWebElement DirectOrIndirectOwnerShip_No
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id($"ctl00_MainContent_ucAgreements_{RegID}_rptIndividualQuestions_ctl01_rblConfirmIndividualQuestion_0")).Element;
            }
        }
        public IWebElement DirectOrIndirectOwnerShip_Comments
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id($"ctl00_MainContent_ucAgreements_{RegID}_rptIndividualQuestions_ctl01_txtIndividualResponseComment")).Element;
            }
        }
        public IWebElement ConvictedOfCriminalOffense_Yes
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id($"ctl00_MainContent_ucAgreements_{RegID}_rptIndividualQuestions_ctl02_rblConfirmIndividualQuestion_1")).Element;
            }
        }
        public IWebElement ConvictedOfCriminalOffense_No
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id($"ctl00_MainContent_ucAgreements_{RegID}_rptIndividualQuestions_ctl02_rblConfirmIndividualQuestion_0")).Element;
            }
        }
        public IWebElement ConvictedOfCriminalOffense_Comments
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id($"ctl00_MainContent_ucAgreements_{RegID}_rptIndividualQuestions_ctl02_txtIndividualResponseComment")).Element;
            }
        }
        public IWebElement ConvictedViolenceOfLaw_Yes
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id($"ctl00_MainContent_ucAgreements_{RegID}_rptIndividualQuestions_ctl03_rblConfirmIndividualQuestion_1")).Element;
            }
        }
        public IWebElement ConvictedViolenceOfLaw_No
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id($"ctl00_MainContent_ucAgreements_{RegID}_rptIndividualQuestions_ctl03_rblConfirmIndividualQuestion_0")).Element;
            }
        }

        public IWebElement SanctionIndividual_No
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id($"ctl00_MainContent_ucAgreements_{RegID}_rptIndividualQuestions_ctl04_rblConfirmIndividualQuestion_0")).Element;
            }
        }

        public IWebElement SanctionIndividual_Yes
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id($"ctl00_MainContent_ucAgreements_{RegID}_rptIndividualQuestions_ctl04_rblConfirmIndividualQuestion_1")).Element;
            }
        }


        #region Individual Provider Questions

        public IWebElement IndividualProviderQuestions_1_Yes
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id($"ctl00_MainContent_ucAgreements_{RegID}_rptIndividualQuestions_ctl01_rblConfirmIndividualQuestion_1")).Element;
            }
        }
        public IWebElement IndividualProviderQuestions_1_No
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id($"ctl00_MainContent_ucAgreements_{RegID}_rptIndividualQuestions_ctl01_rblConfirmIndividualQuestion_0")).Element;
            }
        }

        public IWebElement IndividualProviderQuestions_2_Yes
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id($"ctl00_MainContent_ucAgreements_{RegID}_rptIndividualQuestions_ctl02_rblConfirmIndividualQuestion_1")).Element;
            }
        }
        public IWebElement IndividualProviderQuestions_2_No
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id($"ctl00_MainContent_ucAgreements_{RegID}_rptIndividualQuestions_ctl02_rblConfirmIndividualQuestion_0")).Element;
            }
        }

        public IWebElement IndividualProviderQuestions_3_Yes
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id($"ctl00_MainContent_ucAgreements_{RegID}_rptIndividualQuestions_ctl03_rblConfirmIndividualQuestion_1")).Element;
            }
        }
        public IWebElement IndividualProviderQuestions_3_No
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id($"ctl00_MainContent_ucAgreements_{RegID}_rptIndividualQuestions_ctl03_rblConfirmIndividualQuestion_0")).Element;
            }
        }

        public IWebElement IndividualProviderQuestions_4_Yes
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id($"ctl00_MainContent_ucAgreements_{RegID}_rptIndividualQuestions_ctl04_rblConfirmIndividualQuestion_1")).Element;
            }
        }
        public IWebElement IndividualProviderQuestions_4_No
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id($"ctl00_MainContent_ucAgreements_{RegID}_rptIndividualQuestions_ctl04_rblConfirmIndividualQuestion_0")).Element;
            }
        }


        #endregion



        public IWebElement ConvictedViolenceOfLaw_Comments
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id($"ctl00_MainContent_ucAgreements_{RegID}_rptIndividualQuestions_ctl03_txtIndividualResponseComment")).Element;
            }
        }
        public IWebElement ProviderAgreementAttestation
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id($"ctl00_MainContent_ucAgreements_{RegID}_chkPA07")).Element;
            }
        }
        public IWebElement NameOfPerson
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id($"ctl00_MainContent_ucAgreements_{RegID}_txtAttester")).Element;
            }
        }
        public IWebElement Password
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id($"ctl00_MainContent_ucAgreements_{RegID}_txtPassword")).Element;
            }
        }
        public IWebElement SaveButton
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id($"ctl00_MainContent_ucAgreements_{RegID}_btnSaveSignature")).Element;
            }
        }
        public IWebElement ReviewModelPopup
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id($"ctl00_MainContent_pnlModal")).Element;
            }
        }

        public IWebElement ReviewModelPopupOK
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id($"ctl00_MainContent_btnModalOk")).Element;
            }
        }




        #endregion

        #region Functions
        public IWebElement getVoluntarilysurrendered(string value)
        {
            return value == "Yes" ? Voluntarilysurrendered_Yes : Voluntarilysurrendered_No;
        }

        public IWebElement getInVoluntarilySuspended(string value)
        {
            return value == "Yes" ? InVoluntarilySuspended_Yes : InVoluntarilySuspended_No;
        }
        public IWebElement getResignfromaninternship(string value)
        {
            return value == "Yes" ? Resignfromaninternship_Yes : Resignfromaninternship_No;
        }

        public IWebElement getInsuranceCancelledOrSuspended(string value)
        {
            return value == "Yes" ? InsuranceCancelledOrSuspended_Yes : InsuranceCancelledOrSuspended_No;
        }
        public IWebElement getInfoReportedToNPDB(string value)
        {
            return value == "Yes" ? InfoReportedToNPDB_Yes : InfoReportedToNPDB_No;
        }

        public IWebElement getDirectOrIndirectOwnerShip(string value)
        {
            return value == "Yes" ? DirectOrIndirectOwnerShip_Yes : DirectOrIndirectOwnerShip_No;
        }
        public IWebElement getConvictedOfCriminalOffense(string value)
        {
            return value == "Yes" ? ConvictedOfCriminalOffense_Yes : ConvictedOfCriminalOffense_No;
        }
        public IWebElement getConvictedViolenceOfLaw(string value)
        {
            return value == "Yes" ? ConvictedViolenceOfLaw_Yes : ConvictedViolenceOfLaw_No;
        }
        public IWebElement getSanction(string value)
        {
            return value == "Yes" ? SanctionIndividual_Yes : SanctionIndividual_No;
        }

        public IWebElement IndividualProviderQuestions_1(string value)
        {
            return value == "Yes" ? IndividualProviderQuestions_1_Yes : IndividualProviderQuestions_1_No;
        }

        public IWebElement IndividualProviderQuestions_2(string value)
        {
            return value == "Yes" ? IndividualProviderQuestions_2_Yes : IndividualProviderQuestions_2_No;
        }

        public IWebElement IndividualProviderQuestions_3(string value)
        {
            return value == "Yes" ? IndividualProviderQuestions_3_Yes : IndividualProviderQuestions_3_No;
        }

        public IWebElement IndividualProviderQuestions_4(string value)
        {
            return value == "Yes" ? IndividualProviderQuestions_4_Yes : IndividualProviderQuestions_4_No;
        }

        public void SetAgreemntsInformation(AgreementsDTO info)
        {
            if (Convert.ToBoolean(info.MedicaidPrioviderIntialTermsAndConditions))
                IntialTermsAndConditions.Click();

            if (Convert.ToBoolean(info.MedicaidPrioviderFullTermsAndConditions))
                FullTermsAndConditions.Click();

            if (Convert.ToBoolean(info.MedicaidPrioviderProvisionCheck))
                MedicaidPrioviderProvisionCheck.Click();

            if (!string.IsNullOrEmpty(info.Voluntarilysurrendered))
                getVoluntarilysurrendered(info.Voluntarilysurrendered).Click();

            if (!string.IsNullOrEmpty(info.Voluntarilysurrendered_Comments))
                Voluntarilysurrendered_Comments.Set(info.Voluntarilysurrendered_Comments, true);

            if (!string.IsNullOrEmpty(info.InVoluntarilySuspended))
                getInVoluntarilySuspended(info.InVoluntarilySuspended).Click();

            if (!string.IsNullOrEmpty(info.InVoluntarilySuspended_Comments))
                InVoluntarilySuspended_Comments.Set(info.InVoluntarilySuspended_Comments, true);

            if (!string.IsNullOrEmpty(info.Resignfromaninternship))
                getResignfromaninternship(info.Resignfromaninternship).Click();

            if (!string.IsNullOrEmpty(info.Resignfromaninternship_Comments))
                Resignfromaninternship_Comments.Set(info.Resignfromaninternship_Comments, true);

            if (!string.IsNullOrEmpty(info.InsuranceCancelledOrSuspended))
                getInsuranceCancelledOrSuspended(info.InsuranceCancelledOrSuspended).Click();

            if (!string.IsNullOrEmpty(info.InsuranceCancelledOrSuspended_Comments))
                InsuranceCancelledOrSuspended_Comments.Set(info.InsuranceCancelledOrSuspended_Comments, true);

            if (!string.IsNullOrEmpty(info.InfoReportedToNPDB))
                getInfoReportedToNPDB(info.InfoReportedToNPDB).Click();

            if (!string.IsNullOrEmpty(info.InfoReportedToNPDB_Comments))
                InfoReportedToNPDB_Comments.Set(info.InfoReportedToNPDB_Comments, true);

            if (!string.IsNullOrEmpty(info.DirectOrIndirectOwnerShip))
                getDirectOrIndirectOwnerShip(info.DirectOrIndirectOwnerShip).Click();

            if (!string.IsNullOrEmpty(info.DirectOrIndirectOwnerShip_Comments))
                DirectOrIndirectOwnerShip_Comments.Set(info.DirectOrIndirectOwnerShip_Comments, true);

            if (!string.IsNullOrEmpty(info.ConvictedOfCriminalOffense))
                getConvictedOfCriminalOffense(info.ConvictedOfCriminalOffense).Click();

            if (!string.IsNullOrEmpty(info.ConvictedOfCriminalOffense_Comments))
                ConvictedOfCriminalOffense_Comments.Set(info.ConvictedOfCriminalOffense_Comments, true);

            if (!string.IsNullOrEmpty(info.ConvictedViolenceOfLaw))
                getConvictedViolenceOfLaw(info.ConvictedViolenceOfLaw).Click();

            if (!string.IsNullOrEmpty(info.ConvictedViolenceOfLaw_Comments))
                ConvictedViolenceOfLaw_Comments.Set(info.ConvictedViolenceOfLaw_Comments, true);

            if (!string.IsNullOrEmpty(info.SanctionIndividual_No))
                getSanction(info.SanctionIndividual_No).Click();

            if (Convert.ToBoolean(info.ProviderAgreementAttestation))
                ProviderAgreementAttestation.Click();

            if (!string.IsNullOrEmpty(info.NameOfThePersonAttesting))
                NameOfPerson.Set(info.NameOfThePersonAttesting, true);



            //if (!string.IsNullOrEmpty(UserCredentials.PasswordGenerator.GetPassword(Users.CurrentEnvironment)))
            //    Password.Set(UserCredentials.PasswordGenerator.GetPassword(Users.CurrentEnvironment), true);

        }

        public void SetAgreemntsInformationForAnestesia(AgreementsDTO info)
        {
            if (Convert.ToBoolean(info.MedicaidPrioviderIntialTermsAndConditions))
                IntialTermsAndConditions.Click();

            if (Convert.ToBoolean(info.MedicaidPrioviderFullTermsAndConditions))
                FullTermsAndConditions.Click();

            if (Convert.ToBoolean(info.MedicaidPrioviderProvisionCheck))
                MedicaidPrioviderProvisionCheck.Click();

            if (!string.IsNullOrEmpty(info.IndividualProviderQuestion_1))
                IndividualProviderQuestions_1(info.IndividualProviderQuestion_1).Click();

            if (!string.IsNullOrEmpty(info.IndividualProviderQuestion_2))
                IndividualProviderQuestions_2(info.IndividualProviderQuestion_2).Click();

            if (!string.IsNullOrEmpty(info.IndividualProviderQuestion_3))
                IndividualProviderQuestions_3(info.IndividualProviderQuestion_3).Click();

            if (!string.IsNullOrEmpty(info.IndividualProviderQuestion_4))
                IndividualProviderQuestions_4(info.IndividualProviderQuestion_4).Click();

            if (Convert.ToBoolean(info.ProviderAgreementAttestation))
                ProviderAgreementAttestation.Click();

            if (!string.IsNullOrEmpty(info.NameOfThePersonAttesting))
                NameOfPerson.Set(info.NameOfThePersonAttesting, true);

        }

        #endregion
    }
}
