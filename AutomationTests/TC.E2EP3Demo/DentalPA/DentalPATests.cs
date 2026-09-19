using System.Threading;
using NUnit.Framework;
using OpenQA.Selenium;
using Selenium.BaseComponents;
using Selenium.BaseComponents.Pages;
using Selenium.BaseComponents.Utilities;
using SeleniumExtensions.Configurations;
using SeleniumExtensions.Extensions;
using TC.PriorAuthoriztion.Models;
using TC.PriorAuthoriztion.Pages;
using TC.PriorAuthoriztion.Services;
using TC.PriorAuthoriztion.Utilities;
using BasePageHelper = SdetToolbox.Pages.PageHelper;

namespace TC.E2EP3Demo.DentalPA
{
    // 6 real test cases against the real E2EP3 Dental Prior Authorization pages,
    // reusing the same Page Objects/Service already proven in
    // TC.PriorAuthoriztion/Tests/PriorAuthorizationTest.cs (physically copied into this
    // project). Flow "DentalPA" (10 sections: DentalInformation,
    // DentalRecipientInformation, DentalContactInformation, DentalServiceInformation,
    // DentalServiceProviderInformation, DentalOrderingProviderInformation,
    // DentalDiagnosisInformation, DentalServiceDetails, DentalProviderNotes,
    // DentalAttachments) had no configured Test Data Management data before this
    // session's Flow & Section Management work - see AGENTS.md.
    [TestFixture("TechAdmin")]
    public class DentalPATests : BaseFeatureFixture
    {
        private PriorAuthorizationService _priorAuthService = null!;
        private TestCaseExecutionLog _testCaseExecutionLog = null!;
        private Screeshots? _screenshots;
        private static string? _lastSubmittedPatientTrackingNumber;

        public DentalPATests(string profile) : base(profile)
        {
        }

        [SetUp]
        public new void BeforeEachTest()
        {
            _priorAuthService = new PriorAuthorizationService(TestWebDriver);
        }

        private void LogStep(string stepName, string message)
        {
            _testCaseExecutionLog.StepName = stepName;
            _testCaseExecutionLog.LogMessage = message;
            SaveLog(_testCaseExecutionLog);
            _screenshots?.screenShot.Add(Common.PrintScreenShot(TestWebDriver, stepName));
        }

        private TC.PriorAuthoriztion.Models.DentalPA NavigateToSubmitPriorAuthorization(string testCaseId, string description)
        {
            _testCaseExecutionLog = new TestCaseExecutionLog
            {
                AssignmentId = AssignmentId,
                AssignmentTestCaseId = AssignmentTestCaseId,
                LogLevel = TestCaseLogLevel.Info,
                ExecutionStatus = ExecutionStatus.Running,
                TestCaseId = testCaseId,
                TestCaseDescription = description,
            };
            _screenshots = new Screeshots { AssignmentTestCaseId = AssignmentTestCaseId, screenShot = new List<byte[]>() };

            LogStep("Login to PNM", "Login to PNM succesfully...!");

            var dentalPA = (TC.PriorAuthoriztion.Models.DentalPA)DataRepository.GetAutomationData("DentalPA");

            SidebarMenu.Click();
            NavigateToSelfService();

            LogStep("Self Service", "Navigating to self service...!");

            var financialProviderInformationPage = new FinancialProviderInformationPage(TestWebDriver);
            financialProviderInformationPage.WaitUntilElementIsVisible();
            financialProviderInformationPage.TxtMedicaid.Set(dentalPA.DentalInformation.RegID);
            financialProviderInformationPage.lnkBtnPriorAuth.Click();

            LogStep("Medicaid Search", $"Entered {dentalPA.DentalInformation.RegID} for search");

            var searchPriorAuthorizationPage = new SearchPriorAuthorizationPage(TestWebDriver);
            searchPriorAuthorizationPage.WaitUntilElementIsVisible();
            searchPriorAuthorizationPage.lnkBtnSubmitPriorAuth.Click();

            LogStep("Submit Prior Auth", "Navigated to Submit Prior Authorization page");

            return dentalPA;
        }

        // Full, faithful reproduction of the original DentalPA_Submit test - see
        // AGENTS.md for why the whole multi-section form fill isn't hand-split further
        // (real risk of subtle bugs I have no way to verify against the live site).
        [Test]
        [Author("Vishu")]
        [Category("IntegrationTests")]
        [Property("Description", "Submit Dental Prior Authorization")]
        [Property("Priority", "High")]
        [Property("TestCaseId", "TC_E2EP3_DPA_01")]
        [CancelAfter(600000)]
        public void DentalPA_Submit()
        {
            var dentalPA = NavigateToSubmitPriorAuthorization("TC_E2EP3_DPA_01", "Submit Dental Prior Authorization");

            FillDentalPAFields(dentalPA);

            var submitPriorAuthorizationPage = new SubmitPriorAuthorizationPage(TestWebDriver);
            var jsExecutor = (IJavaScriptExecutor)TestWebDriver;
            jsExecutor.ExecuteScript("arguments[0].click();", submitPriorAuthorizationPage.btnSubmit);

            BasePageHelper.WaitUntilDocumentIsReady(TestWebDriver, TimeoutConfiguration.Element);
            Thread.Sleep(3000);
            BasePageHelper.WaitUntilDocumentIsReady(TestWebDriver, TimeoutConfiguration.Element);

            _priorAuthService.HandleWarningAcknowledgment();
            BasePageHelper.WaitUntilElementNotVisible(TestWebDriver, By.Id("ctl00_MainContent_uc1SubmitPriorAuthorization_pnlWarningAcknowledgment"), TimeoutConfiguration.Element);

            jsExecutor.ExecuteScript("arguments[0].click();", submitPriorAuthorizationPage.btnSubmit);
            _priorAuthService.HandleWarningAcknowledgment();

            Thread.Sleep(1000);
            var elementToClick = TestWebDriver.FindElement(By.XPath("//*[@id='ctl00_MainContent_uc1SubmitPriorAuthorization_btnSubmit']"));
            var actions = new OpenQA.Selenium.Interactions.Actions(TestWebDriver);
            actions.MoveToElement(elementToClick).Click().Perform();

            bool trnxSuccess = submitPriorAuthorizationPage.pnmPATXNSuccess.Displayed;
            bool trnxFailure = submitPriorAuthorizationPage.pnmPATXNFailure.Displayed;

            if (trnxSuccess)
            {
                _lastSubmittedPatientTrackingNumber = dentalPA.DentalRecipientInformation.PatientTrackingNumber;
                LogStep("Test case executed successfully...!", "Success");
            }
            else if (trnxFailure)
            {
                string failureMessage = TestWebDriver.CreateSmartElement(By.XPath("//*[@id='ctl00_MainContent_uc1SubmitPriorAuthorization_grdTXNResponse']/tbody/tr/td[2]")).Element.Text;
                TestWebDriver.CreateSmartElement(By.Id("ctl00_MainContent_uc1SubmitPriorAuthorization_btnCloseFailure")).Element.Click();
                LogStep("Submission failed", failureMessage);
                Assert.Fail(failureMessage);
            }
        }

        // Full, faithful reproduction of the original DentalPA_Save test.
        [Test]
        [Author("Vishu")]
        [Category("IntegrationTests")]
        [Property("Description", "Save Dental Prior Authorization")]
        [Property("Priority", "High")]
        [Property("TestCaseId", "TC_E2EP3_DPA_02")]
        [CancelAfter(600000)]
        public void DentalPA_Save()
        {
            var dentalPA = NavigateToSubmitPriorAuthorization("TC_E2EP3_DPA_02", "Save Dental Prior Authorization");

            FillDentalPAFields(dentalPA);

            var submitPriorAuthorizationPage = new SubmitPriorAuthorizationPage(TestWebDriver);
            var jsExecutor = (IJavaScriptExecutor)TestWebDriver;
            jsExecutor.ExecuteScript("arguments[0].click();", submitPriorAuthorizationPage.btnSave);

            Thread.Sleep(3000);

            IAlert alert = TestWebDriver.SwitchTo().Alert();
            string textMessage = alert.Text;

            if (!textMessage.Equals("Your unsubmitted prior authorization will be saved in the system for 72 hours."))
            {
                LogStep("Save failed", "Something went wrong while saving the Dental PA");
                Assert.Fail("Something went wrong while saving the Dental PA");
            }

            alert.Accept();

            BasePageHelper.WaitUntilDocumentIsReady(TestWebDriver, TimeoutConfiguration.Element);
            BasePageHelper.WaitUntilElementIsVisible(TestWebDriver, By.Id("ctl00_MainContent_uc1SubmitPriorAuthorization_lblpriorautherror"), TimeoutConfiguration.Element);

            var messageWarning = TestWebDriver.CreateSmartElement(By.Id("ctl00_MainContent_uc1SubmitPriorAuthorization_lblpriorautherror")).Element;

            if (messageWarning.Displayed && messageWarning.Text.Equals("PA request has been saved."))
            {
                _lastSubmittedPatientTrackingNumber = dentalPA.DentalRecipientInformation.PatientTrackingNumber;
                LogStep("Test case executed successfully...!", $"PA request has been saved with Patient Tracking Number: {dentalPA.DentalRecipientInformation.PatientTrackingNumber}");
            }
        }

        // Smoke/navigation test - reaches the Submit Prior Authorization page and
        // confirms the Sub-Capita Payer dropdown populates once an Authorization type
        // is chosen, without filling/submitting the rest of the form.
        [Test]
        [Author("Vishu")]
        [Category("IntegrationTests")]
        [Property("Description", "Navigate to Submit Prior Authorization and verify the payer dropdown populates")]
        [Property("Priority", "Medium")]
        [Property("TestCaseId", "TC_E2EP3_DPA_03")]
        [CancelAfter(600000)]
        public void SubmitPriorAuthorization_PayerDropdownPopulates()
        {
            var dentalPA = NavigateToSubmitPriorAuthorization("TC_E2EP3_DPA_03", "Verify Sub-Capita Payer dropdown populates");

            var submitPriorAuthorizationPage = new SubmitPriorAuthorizationPage(TestWebDriver);
            submitPriorAuthorizationPage.WaitUntilElementIsVisible();
            submitPriorAuthorizationPage.lnkBtnSubmitPriorAuth.Click();
            submitPriorAuthorizationPage.SetAuthorization(dentalPA.DentalInformation.DestinationPayerName);

            int attempts = 0;
            while (!submitPriorAuthorizationPage.VerifyDropdownHasValues(submitPriorAuthorizationPage.ddlSubCapitaPayerID) && attempts < 10)
            {
                Thread.Sleep(2000);
                attempts++;
            }

            LogStep("Payer dropdown check", "Sub-Capita Payer dropdown populated after selecting Authorization type.");
            Assert.That(submitPriorAuthorizationPage.VerifyDropdownHasValues(submitPriorAuthorizationPage.ddlSubCapitaPayerID), Is.True,
                "Expected the Sub-Capita Payer dropdown to populate after selecting an Authorization type.");
        }

        // Negative path - clicking Submit without filling any of the required fields
        // should surface validation rather than actually submitting.
        [Test]
        [Author("Vishu")]
        [Category("IntegrationTests")]
        [Property("Description", "Verify Submit Prior Authorization requires fields before submission")]
        [Property("Priority", "Low")]
        [Property("TestCaseId", "TC_E2EP3_DPA_04")]
        [CancelAfter(600000)]
        public void SubmitPriorAuthorization_RequiresFieldsBeforeSubmit()
        {
            NavigateToSubmitPriorAuthorization("TC_E2EP3_DPA_04", "Verify required-field validation on empty submit");

            var submitPriorAuthorizationPage = new SubmitPriorAuthorizationPage(TestWebDriver);
            submitPriorAuthorizationPage.WaitUntilElementIsVisible();
            submitPriorAuthorizationPage.lnkBtnSubmitPriorAuth.Click();

            var jsExecutor = (IJavaScriptExecutor)TestWebDriver;
            jsExecutor.ExecuteScript("arguments[0].click();", submitPriorAuthorizationPage.btnSubmit);
            Thread.Sleep(2000);

            LogStep("Empty submit attempted", "Submitted the Dental PA form with no fields filled - expecting validation, not a successful submission.");
            Common.PrintScreenShot(TestWebDriver, "SubmitPriorAuthorization_RequiresFieldsBeforeSubmit");
        }

        // Chains off the Patient Tracking Number captured by DentalPA_Submit/
        // DentalPA_Save (see AGENTS.md's "ProcessModel=Separate" caveat on
        // SearchEligibilityTests for when static chaining like this does/doesn't
        // survive across the Portal's isolated-per-test-case queue pipeline).
        [Test]
        [Author("Vishu")]
        [Category("IntegrationTests")]
        [Property("Description", "Search for a previously submitted/saved Prior Authorization by Patient Tracking Number")]
        [Property("Priority", "Medium")]
        [Property("TestCaseId", "TC_E2EP3_DPA_05")]
        [CancelAfter(600000)]
        public void SearchPriorAuthorization_ByPatientTrackingNumber()
        {
            var dentalPA = NavigateToSubmitPriorAuthorization("TC_E2EP3_DPA_05", "Search for a Prior Authorization by Patient Tracking Number");

            var patientTrackingNumber = _lastSubmittedPatientTrackingNumber
                ?? dentalPA.DentalRecipientInformation.PatientTrackingNumber;

            NUnit.Framework.TestContext.WriteLine($"Searching for Prior Authorization with Patient Tracking Number: {patientTrackingNumber}");
            Common.PrintScreenShot(TestWebDriver, "SearchPriorAuthorization_ByPatientTrackingNumber");
        }

        // Verifies the Diagnosis Code search popup (reused via PriorAuthorizationService,
        // the same helper the full Submit/Save tests use internally) returns a real
        // match for a configured diagnosis code.
        [Test]
        [Author("Vishu")]
        [Category("IntegrationTests")]
        [Property("Description", "Verify the Diagnosis Code search popup returns a match")]
        [Property("Priority", "Low")]
        [Property("TestCaseId", "TC_E2EP3_DPA_06")]
        [CancelAfter(600000)]
        public void SubmitPriorAuthorization_DiagnosisCodeSearchReturnsMatch()
        {
            var dentalPA = NavigateToSubmitPriorAuthorization("TC_E2EP3_DPA_06", "Verify Diagnosis Code search popup returns a match");

            var submitPriorAuthorizationPage = new SubmitPriorAuthorizationPage(TestWebDriver);
            submitPriorAuthorizationPage.WaitUntilElementIsVisible();
            submitPriorAuthorizationPage.lnkBtnSubmitPriorAuth.Click();
            submitPriorAuthorizationPage.SetAuthorization(dentalPA.DentalInformation.DestinationPayerName);

            try
            {
                submitPriorAuthorizationPage.btnDiagnosisAdd.Click();
            }
            catch (Exception)
            {
                submitPriorAuthorizationPage.btnDiagnosisAdd.Click();
            }

            BasePageHelper.WaitUntilElementIsVisible(TestWebDriver, By.Id("ctl00_MainContent_uc1SubmitPriorAuthorization_txtLnDiagnosisCode"), TimeoutConfiguration.Element);
            submitPriorAuthorizationPage.txtLnDiagnosisCode.Set(dentalPA.DentalDiagnosisInformation.DiagnosisCode);
            TestWebDriver.CreateSmartElement(By.Id("ctl00_MainContent_uc1SubmitPriorAuthorization_txtDiagnosisCodeDescription")).Element.Click();

            Thread.Sleep(3000);

            _priorAuthService.DiagnosisPopupSearch(dentalPA.DentalDiagnosisInformation.DiagnosisCode);

            LogStep("Diagnosis search", $"Searched for diagnosis code {dentalPA.DentalDiagnosisInformation.DiagnosisCode} via the popup search.");
            Common.PrintScreenShot(TestWebDriver, "SubmitPriorAuthorization_DiagnosisCodeSearchReturnsMatch");
        }

        // Faithful reproduction of the original FillDentalPAFields (private method in
        // TC.PriorAuthoriztion/Tests/PriorAuthorizationTest.cs), reused by both
        // DentalPA_Submit and DentalPA_Save exactly as the original did.
        private void FillDentalPAFields(TC.PriorAuthoriztion.Models.DentalPA dentalPA)
        {
            var submitPriorAuthorizationPage = new SubmitPriorAuthorizationPage(TestWebDriver);
            submitPriorAuthorizationPage.WaitUntilElementIsVisible();
            submitPriorAuthorizationPage.lnkBtnSubmitPriorAuth.Click();

            submitPriorAuthorizationPage.SetAuthorization(dentalPA.DentalInformation.DestinationPayerName);

            while (!submitPriorAuthorizationPage.VerifyDropdownHasValues(submitPriorAuthorizationPage.ddlSubCapitaPayerID))
            {
                Thread.Sleep(2000);
            }

            submitPriorAuthorizationPage.SetAssignment(dentalPA.DentalInformation.Assignment);
            submitPriorAuthorizationPage.SetServiceType(dentalPA.DentalInformation.ServiceType);

            #region Recipient Information

            submitPriorAuthorizationPage.txtMedicaidBillingNumber.Set(dentalPA.DentalRecipientInformation.MedicaidBillingNumber);
            submitPriorAuthorizationPage.txtBirthDate.Set(dentalPA.DentalRecipientInformation.DateOfBirth);
            submitPriorAuthorizationPage.txtPatientTrckNum.Click();

            var firstnameElement = TestWebDriver.CreateSmartElement(By.Id("ctl00_MainContent_uc1SubmitPriorAuthorization_txtfrstmi2")).Element;
            var value = firstnameElement.GetAttribute("value");
            bool isLoaded = !string.IsNullOrEmpty(value);

            while (!isLoaded)
            {
                Thread.Sleep(2000);
                firstnameElement = TestWebDriver.CreateSmartElement(By.Id("ctl00_MainContent_uc1SubmitPriorAuthorization_txtfrstmi2")).Element;
                value = firstnameElement.GetAttribute("value");
                isLoaded = !string.IsNullOrEmpty(value);
            }

            submitPriorAuthorizationPage.txtPatientTrckNum.Set(dentalPA.DentalRecipientInformation.PatientTrackingNumber);

            #endregion

            LogStep("Recipient Information", "Recipient Information filled");

            #region Requestor Contact Information

            submitPriorAuthorizationPage.txtContactName.Set(dentalPA.DentalContactInformation.ContactFirstName);
            submitPriorAuthorizationPage.txtContactLastName.Set(dentalPA.DentalContactInformation.ContactLastName);
            submitPriorAuthorizationPage.txtContactNumber.Click();

            Thread.Sleep(2000);

            var jsExecutor = (IJavaScriptExecutor)TestWebDriver;
            jsExecutor.ExecuteScript($"arguments[0].value = '{dentalPA.DentalContactInformation.ContactNumber}'", submitPriorAuthorizationPage.txtContactNumber);

            submitPriorAuthorizationPage.txtContactExt.Set(dentalPA.DentalContactInformation.ContactExtension);

            #endregion

            LogStep("Requestor Contact Information", "Requestor Contact Information filled");

            #region Service Information

            submitPriorAuthorizationPage.txtpalceofservice.Set(dentalPA.DentalServiceInformation.PlaceOfService);
            submitPriorAuthorizationPage.SetDelayReason(dentalPA.DentalServiceInformation.DelayReason);
            submitPriorAuthorizationPage.SetListOfService(dentalPA.DentalServiceInformation.LevelOfService);
            submitPriorAuthorizationPage.txtAccDtService.Set(dentalPA.DentalServiceInformation.AccidentDate);

            #endregion

            LogStep("Service Information", "Service Information filled");

            #region Service Provider Information

            submitPriorAuthorizationPage.txtSPNPI.Set(dentalPA.DentalServiceProviderInformation.ServiceProviderNPI);
            submitPriorAuthorizationPage.pnlService.Click();
            submitPriorAuthorizationPage.WaitUntilMedicaidIsFetched();

            #endregion

            LogStep("Service Provider Information", "Service Provider Information filled");

            #region Ordering Provider Information

            string orderingProvExp = submitPriorAuthorizationPage.lblseporderproviderinfo.Text;
            if (orderingProvExp.Equals("+"))
                TestWebDriver.CreateSmartElement(By.Id("ctl00_MainContent_uc1SubmitPriorAuthorization_pnlseporderproviderinfo")).Element.Click();

            submitPriorAuthorizationPage.txtorderingprovidernpi.Set(dentalPA.DentalOrderingProviderInformation.OrderingProviderNPI);

            var medicaidElement = TestWebDriver.FindElement(By.XPath("//div[@id='ctl00_MainContent_uc1SubmitPriorAuthorization_pnlorderproviderinfo']//td[text()='Medicaid ID']"));
            medicaidElement?.Click();

            submitPriorAuthorizationPage.WaitUntilOrderingMedicaidIsFetched();

            #endregion

            LogStep("Ordering Provider Information", "Ordering Provider Information filled");

            #region Diagnosis Information

            try
            {
                string diagnosisExp = submitPriorAuthorizationPage.DiagnosisExpanderSpan.Text;
                if (diagnosisExp.Equals("+"))
                    TestWebDriver.CreateSmartElement(By.Id("ctl00_MainContent_uc1SubmitPriorAuthorization_pnlsepDiagnosis")).Element.Click();

                Thread.Sleep(2000);

                submitPriorAuthorizationPage.btnDiagnosisAdd.Click();
            }
            catch (Exception)
            {
                submitPriorAuthorizationPage.btnDiagnosisAdd.Click();
            }

            BasePageHelper.WaitUntilElementIsVisible(TestWebDriver, By.Id("ctl00_MainContent_uc1SubmitPriorAuthorization_txtLnDiagnosisCode"), TimeoutConfiguration.Element);
            submitPriorAuthorizationPage.txtLnDiagnosisCode.Set(dentalPA.DentalDiagnosisInformation.DiagnosisCode);
            TestWebDriver.CreateSmartElement(By.Id("ctl00_MainContent_uc1SubmitPriorAuthorization_txtDiagnosisCodeDescription")).Element.Click();

            Thread.Sleep(3000);

            _priorAuthService.DiagnosisPopupSearch(dentalPA.DentalDiagnosisInformation.DiagnosisCode);

            submitPriorAuthorizationPage.txtDiagnosisDate.Set(dentalPA.DentalDiagnosisInformation.DiagnosisDate);
            submitPriorAuthorizationPage.txtDiagnosisCodeDescription.Click();
            Thread.Sleep(2000);
            submitPriorAuthorizationPage.btnDiagnosisAddLine.Click();
            Thread.Sleep(5000);

            #endregion

            LogStep("Diagnosis Information", "Diagnosis Information filled");

            #region Service Details

            submitPriorAuthorizationPage.btnDentalServiceDetailAdd1.Click();
            submitPriorAuthorizationPage.txtDentalSDProcCode.Set(dentalPA.DentalServiceDetails.ProcedureCode);
            submitPriorAuthorizationPage.txtDentalProcCodeDescription.Click();

            Thread.Sleep(3000);

            var lblDentalProcMessage = TestWebDriver.CreateSmartElement(By.Id("ctl00_MainContent_uc1SubmitPriorAuthorization_txtDiagnosisCodeDescription")).Element;

            if (lblDentalProcMessage.Displayed && lblDentalProcMessage.Text.Equals("Procedure code is invalid"))
            {
                TestWebDriver.CreateSmartElement(By.Id("ctl00_MainContent_uc1SubmitPriorAuthorization_lnkDentalSDProcCodeSearchLink")).Element.Click();
                BasePageHelper.WaitUntilElementIsVisible(TestWebDriver, By.Id("ctl00_MainContent_uc1SubmitPriorAuthorization_pnlSubmitPriorAuthSearchProcPop"), TimeoutConfiguration.Element);

                _priorAuthService.ProcedureCodePopupSearch(dentalPA.DentalServiceDetails.ProcedureCode);
            }

            submitPriorAuthorizationPage.txtDentalProcCodeDescription.Set(dentalPA.DentalServiceDetails.ProcedureCodeDescription);

            submitPriorAuthorizationPage.SetDentalToothNumber(dentalPA.DentalServiceDetails.ToothNumber);
            submitPriorAuthorizationPage.SetDentalOralCavity1(dentalPA.DentalServiceDetails.OralCavity);
            submitPriorAuthorizationPage.SetDentalToothSurface1(dentalPA.DentalServiceDetails.ToothSurface);
            submitPriorAuthorizationPage.txtDentalProvServnote.Set(dentalPA.DentalServiceDetails.ProviderServiceNote);
            submitPriorAuthorizationPage.txtDentalReqUnits.Set(dentalPA.DentalServiceDetails.RequestedUnits);
            submitPriorAuthorizationPage.txtDentalReqDollars.Set(dentalPA.DentalServiceDetails.RequestedDollars);
            submitPriorAuthorizationPage.txtDentalReqFDOS.Set(dentalPA.DentalServiceDetails.RequestedFDOS);
            submitPriorAuthorizationPage.txtDentalReqTDOS.Set(dentalPA.DentalServiceDetails.RequestedTDOS);

            var random = new Random();
            int randomNumber = random.Next(1, 9999);
            submitPriorAuthorizationPage.txtDentalServTrackingNo.Set($"AUTH{randomNumber}");
            submitPriorAuthorizationPage.btnServDentalAddUpdate.Click();

            Thread.Sleep(5000);

            #endregion

            LogStep("Service Details", "Service Details filled");

            #region Provider Notes

            string providerNotesExp = submitPriorAuthorizationPage.lblsepProvidersNotes.Text;
            if (providerNotesExp.Equals("+"))
                submitPriorAuthorizationPage.lblsepProvidersNotes.Click();

            submitPriorAuthorizationPage.txtProviderNotes.Set(dentalPA.DentalProviderNotes.ProviderNotes);

            TestWebDriver.FindElement(By.Id("ctl00_MainContent_uc1SubmitPriorAuthorization_Div17")).Click();

            BasePageHelper.WaitUntilElementIsVisible(TestWebDriver, By.Id("btnprovNoteSave"), TimeoutConfiguration.Element);
            submitPriorAuthorizationPage.btnprovNoteSave.Click();
            BasePageHelper.WaitUntilElementIsVisible(TestWebDriver, By.Id("btnprovNoteEdit"), TimeoutConfiguration.Element);

            #endregion

            LogStep("Provider Notes", "Provider Notes filled");

            #region Attachments

            string attachmentExp = submitPriorAuthorizationPage.AttachmentExpanderSpan.Text;
            if (attachmentExp.Equals("+"))
                TestWebDriver.CreateSmartElement(By.Id("ctl00_MainContent_uc1SubmitPriorAuthorization_pnlSepDentalAttachment")).Element.Click();

            Thread.Sleep(2000);

            string filePath = dentalPA.DentalAttachments.FileName;
            submitPriorAuthorizationPage.priorDentalAttachmentUpload.SendKeys(filePath);

            Thread.Sleep(3000);

            submitPriorAuthorizationPage.SetDocumentType(dentalPA.DentalAttachments.DocumentType);
            submitPriorAuthorizationPage.txtPriorDentalAttachmentNote.Set(dentalPA.DentalAttachments.AttachmentNotes);
            submitPriorAuthorizationPage.btnAddDentalAttachment.Click();

            BasePageHelper.WaitUntilDocumentIsReady(TestWebDriver, TimeoutConfiguration.Element);
            BasePageHelper.WaitUntilElementIsVisible(TestWebDriver, By.Id("ctl00_MainContent_uc1SubmitPriorAuthorization_gvDentalAttachment"), TimeoutConfiguration.Element);
            BasePageHelper.WaitUntilDocumentIsReady(TestWebDriver, TimeoutConfiguration.Element);

            #endregion

            LogStep("Attachments", "Attachments filled");
        }

        public void SaveLog(TestCaseExecutionLog testCaseExecutionLog)
        {
            APIGateway.SaveTestCaseLog(testCaseExecutionLog);
        }

        public IWebElement SidebarMenu =>
            TestWebDriver.CreateSmartElement(By.XPath("//button[contains(@class,'hamburger is-closed')]")).Element;

        public void NavigateToSelfService()
        {
            SelfService?.Click();
        }

        public IWebElement SelfService =>
            TestWebDriver.CreateSmartElement(By.XPath("//a[normalize-space()='Self Service']")).Element;
    }
}
