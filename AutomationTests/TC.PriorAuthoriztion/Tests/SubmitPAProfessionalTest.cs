using NUnit.Framework;
using OpenQA.Selenium;
using Selenium.BaseComponents.Attributes;
using Selenium.BaseComponents.Pages;
using Selenium.BaseComponents.Utilities;
using SeleniumExtensions.Configurations;
using SeleniumExtensions.Extensions;
using System.Globalization;
using TC.PriorAuthoriztion.Pages;
using TC.PriorAuthoriztion.Services;
using TestDataModels;
using BasePageHelper = SdetToolbox.Pages.PageHelper;

namespace TC.PriorAuthoriztion.Tests
{
    // Professional Prior Authorization, driven by the TestDataModels.SubmitPAProfessional
    // model: [TestDataFlow] tells the Portal which model's sections/fields to show for
    // this test case, and TestData.Load<T>() loads this test case's own data (multi-row
    // sections such as Diagnoses become lists).
    [TestFixture("TechAdmin")]
    [TestDataFlow(typeof(SubmitPAProfessional))]
    public class SubmitPAProfessionalTest : BaseFeatureFixture
    {
        private PriorAuthorizationService _priorAuthService;
        private TestCaseExecutionLog _testCaseExecutionLog;
        private Screeshots? _screenshots;

        public SubmitPAProfessionalTest(string profile) : base(profile)
        {
        }

        [SetUp]
        public new void BeforeEachTest()
        {
            _priorAuthService = new PriorAuthorizationService(TestWebDriver);
        }

        [Test]
        [Category("IntegrationTests")]
        [Property("Description", "Save a Professional Prior Authorization from code-defined test data")]
        [Property("Priority", "High")]
        [Property("TestCaseId", "TC_PA_PROF_01")]
        [CancelAfter(600000)]
        public void ProfessionalPA_Save()
        {
            StartTest("TC_PA_PROF_01", "Save a Professional Prior Authorization from code-defined test data");

            var data = TestData.Load<SubmitPAProfessional>();
            var page = FillProfessionalPA(data, includeOrderingProvider: true);

            SaveAndVerify(page, data);
        }

        // Same form and model as TC_PA_PROF_01, different scenario: no ordering provider,
        // and several diagnosis lines - every line must show up on the form before it's
        // saved. Its data is saved per test case in the Portal, so it can hold its own
        // diagnosis rows independently of TC_PA_PROF_01 (until saved, both start from the
        // model's defaults).
        [Test]
        [Category("IntegrationTests")]
        [Property("Description", "Save a Professional Prior Authorization with multiple diagnoses and no ordering provider")]
        [Property("Priority", "Medium")]
        [Property("TestCaseId", "TC_PA_PROF_02")]
        [CancelAfter(600000)]
        public void ProfessionalPA_SaveMultipleDiagnosesWithoutOrderingProvider()
        {
            StartTest("TC_PA_PROF_02", "Save a Professional Prior Authorization with multiple diagnoses and no ordering provider");

            var data = TestData.Load<SubmitPAProfessional>();
            Assert.That(data.Diagnoses.Count, Is.GreaterThanOrEqualTo(2),
                "This scenario needs at least 2 rows in the Diagnoses section of its test data.");

            var page = FillProfessionalPA(data, includeOrderingProvider: false);

            var pageSource = TestWebDriver.PageSource;
            var missing = data.Diagnoses.Where(d => !pageSource.Contains(d.DiagnosisCode, StringComparison.OrdinalIgnoreCase))
                .Select(d => d.DiagnosisCode).ToList();
            Assert.That(missing, Is.Empty, $"Diagnosis line(s) not shown on the form: {string.Join(", ", missing)}");
            LogStep("Diagnosis lines verified", $"All {data.Diagnoses.Count} diagnosis lines are on the form");

            SaveAndVerify(page, data);
        }

        private void StartTest(string testCaseId, string description)
        {
            _testCaseExecutionLog = new TestCaseExecutionLog
            {
                AssignmentId = AssignmentId,
                AssignmentTestCaseId = AssignmentTestCaseId,
                LogLevel = TestCaseLogLevel.Info,
                ExecutionStatus = ExecutionStatus.Running,
                TestCaseId = testCaseId,
                TestCaseDescription = description
            };
            _screenshots = new Screeshots { AssignmentTestCaseId = AssignmentTestCaseId, screenShot = new List<byte[]>() };

            LogStep("Login to PNM", "Login to PNM succesfully...!");
        }

        private SubmitPriorAuthorizationPage FillProfessionalPA(SubmitPAProfessional data, bool includeOrderingProvider)
        {
            OpenSubmitPriorAuthorization(data.ProviderHeaderInfomration);
            var page = new SubmitPriorAuthorizationPage(TestWebDriver);

            FillHeader(page, data.ProviderHeaderInfomration);
            FillRecipient(page, data.Recipient);
            FillRequester(page, data.Requester);
            FillServiceInformation(page, data.ServiceInformation);
            FillServiceProvider(page, data.ServiceProvider);
            if (includeOrderingProvider)
                FillOrderingProvider(page, data.OrderingProvider);
            FillDiagnoses(page, data.Diagnoses);
            FillProviderNotes(page, data.ProviderNotes);
            return page;
        }

        private void SaveAndVerify(SubmitPriorAuthorizationPage page, SubmitPAProfessional data)
        {
            ((IJavaScriptExecutor)TestWebDriver).ExecuteScript("arguments[0].click();", page.btnSave);
            Thread.Sleep(3000);

            IAlert alert = TestWebDriver.SwitchTo().Alert();
            string alertText = alert.Text;
            alert.Accept();
            if (!alertText.Equals("Your unsubmitted prior authorization will be saved in the system for 72 hours."))
            {
                LogStep("Save failed", $"Unexpected message while saving the Professional PA: {alertText}");
                Assert.Fail($"Unexpected message while saving the Professional PA: {alertText}");
            }

            BasePageHelper.WaitUntilDocumentIsReady(TestWebDriver, TimeoutConfiguration.Element);
            BasePageHelper.WaitUntilElementIsVisible(TestWebDriver, By.Id("ctl00_MainContent_uc1SubmitPriorAuthorization_lblpriorautherror"), TimeoutConfiguration.Element);

            IWebElement message = TestWebDriver.CreateSmartElement(By.Id("ctl00_MainContent_uc1SubmitPriorAuthorization_lblpriorautherror")).Element;
            Assert.That(message.Displayed && message.Text.Equals("PA request has been saved."), Is.True,
                $"Professional PA was not saved. Message: {message.Text}");

            LogStep("Test case executed successfully...!",
                $"PA request has been saved with Patient Tracking Number::{data.Recipient.PatientTrackingNumber}");
        }

        private void OpenSubmitPriorAuthorization(ProviderHeaderInfomration header)
        {
            SidebarMenu.Click();
            SelfService.Click();
            LogStep("Self Service", "Navigating to self service...!");

            var financialProviderInformationPage = new FinancialProviderInformationPage(TestWebDriver);
            financialProviderInformationPage.WaitUntilElementIsVisible();
            financialProviderInformationPage.TxtMedicaid.Set(header.ProviderMedicaidId);
            financialProviderInformationPage.lnkBtnPriorAuth.Click();
            LogStep("Medicaid Search", $"Entered {header.ProviderMedicaidId} for search");

            var searchPriorAuthorizationPage = new SearchPriorAuthorizationPage(TestWebDriver);
            searchPriorAuthorizationPage.WaitUntilElementIsVisible();
            searchPriorAuthorizationPage.lnkBtnSubmitPriorAuth.Click();
            LogStep("Submit Prior Auth", "Navigated to Submit Prior Authorization page");
        }

        private void FillHeader(SubmitPriorAuthorizationPage page, ProviderHeaderInfomration header)
        {
            page.WaitUntilElementIsVisible();

            // Claim type radio buttons are keyed by lower-case value (e.g. "dental",
            // "professional").
            var claimType = string.IsNullOrWhiteSpace(header.PriorAuthorizationType) ? "professional" : header.PriorAuthorizationType.Trim().ToLowerInvariant();
            TestWebDriver.CreateSmartElement(By.XPath($"//input[@value='{claimType}']")).Element.Click();

            page.SetAuthorization(header.DestinationPayer);
            while (!page.VerifyDropdownHasValues(page.ddlSubCapitaPayerID))
                Thread.Sleep(2000);

            page.SetAssignment(header.Assignment);
            page.SetServiceType(header.ServiceType);
            LogStep("Header Information", "Header Information filled");
        }

        private void FillRecipient(SubmitPriorAuthorizationPage page, RecipientInfo recipient)
        {
            page.txtMedicaidBillingNumber.Set(recipient.MedicaidBillingNumber);
            page.txtBirthDate.Set(FormatDate(recipient.DateOfBirth));
            page.txtPatientTrckNum.Click();

            // Recipient details are looked up by the app once Medicaid ID + DOB are in.
            var firstName = By.Id("ctl00_MainContent_uc1SubmitPriorAuthorization_txtfrstmi2");
            while (string.IsNullOrEmpty(TestWebDriver.CreateSmartElement(firstName).Element.GetAttribute("value")))
                Thread.Sleep(2000);

            page.txtPatientTrckNum.Set(recipient.PatientTrackingNumber);
            LogStep("Recipient Information", "Recipient Information filled");
        }

        private void FillRequester(SubmitPriorAuthorizationPage page, RequesterContactInfo requester)
        {
            page.txtContactName.Set(requester.FirstName);
            page.txtContactLastName.Set(requester.LastName);
            page.txtContactNumber.Click();
            Thread.Sleep(2000);
            ((IJavaScriptExecutor)TestWebDriver).ExecuteScript("arguments[0].value = arguments[1];", page.txtContactNumber, requester.ContactNumber ?? "");
            page.txtContactExt.Set(requester.Extension);
            LogStep("Requestor Contact Information", "Requestor Contact Information filled");
        }

        private void FillServiceInformation(SubmitPriorAuthorizationPage page, ServiceInformation service)
        {
            SetIfPresent(page.txtpalceofservice, service.PlaceOfService);
            page.SetDelayReason(service.DelayReason);
            page.SetListOfService(service.LevelOfService);
            SetIfPresent(page.txtAccDtService, FormatDate(service.AccidentDate));
            SetIfPresent(page.txtDateOfpatientEvent, FormatDate(service.DateOfPatientEvent));
            SetIfPresent(page.txtProfOnsetIllness, FormatDate(service.DateOfOnsetOfIllness));
            SetIfPresent(page.txtMenDtInst, FormatDate(service.LastMenstrualPeriodDate));
            SetIfPresent(page.txtEstDOB, FormatDate(service.EstimatedBirthDate));
            SetIfPresent(page.txtPANumInst, service.AssociatePaNumber);
            LogStep("Service Information", "Service Information filled");
        }

        private void FillServiceProvider(SubmitPriorAuthorizationPage page, ServiceProviderInfo provider)
        {
            page.txtSPNPI.Set(provider.Npi);
            page.pnlService.Click();
            page.WaitUntilMedicaidIsFetched();
            LogStep("Service Provider Information", "Service Provider Information filled");
        }

        private void FillOrderingProvider(SubmitPriorAuthorizationPage page, OrderingProviderInfo provider)
        {
            if (string.IsNullOrWhiteSpace(provider.Npi))
                return;

            if (page.lblseporderproviderinfo.Text.Equals("+"))
                TestWebDriver.CreateSmartElement(By.Id("ctl00_MainContent_uc1SubmitPriorAuthorization_pnlseporderproviderinfo")).Element.Click();

            page.txtorderingprovidernpi.Set(provider.Npi);
            TestWebDriver.FindElement(By.XPath("//div[@id='ctl00_MainContent_uc1SubmitPriorAuthorization_pnlorderproviderinfo']//td[text()='Medicaid ID']")).Click();
            page.WaitUntilOrderingMedicaidIsFetched();
            LogStep("Ordering Provider Information", "Ordering Provider Information filled");
        }

        // One diagnosis line per row of the Diagnoses section, in Sequence order.
        private void FillDiagnoses(SubmitPriorAuthorizationPage page, List<DiagnosisInfo> diagnoses)
        {
            if (diagnoses == null || diagnoses.Count == 0)
                return;

            if (page.DiagnosisExpanderSpan.Text.Equals("+"))
                TestWebDriver.CreateSmartElement(By.Id("ctl00_MainContent_uc1SubmitPriorAuthorization_pnlsepDiagnosis")).Element.Click();
            Thread.Sleep(2000);

            foreach (var diagnosis in diagnoses.OrderBy(d => d.Sequence))
            {
                page.btnDiagnosisAdd.Click();
                BasePageHelper.WaitUntilElementIsVisible(TestWebDriver, By.Id("ctl00_MainContent_uc1SubmitPriorAuthorization_txtLnDiagnosisCode"), TimeoutConfiguration.Element);

                if (!string.IsNullOrWhiteSpace(diagnosis.DiagnosisCodeType))
                    new OpenQA.Selenium.Support.UI.SelectElement(page.ddlDiagnosisCodeType).SelectByText(diagnosis.DiagnosisCodeType);

                page.txtLnDiagnosisCode.Set(diagnosis.DiagnosisCode);
                page.txtDiagnosisCodeDescription.Click();
                Thread.Sleep(3000);
                _priorAuthService.DiagnosisPopupSearch(diagnosis.DiagnosisCode);

                SetIfPresent(page.txtDiagnosisDate, FormatDate(diagnosis.DiagnosisDate));
                page.txtDiagnosisCodeDescription.Click();
                Thread.Sleep(2000);
                page.btnDiagnosisAddLine.Click();
                Thread.Sleep(5000);
            }

            LogStep("Diagnosis Information", $"{diagnoses.Count} diagnosis line(s) filled");
        }

        private void FillProviderNotes(SubmitPriorAuthorizationPage page, string providerNotes)
        {
            if (string.IsNullOrWhiteSpace(providerNotes))
                return;

            if (page.lblsepProvidersNotes.Text.Equals("+"))
                page.lblsepProvidersNotes.Click();

            page.txtProviderNotes.Set(providerNotes);
            TestWebDriver.FindElement(By.Id("ctl00_MainContent_uc1SubmitPriorAuthorization_Div17")).Click();
            BasePageHelper.WaitUntilElementIsVisible(TestWebDriver, By.Id("btnprovNoteSave"), TimeoutConfiguration.Element);
            page.btnprovNoteSave.Click();
            BasePageHelper.WaitUntilElementIsVisible(TestWebDriver, By.Id("btnprovNoteEdit"), TimeoutConfiguration.Element);
            LogStep("Provider Notes", "Provider Notes filled");
        }

        private static void SetIfPresent(IWebElement element, string value)
        {
            if (!string.IsNullOrWhiteSpace(value))
                element.Set(value);
        }

        // The app's date boxes take M/d/yyyy (matches existing Prior Authorization data).
        private static string FormatDate(DateTime? value) =>
            value?.ToString("M/d/yyyy", CultureInfo.InvariantCulture) ?? "";

        private void LogStep(string stepName, string message)
        {
            _testCaseExecutionLog.StepName = stepName;
            _testCaseExecutionLog.LogMessage = message;
            APIGateway.SaveTestCaseLog(_testCaseExecutionLog);
            _screenshots?.screenShot.Add(Common.PrintScreenShot(TestWebDriver, stepName));
        }

        [TearDown]
        public void AfterTest()
        {
            if (_screenshots?.screenShot == null || !_screenshots.screenShot.Any() || _screenshots.AssignmentTestCaseId == 0)
                return;

            var screenshots = _screenshots.screenShot
                .Select((screen, index) => new TestScreenshot
                {
                    ID = index + 1,
                    AssignmentTestCaseId = _screenshots.AssignmentTestCaseId,
                    Caption = $"Screenshot_{index + 1}",
                    Screenshot = $"data:image/png;base64,{Convert.ToBase64String(screen)}",
                    TakenAt = DateTime.Now,
                })
                .ToList();

            APIGateway.SaveMethodScreenShots(screenshots);
        }

        private IWebElement SidebarMenu =>
            TestWebDriver.CreateSmartElement(By.XPath("//button[contains(@class,'hamburger is-closed')]")).Element;

        private IWebElement SelfService =>
            TestWebDriver.CreateSmartElement(By.XPath("//a[normalize-space()='Self Service']")).Element;
    }
}
