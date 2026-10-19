using NUnit.Framework;
using OpenQA.Selenium;
using Selenium.BaseComponents.Attributes;
using Selenium.BaseComponents.Pages;
using Selenium.BaseComponents.Utilities;
using SeleniumExtensions.Configurations;
using SeleniumExtensions.Extensions;
using System.Globalization;
using TC.PriorAuthoriztion.Pages;
using TestDataModels;
using BasePageHelper = SdetToolbox.Pages.PageHelper;

namespace TC.PriorAuthoriztion.Tests
{
    [TestFixture("TechAdmin")]
    [TestDataFlow(typeof(SubmitPAProfessional))]
    public class SubmitPAProfessionalTest : BaseFeatureFixture
    {
        private TestCaseExecutionLog _testCaseExecutionLog = null!;
        private Screeshots? _screenshots;

        public SubmitPAProfessionalTest(string profile) : base(profile)
        {
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

        private ProfessionalPAPage FillProfessionalPA(SubmitPAProfessional data, bool includeOrderingProvider)
        {
            OpenSubmitPriorAuthorization(data.ProviderHeaderInfomration);
            var page = new ProfessionalPAPage(TestWebDriver);

            FillHeader(page, data.ProviderHeaderInfomration);
            FillRecipient(page, data.Recipient);
            FillRequester(page, data.Requester);
            FillServiceInformation(page, data.ServiceInformation);
            FillServiceProvider(page, data.ServiceProvider);
            if (includeOrderingProvider)
                FillOrderingProvider(page, data.OrderingProvider);
            FillDiagnoses(page, data.Diagnoses);
            FillServiceDetails(page, data.ServiceDetails);
            FillProviderNotes(data.ProviderNotes);
            FillAttachments(page, data.Attachments);
            return page;
        }

        private void SaveAndVerify(ProfessionalPAPage page, SubmitPAProfessional data)
        {
            ((IJavaScriptExecutor)TestWebDriver).ExecuteScript("arguments[0].click();", page.Visible(page.SaveButton));
            new OpenQA.Selenium.Support.UI.WebDriverWait(TestWebDriver, TimeoutConfiguration.Element)
                .Until(d => { try { d.SwitchTo().Alert(); return true; } catch (NoAlertPresentException) { return false; } });

            IAlert alert = TestWebDriver.SwitchTo().Alert();
            string alertText = alert.Text;
            alert.Accept();
            if (!alertText.Equals("Your unsubmitted prior authorization will be saved in the system for 72 hours."))
            {
                LogStep("Save failed", $"Unexpected message while saving the Professional PA: {alertText}");
                Assert.Fail($"Unexpected message while saving the Professional PA: {alertText}");
            }

            BasePageHelper.WaitUntilDocumentIsReady(TestWebDriver, TimeoutConfiguration.Element);
            var message = page.Visible(page.SaveMessage);
            Assert.That(message.Text.Trim(), Is.EqualTo("PA request has been saved."), "Professional PA was not saved.");

            LogStep("Test case executed successfully...!",
                $"PA request saved for recipient {data.Recipient.MedicaidBillingNumber} with {data.Diagnoses.Count} diagnosis line(s) and {data.ServiceDetails.Count} service line(s)");
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

        private void FillHeader(ProfessionalPAPage page, ProviderHeaderInfomration header)
        {
            if (!string.IsNullOrWhiteSpace(header.PriorAuthorizationType) &&
                !header.PriorAuthorizationType.Trim().Equals("Professional", StringComparison.OrdinalIgnoreCase))
            {
                Assert.Fail($"This test submits Professional prior authorizations, but the test data's PriorAuthorizationType is \"{header.PriorAuthorizationType}\".");
            }

            page.Click(page.ProfessionalClaimType);
            BasePageHelper.WaitUntilDocumentIsReady(TestWebDriver, TimeoutConfiguration.Element);

            VerifyLabel(By.Id("ctl00_MainContent_ucRegProgressBar_lblPRONPI2"), header.ProviderNpi, "Provider NPI");
            VerifyLabel(By.Id("ctl00_MainContent_ucRegProgressBar_lblProviderName2"), header.ProviderName, "Provider name");

            page.Select(page.DestinationPayer, header.DestinationPayer);
            page.Select(page.Assignment, header.Assignment);
            page.Select(page.ServiceType, header.ServiceType);
            LogStep("Header Information", $"Professional PA: {header.DestinationPayer} / {header.Assignment} / {header.ServiceType}");
        }

        private void FillRecipient(ProfessionalPAPage page, RecipientInfo recipient)
        {
            page.Type(page.MedicaidBillingNumber, recipient.MedicaidBillingNumber, pressEnter: true);

            var loaded = false;
            for (var attempt = 1; attempt <= 2 && !loaded; attempt++)
            {
                page.Type(page.BirthDate, FormatDate(recipient.DateOfBirth));
                page.Click(page.PatientTrackingNumber);
                try
                {
                    page.WaitForValue(page.RecipientFirstName, TimeSpan.FromSeconds(30));
                    loaded = true;
                }
                catch (WebDriverTimeoutException)
                {
                    BasePageHelper.WaitUntilDocumentIsReady(TestWebDriver, TimeoutConfiguration.Element);
                }
            }

            if (!loaded)
                Assert.Fail($"Recipient details weren't loaded for Medicaid billing number {recipient.MedicaidBillingNumber} and date of birth {FormatDate(recipient.DateOfBirth)}.");

            page.Type(page.PatientTrackingNumber, recipient.PatientTrackingNumber);
            LogStep("Recipient Information", $"Recipient {recipient.MedicaidBillingNumber} loaded");
        }

        private void FillRequester(ProfessionalPAPage page, RequesterContactInfo requester)
        {
            page.Type(page.ContactFirstName, requester.FirstName);
            page.Type(page.ContactLastName, requester.LastName);

            var digits = new string((requester.ContactNumber ?? "").Where(char.IsDigit).ToArray());
            if (digits.Length > 0)
            {
                var number = page.Visible(page.ContactNumber);
                number.Click();
                number.SendKeys(Keys.Home);
                number.SendKeys(digits);
                var typed = new string((number.GetAttribute("value") ?? "").Where(char.IsDigit).ToArray());
                if (typed != digits)
                    ((IJavaScriptExecutor)TestWebDriver).ExecuteScript("arguments[0].value = arguments[1];", number, digits);
            }

            page.Type(page.ContactExtension, requester.Extension);
            LogStep("Requestor Contact Information", $"Requester {requester.FirstName} {requester.LastName}");
        }

        private void FillServiceInformation(ProfessionalPAPage page, ServiceInformation service)
        {
            if (!string.IsNullOrWhiteSpace(service.PlaceOfService))
            {
                page.Click(page.PlaceOfServiceSearch);
                page.Visible(page.PlaceOfServicePopup);
                page.Type(page.PlaceOfServiceSearchCode, service.PlaceOfService);
                page.Click(page.PlaceOfServiceSearchButton);
                page.ClickLinkInPopup(page.PlaceOfServicePopup, service.PlaceOfService);
                page.ClosePopup(page.PlaceOfServicePopup);

                var code = service.PlaceOfService.Trim();
                var selected = (TestWebDriver.FindElement(page.PlaceOfService).GetAttribute("value") ?? "").Trim();
                Assert.That(selected == code || selected.StartsWith(code + "-", StringComparison.OrdinalIgnoreCase), Is.True,
                    $"Place of service wasn't filled from the search popup. Expected \"{code}\" (optionally with its description), but the field shows \"{selected}\".");
            }

            page.Select(page.LevelOfService, service.LevelOfService);
            page.Select(page.DelayReason, service.DelayReason);
            TypeDate(page, page.AccidentDate, service.AccidentDate);
            TypeDate(page, page.DateOfPatientEvent, service.DateOfPatientEvent);
            TypeDate(page, page.DateOfOnsetOfIllness, service.DateOfOnsetOfIllness);
            TypeDate(page, page.LastMenstrualPeriodDate, service.LastMenstrualPeriodDate);
            TypeDate(page, page.EstimatedBirthDate, service.EstimatedBirthDate);
            page.Type(page.AssociatePaNumber, service.AssociatePaNumber);
            LogStep("Service Information", $"Place of service {service.PlaceOfService}, level {service.LevelOfService}");
        }

        private void FillServiceProvider(ProfessionalPAPage page, ServiceProviderInfo provider)
        {
            if (string.IsNullOrWhiteSpace(provider.Npi))
                return;

            page.Type(page.ServiceProviderNpi, provider.Npi);
            page.Click(page.ServiceProviderSearch);
            page.Type(page.ServiceProviderSearchNpi, provider.Npi);
            page.Click(page.ServiceProviderSearchButton);
            page.ClickVisibleLink(provider.Npi);
            page.WaitForText(page.ServiceProviderMedicaidId, TimeoutConfiguration.Element);

            VerifyLabel(page.ServiceProviderMedicaidId, provider.MedicaidId, "Service provider Medicaid ID");
            VerifyLabel(page.ServiceProviderName, provider.OrganizationName, "Service provider name");
            LogStep("Service Provider Information",
                $"Service provider {provider.Npi}: {TestWebDriver.FindElement(page.ServiceProviderMedicaidId).Text} {TestWebDriver.FindElement(page.ServiceProviderName).Text}");
        }

        private void FillOrderingProvider(ProfessionalPAPage page, OrderingProviderInfo provider)
        {
            if (string.IsNullOrWhiteSpace(provider.Npi))
                return;

            page.ExpandSection(page.OrderingProviderHeader, page.OrderingProviderNpi);
            page.Type(page.OrderingProviderNpi, provider.Npi);
            page.Click(page.OrderingProviderSearch);
            page.Type(page.OrderingProviderSearchNpi, provider.Npi);
            page.Click(page.OrderingProviderSearchButton);
            page.ClickVisibleLink(provider.Npi);
            BasePageHelper.WaitUntilDocumentIsReady(TestWebDriver, TimeoutConfiguration.Element);
            LogStep("Ordering Provider Information", $"Ordering provider {provider.Npi}");
        }

        private void FillDiagnoses(ProfessionalPAPage page, List<DiagnosisInfo> diagnoses)
        {
            if (diagnoses == null || diagnoses.Count == 0)
                return;

            page.ExpandSection(page.DiagnosisHeader, page.DiagnosisAdd);

            foreach (var diagnosis in diagnoses.OrderBy(d => d.Sequence))
            {
                page.Click(page.DiagnosisAdd);
                page.Type(page.DiagnosisCode, diagnosis.DiagnosisCode);
                page.Click(page.DiagnosisSearch);
                page.Type(page.DiagnosisSearchCode, diagnosis.DiagnosisCode);
                page.Click(page.DiagnosisSearchButton);
                page.ClickVisibleLink(diagnosis.DiagnosisCode);

                TypeDate(page, page.DiagnosisDate, diagnosis.DiagnosisDate);
                page.Select(page.DiagnosisCodeType, diagnosis.DiagnosisCodeType);
                page.Click(page.DiagnosisAddLine);
                WaitUntilHidden(page.DiagnosisCode);
            }

            LogStep("Diagnosis Information", $"{diagnoses.Count} diagnosis line(s): {string.Join(", ", diagnoses.Select(d => d.DiagnosisCode))}");
        }

        private void FillServiceDetails(ProfessionalPAPage page, List<ServiceDetail> serviceDetails)
        {
            if (serviceDetails == null || serviceDetails.Count == 0)
                return;

            page.ExpandSection(page.ServiceDetailsHeader, page.ServiceDetailAdd);

            foreach (var line in serviceDetails.OrderBy(s => s.LineNumber))
            {
                page.Click(page.ServiceDetailAdd);
                page.Type(page.ProcedureCode, line.ProcedureCode);
                page.Click(page.ProcedureCodeSearch);
                page.Type(page.ProcedureSearchCode, line.ProcedureCode);
                page.Click(page.ProcedureSearchButton);
                page.ClickVisibleLink(line.ProcedureCode);

                page.Type(page.Modifier(1), line.Modifier1);
                page.Type(page.Modifier(2), line.Modifier2);
                page.Type(page.Modifier(3), line.Modifier3);
                page.Type(page.Modifier(4), line.Modifier4);
                page.Type(page.RequestedUnits, line.RequestedUnits.ToString(CultureInfo.InvariantCulture));
                page.Type(page.RequestedDollars, line.RequestedDollars.ToString(CultureInfo.InvariantCulture));
                TypeDate(page, page.RequestedFromDate, line.RequestedFromDate);
                TypeDate(page, page.RequestedToDate, line.RequestedToDate);

                page.Click(page.ServiceDetailSave);
                WaitUntilHidden(page.ProcedureCode);
            }

            LogStep("Service Details", $"{serviceDetails.Count} service line(s): {string.Join(", ", serviceDetails.Select(s => s.ProcedureCode))}");
        }

        private void FillProviderNotes(string providerNotes)
        {
            if (string.IsNullOrWhiteSpace(providerNotes))
                return;

            var page = new SubmitPriorAuthorizationPage(TestWebDriver);
            if (page.lblsepProvidersNotes.Text.Equals("+"))
                page.lblsepProvidersNotes.Click();

            page.txtProviderNotes.Set(providerNotes);
            TestWebDriver.FindElement(By.Id("ctl00_MainContent_uc1SubmitPriorAuthorization_Div17")).Click();
            BasePageHelper.WaitUntilElementIsVisible(TestWebDriver, By.Id("btnprovNoteSave"), TimeoutConfiguration.Element);
            page.btnprovNoteSave.Click();
            BasePageHelper.WaitUntilElementIsVisible(TestWebDriver, By.Id("btnprovNoteEdit"), TimeoutConfiguration.Element);
            LogStep("Provider Notes", "Provider Notes filled");
        }

        private void FillAttachments(ProfessionalPAPage page, List<AttachmentInfo> attachments)
        {
            if (attachments == null || attachments.Count == 0)
                return;

            page.ExpandSection(page.AttachmentHeader, page.AttachmentUpload);

            foreach (var attachment in attachments.OrderBy(a => a.LineItem))
            {
                if (string.IsNullOrWhiteSpace(attachment.FilePath) || !File.Exists(attachment.FilePath))
                    Assert.Fail($"Attachment file \"{attachment.FilePath}\" was not found on the machine running the test.");

                TestWebDriver.FindElement(page.AttachmentUpload).SendKeys(attachment.FilePath);
                page.Select(page.AttachmentDocumentType, attachment.DocumentType);
                page.Type(page.AttachmentNote, attachment.Note);
                page.Click(page.AttachmentAdd);
                BasePageHelper.WaitUntilDocumentIsReady(TestWebDriver, TimeoutConfiguration.Element);
            }

            LogStep("Attachments", $"{attachments.Count} attachment(s) added");
        }

        private void TypeDate(ProfessionalPAPage page, By field, DateTime? value)
        {
            if (!value.HasValue) return;
            var element = page.Visible(field);
            element.Clear();
            element.SendKeys(FormatDate(value));
            element.SendKeys(Keys.Tab);
        }

        private void VerifyLabel(By label, string? expected, string what)
        {
            if (string.IsNullOrWhiteSpace(expected)) return;
            var actual = TestWebDriver.FindElement(label).Text.Trim();
            Assert.That(actual, Is.EqualTo(expected.Trim()).IgnoreCase, $"{what} on the form doesn't match the test data.");
        }

        private void WaitUntilHidden(By by)
        {
            try
            {
                new OpenQA.Selenium.Support.UI.WebDriverWait(TestWebDriver, TimeoutConfiguration.Element)
                    .Until(d => d.FindElements(by).All(e => !e.Displayed));
            }
            catch (WebDriverTimeoutException)
            {
                Assert.Fail($"The line wasn't added - the edit row ({by}) is still open. Check the screenshot for a validation message.");
            }
        }

        private static string FormatDate(DateTime? value) =>
            value?.ToString("MM/dd/yyyy", CultureInfo.InvariantCulture) ?? "";

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
