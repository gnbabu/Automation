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
    [TestDataNote("Attachments", "Professional PAs need at least one attachment.")]
    [TestDataNote("Attachments", "A full path on the machine that runs the test, or a path relative to the test DLL folder, e.g. TestFiles\\SampleAttachment.pdf (shipped with the tests).", Field = "FilePath")]
    [TestDataNote("Requester", "Enter the phone number as 10 digits, no dashes.", Field = "ContactNumber")]
    public class SubmitPAProfessionalTest : BaseFeatureFixture
    {
        private TestCaseExecutionLog _testCaseExecutionLog = null!;
        private Screeshots? _screenshots;

        public SubmitPAProfessionalTest(string profile) : base(profile)
        {
        }

        [Test]
        [Category("IntegrationTests")]
        [Property("Description", "Submit a Professional PA with two diagnoses, two service lines and an attachment")]
        [Property("Priority", "High")]
        [Property("TestCaseId", "TC_PA_PROF_01")]
        [TestDataNote("Diagnoses", "Needs at least 2 diagnosis rows.")]
        [TestDataNote("ServiceDetails", "Needs at least 2 service lines.")]
        [TestDataNote("ServiceDetails", "E2E rejects a second PA for the same member, provider and dates. Change the service line dates here before re-running.")]
        [TestDataNote("Diagnoses", "Diagnosis dates can't be in the future.", Field = "DiagnosisDate")]
        [CancelAfter(900000)]
        public void ProfessionalPA_Save()
        {
            StartTest("TC_PA_PROF_01", "Submit a Professional PA with two diagnoses, two service lines and an attachment");

            var data = TestData.Load<SubmitPAProfessional>();
            Assert.That(data.Diagnoses.Count, Is.GreaterThanOrEqualTo(2), "This scenario needs at least 2 rows in the Diagnoses section of its test data.");
            Assert.That(data.ServiceDetails.Count, Is.GreaterThanOrEqualTo(2), "This scenario needs at least 2 rows in the ServiceDetails section of its test data.");
            Assert.That(data.Attachments.Count, Is.GreaterThanOrEqualTo(1), "This scenario needs at least 1 row in the Attachments section of its test data.");

            var page = FillProfessionalPA(data, includeOrderingProvider: true);
            VerifyFormGrids(data, "before submitting");
            SubmitAndVerify(page, data);
        }

        [Test]
        [Category("IntegrationTests")]
        [Property("Description", "Save a Professional Prior Authorization with multiple diagnoses and no ordering provider")]
        [Property("Priority", "Medium")]
        [Property("TestCaseId", "TC_PA_PROF_02")]
        [TestDataNote("This scenario saves a PA with several diagnoses and no ordering provider.")]
        [TestDataNote("Diagnoses", "Needs at least 2 diagnosis rows.")]
        [TestDataNote("OrderingProvider", "Not used by this test case.")]
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

        private void VerifyFormGrids(SubmitPAProfessional data, string when)
        {
            var source = TestWebDriver.PageSource;
            var missing = new List<string>();
            missing.AddRange(data.Diagnoses.Where(d => !source.Contains(d.DiagnosisCode, StringComparison.OrdinalIgnoreCase))
                .Select(d => $"diagnosis {d.DiagnosisCode}"));
            missing.AddRange(data.ServiceDetails.Where(s => !source.Contains(s.ProcedureCode, StringComparison.OrdinalIgnoreCase))
                .Select(s => $"service line {s.ProcedureCode}"));
            missing.AddRange(data.Attachments.Select(a => Path.GetFileName(a.FilePath ?? ""))
                .Where(name => name.Length > 0 && !source.Contains(name, StringComparison.OrdinalIgnoreCase))
                .Select(name => $"attachment {name}"));

            if (missing.Count > 0)
            {
                LogStep("Grid check failed", $"Not shown {when}: {string.Join(", ", missing)}");
                Assert.Fail($"These lines aren't shown in their grids {when}: {string.Join(", ", missing)}");
            }

            LogStep("Grid check",
                $"Shown {when}: diagnoses {string.Join(", ", data.Diagnoses.Select(d => d.DiagnosisCode))}; " +
                $"service lines {string.Join(", ", data.ServiceDetails.Select(s => s.ProcedureCode))}; " +
                $"attachments {string.Join(", ", data.Attachments.Select(a => Path.GetFileName(a.FilePath ?? "")))}");
        }

        private void SubmitAndVerify(ProfessionalPAPage page, SubmitPAProfessional data)
        {
            var js = (IJavaScriptExecutor)TestWebDriver;
            if (data.Attachments.Count > 0)
                WaitForAttachmentProcessing();
            EnsureRecipientLoaded(page, data.Recipient);

            var textBeforeSubmit = new HashSet<string>(PageTextLines(), StringComparer.OrdinalIgnoreCase);
            js.ExecuteScript("arguments[0].scrollIntoView({block:'center'}); arguments[0].click();", page.Visible(page.SubmitButton));
            LogStep("Submit", "Clicked Submit");

            var deadline = DateTime.UtcNow.AddMinutes(3);
            var resubmitted = 0;
            var reuploads = 0;
            while (true)
            {
                if (DateTime.UtcNow > deadline)
                {
                    LogStep("Submit timed out", "No success or failure message appeared within 3 minutes of clicking Submit.");
                    Assert.Fail("No success or failure message appeared within 3 minutes of clicking Submit.");
                }

                var alertText = HandleSubmitAlert();
                if (alertText != null)
                {
                    LogStep("Submit message", alertText);
                    if (alertText.Contains("will be saved in the system", StringComparison.OrdinalIgnoreCase) && resubmitted++ < 2)
                    {
                        PageHelperWaitForPostback();
                        EnsureRecipientLoaded(page, data.Recipient);
                        textBeforeSubmit = new HashSet<string>(PageTextLines(), StringComparer.OrdinalIgnoreCase);
                        js.ExecuteScript("arguments[0].scrollIntoView({block:'center'}); arguments[0].click();", page.Visible(page.SubmitButton));
                        LogStep("Submit", "Dismissed the save prompt and clicked Submit again");
                        deadline = DateTime.UtcNow.AddMinutes(3);
                    }
                    continue;
                }

                if (page.IsDisplayed(page.SubmitSuccessPanel) || !string.IsNullOrEmpty(FieldValue(page.PaNumberField)))
                    break;

                if (page.IsDisplayed(page.SubmitFailurePanel))
                {
                    var reason = TestWebDriver.FindElements(page.SubmitFailureMessage).FirstOrDefault()?.Text?.Trim();
                    var panelText = TestWebDriver.FindElement(page.SubmitFailurePanel).Text.Trim();
                    var message = string.IsNullOrEmpty(reason) ? panelText : reason;
                    if (message.Contains("Valid authorization", StringComparison.OrdinalIgnoreCase))
                        message += " - a PA already exists for these dates. Change the service line dates in Test Data Management before running again.";
                    LogStep("Submission failed", message);
                    Assert.Fail($"The PA was not submitted: {message}");
                }

                if (page.IsDisplayed(page.WarningPanel))
                {
                    var warning = TestWebDriver.FindElement(page.WarningPanel).Text.Trim();
                    LogStep("Submit warning", $"Acknowledged: {warning}");
                    js.ExecuteScript("arguments[0].click();", TestWebDriver.FindElement(page.WarningYes));
                    page.WaitUntilHidden(page.WarningPanel, TimeoutConfiguration.Element);
                    PageHelperWaitForPostback();

                    var settleUntil = DateTime.UtcNow.AddSeconds(20);
                    while (DateTime.UtcNow < settleUntil && !page.IsDisplayed(page.SubmitSuccessPanel) && !page.IsDisplayed(page.SubmitFailurePanel)
                        && string.IsNullOrEmpty(FieldValue(page.PaNumberField)) && !page.IsDisplayed(page.WarningPanel)
                        && !PageTextLines().Any(line => !textBeforeSubmit.Contains(line) && ErrorText.IsMatch(line)))
                        Thread.Sleep(500);

                    var outcomeShown = page.IsDisplayed(page.SubmitSuccessPanel) || page.IsDisplayed(page.SubmitFailurePanel)
                        || !string.IsNullOrEmpty(FieldValue(page.PaNumberField)) || page.IsDisplayed(page.WarningPanel)
                        || PageTextLines().Any(line => !textBeforeSubmit.Contains(line) && ErrorText.IsMatch(line));
                    if (!outcomeShown && resubmitted++ < 2 && page.IsDisplayed(page.SubmitButton))
                    {
                        EnsureRecipientLoaded(page, data.Recipient);
                        js.ExecuteScript("arguments[0].click();", TestWebDriver.FindElement(page.SubmitButton));
                        LogStep("Submit", "Clicked Submit again after acknowledging the warnings");
                    }
                    continue;
                }

                var errors = VisibleValidationErrors(page);
                errors.AddRange(PageTextLines().Where(line => !textBeforeSubmit.Contains(line) && ErrorText.IsMatch(line)));
                if (errors.Count > 0)
                {
                    PageHelperWaitForPostback();
                    errors = errors.Distinct().ToList();

                    var notUploaded = errors.Select(e => NotUploadedAttachment.Match(e)).FirstOrDefault(m => m.Success);
                    if (notUploaded != null && reuploads < 2 && errors.All(e => NotUploadedAttachment.IsMatch(e) || e.StartsWith("An asterisk", StringComparison.OrdinalIgnoreCase)))
                    {
                        reuploads++;
                        LogStep("Attachment re-upload", $"Attempt {reuploads}: {string.Join(" | ", errors)}");
                        DeleteAttachment(page, notUploaded.Groups[1].Value);
                        FillAttachments(page, data.Attachments);
                        WaitForAttachmentProcessing();
                        EnsureRecipientLoaded(page, data.Recipient);

                        textBeforeSubmit = new HashSet<string>(PageTextLines(), StringComparer.OrdinalIgnoreCase);
                        js.ExecuteScript("arguments[0].scrollIntoView({block:'center'}); arguments[0].click();", page.Visible(page.SubmitButton));
                        LogStep("Submit", $"Clicked Submit again after re-uploading the attachment (attempt {reuploads})");
                        deadline = DateTime.UtcNow.AddMinutes(3);
                        continue;
                    }

                    LogStep("Submission rejected", string.Join(" | ", errors));
                    Assert.Fail($"The application rejected the submission: {string.Join(" | ", errors)}");
                }

                Thread.Sleep(500);
            }

            PageHelperWaitForPostback();
            var successText = page.IsDisplayed(page.SubmitSuccessPanel)
                ? TestWebDriver.FindElement(page.SubmitSuccessPanel).Text.Trim()
                : "";
            var status = FieldValue(page.PaStatusField);
            LogStep("PA submitted", $"Status: {status}. {successText}".Trim());

            var paNumber = FindPaNumber(successText);
            if (string.IsNullOrEmpty(paNumber)) paNumber = FieldValue(page.PaNumberField);
            if (string.IsNullOrEmpty(paNumber))
            {
                LogStep("PA number missing", $"Couldn't find a PA number after submitting. Success message: {successText}. {DescribeCandidates("PANum", "PANo", "PriorAuthNum", "AuthNum")}");
                Assert.Fail($"The PA was submitted but no PA number was found. Success message: {successText}");
            }

            if (!successText.Contains("Successfully Submitted", StringComparison.OrdinalIgnoreCase))
            {
                LogStep("Submission not confirmed", $"PA {paNumber}: the success message wasn't shown. {successText}");
                Assert.Fail($"PA {paNumber}: the success message wasn't shown.");
            }

            CloseSuccessPanel(page);

            LogStep("Test case executed successfully...!",
                $"PA {paNumber} submitted on {DateTime.Today:MM/dd/yyyy} (status Pend) for recipient {data.Recipient.MedicaidBillingNumber} with " +
                $"{data.Diagnoses.Count} diagnoses, {data.ServiceDetails.Count} service lines and {data.Attachments.Count} attachment(s)");
        }

        private static readonly System.Text.RegularExpressions.Regex ErrorText = new(
            @"\b(is not valid|not valid for|is invalid|invalid\b|is required|are required|must be|must not|cannot|can not|not allowed|not eligible|failed|error)\b",
            System.Text.RegularExpressions.RegexOptions.IgnoreCase);

        private static readonly System.Text.RegularExpressions.Regex NotUploadedAttachment = new(
            @"Attachment Id:\s*(\d+).*not uploaded properly", System.Text.RegularExpressions.RegexOptions.IgnoreCase);

        private void EnsureRecipientLoaded(ProfessionalPAPage page, RecipientInfo recipient)
        {
            PageHelperWaitForPostback();
            if (!string.IsNullOrEmpty(FieldValue(page.RecipientFirstName)))
                return;

            LogStep("Recipient reload", "The recipient's name was cleared by a page reload - running the recipient lookup again before submitting");
            FillRecipient(page, recipient);
        }

        private void WaitForAttachmentProcessing()
        {
            PageHelperWaitForPostback();
            Thread.Sleep(TimeSpan.FromSeconds(20));
            PageHelperWaitForPostback();
        }

        private void DeleteAttachment(ProfessionalPAPage page, string documentId)
        {
            var delete = By.XPath(
                $"//td[normalize-space()='{documentId}']/ancestor::tr[1]//*[(self::input and normalize-space(@value)='Delete') or ((self::button or self::a) and normalize-space()='Delete')]");
            var button = TestWebDriver.FindElements(delete).FirstOrDefault(e => e.Displayed);
            if (button == null)
            {
                LogStep("Attachment re-upload failed", $"Couldn't find the Delete button for attachment {documentId}.");
                Assert.Fail($"Couldn't find the Delete button for attachment {documentId}.");
            }

            ((IJavaScriptExecutor)TestWebDriver).ExecuteScript("arguments[0].click();", button);
            for (var i = 0; i < 10 && AcceptAlertIfPresent() == null; i++) Thread.Sleep(300);
            PageHelperWaitForPostback();

            try
            {
                new OpenQA.Selenium.Support.UI.WebDriverWait(TestWebDriver, TimeoutConfiguration.Element)
                    .Until(d => !d.FindElements(By.XPath($"//td[normalize-space()='{documentId}']")).Any(e => e.Displayed));
            }
            catch (WebDriverTimeoutException)
            {
                LogStep("Attachment re-upload failed", $"Attachment {documentId} is still in the grid after clicking Delete.");
                Assert.Fail($"Attachment {documentId} is still in the grid after clicking Delete.");
            }

            LogStep("Attachment deleted", $"Deleted attachment {documentId}");
        }

        private IEnumerable<string> PageTextLines()
        {
            try
            {
                var text = TestWebDriver.FindElement(By.TagName("body")).Text ?? "";
                return text.Split('\n').Select(l => l.Trim()).Where(l => l.Length > 0).ToList();
            }
            catch (WebDriverException)
            {
                return Enumerable.Empty<string>();
            }
        }

        private string? FieldValue(By field)
        {
            try
            {
                var element = TestWebDriver.FindElements(field).FirstOrDefault();
                var value = element?.GetAttribute("value")?.Trim();
                return string.IsNullOrEmpty(value) ? null : value;
            }
            catch (WebDriverException)
            {
                return null;
            }
        }

        private void PageHelperWaitForPostback() =>
            Selenium.BaseComponents.Utilities.PageHelper.WaitForPostback(TestWebDriver, TimeSpan.FromSeconds(30));

        private string? HandleSubmitAlert()
        {
            try
            {
                var alert = TestWebDriver.SwitchTo().Alert();
                var text = alert.Text;
                if (text.Contains("will be saved in the system", StringComparison.OrdinalIgnoreCase))
                    alert.Dismiss();
                else
                    alert.Accept();
                return text;
            }
            catch (NoAlertPresentException)
            {
                return null;
            }
        }

        private string? AcceptAlertIfPresent()
        {
            try
            {
                var alert = TestWebDriver.SwitchTo().Alert();
                var text = alert.Text;
                alert.Accept();
                return text;
            }
            catch (NoAlertPresentException)
            {
                return null;
            }
        }

        private List<string> VisibleValidationErrors(ProfessionalPAPage page)
        {
            var errors = TestWebDriver.FindElements(page.ValidationMessages)
                .Where(e => { try { return e.Displayed; } catch (StaleElementReferenceException) { return false; } })
                .Select(e => e.Text.Trim())
                .Where(t => t.Length > 0)
                .Distinct()
                .ToList();

            var paError = TestWebDriver.FindElements(page.SaveMessage).FirstOrDefault(e => e.Displayed)?.Text?.Trim();
            if (!string.IsNullOrEmpty(paError) && !paError.Contains("saved", StringComparison.OrdinalIgnoreCase)
                && !paError.Contains("submitted", StringComparison.OrdinalIgnoreCase))
                errors.Add(paError);

            return errors;
        }

        private string? FindPaNumber(string successText)
        {
            var labelled = System.Text.RegularExpressions.Regex.Match(successText,
                @"(PA|Prior\s*Auth(orization)?)\s*(Number|No\.?|#)\s*(is)?\s*[:\-]?\s*([A-Z0-9][A-Z0-9\-]{4,})",
                System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            if (labelled.Success) return labelled.Groups[5].Value;

            var fieldValue = FieldValueById(id =>
                (id.Contains("PANum", StringComparison.OrdinalIgnoreCase) || id.Contains("PANo", StringComparison.OrdinalIgnoreCase)
                    || id.Contains("PriorAuthNum", StringComparison.OrdinalIgnoreCase))
                && !id.EndsWith("txtPANumInst", StringComparison.OrdinalIgnoreCase));
            if (!string.IsNullOrEmpty(fieldValue)) return fieldValue;

            var digits = System.Text.RegularExpressions.Regex.Match(successText, @"\b\d{7,}\b");
            return digits.Success ? digits.Value : null;
        }

        private void CloseSuccessPanel(ProfessionalPAPage page)
        {
            if (!page.IsDisplayed(page.SubmitSuccessPanel)) return;

            var close = TestWebDriver.FindElement(page.SubmitSuccessPanel)
                .FindElements(By.XPath(".//input[@type='submit' or @type='button'][normalize-space(@value)='Close'] | .//button[normalize-space()='Close'] | .//a[normalize-space()='Close']"))
                .FirstOrDefault(e => e.Displayed);
            if (close == null) return;

            ((IJavaScriptExecutor)TestWebDriver).ExecuteScript("arguments[0].click();", close);
            PageHelperWaitForPostback();
            page.WaitUntilHidden(page.SubmitSuccessPanel, TimeoutConfiguration.Element);
        }

        private string? FieldValueById(Func<string, bool> idMatches)
        {
            foreach (var element in TestWebDriver.FindElements(By.XPath("//*[@id]")))
            {
                try
                {
                    var id = element.GetAttribute("id") ?? "";
                    if (!idMatches(id) || !element.Displayed) continue;
                    var value = (element.TagName.Equals("input", StringComparison.OrdinalIgnoreCase)
                        ? element.GetAttribute("value") : element.Text)?.Trim();
                    if (!string.IsNullOrEmpty(value)) return value;
                }
                catch (StaleElementReferenceException)
                {
                }
            }
            return null;
        }

        private string DescribeCandidates(params string[] idParts)
        {
            var found = new List<string>();
            foreach (var element in TestWebDriver.FindElements(By.XPath("//*[@id]")))
            {
                try
                {
                    var id = element.GetAttribute("id") ?? "";
                    if (!idParts.Any(p => id.Contains(p, StringComparison.OrdinalIgnoreCase))) continue;
                    var value = element.TagName.Equals("input", StringComparison.OrdinalIgnoreCase) ? element.GetAttribute("value") : element.Text;
                    found.Add($"{id}={(value ?? "").Trim()}{(element.Displayed ? "" : " (hidden)")}");
                }
                catch (StaleElementReferenceException)
                {
                }
            }
            return found.Count == 0 ? "No candidate fields found on the page." : "Candidate fields: " + string.Join("; ", found.Take(15));
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
            var assignment = SelectBestOption(page, page.Assignment, header.Assignment, "Assignment", fallbackToFirst: false);
            var serviceType = SelectBestOption(page, page.ServiceType, header.ServiceType, "Service Type", fallbackToFirst: true);
            var payerId = SelectDestinationPayerId(page, header.DestinationPayerId);
            LogStep("Header Information", $"Professional PA: {header.DestinationPayer} / {assignment} / payer ID {payerId ?? "-"} / {serviceType}");
        }

        private string? SelectBestOption(ProfessionalPAPage page, By dropdown, string? wanted, string what, bool fallbackToFirst)
        {
            if (string.IsNullOrWhiteSpace(wanted)) return null;

            string[] options = Array.Empty<string>();
            try
            {
                new OpenQA.Selenium.Support.UI.WebDriverWait(TestWebDriver, TimeoutConfiguration.Element).Until(d =>
                {
                    options = new OpenQA.Selenium.Support.UI.SelectElement(d.FindElement(dropdown)).Options
                        .Select(o => o.Text.Trim())
                        .Where(t => t.Length > 0 && !t.StartsWith("--") && !t.StartsWith("Select", StringComparison.OrdinalIgnoreCase))
                        .ToArray();
                    return options.Length > 0;
                });
            }
            catch (WebDriverTimeoutException)
            {
                Assert.Fail($"The {what} dropdown has no options.");
            }

            var target = wanted.Trim();
            var choice = options.FirstOrDefault(o => o.Equals(target, StringComparison.OrdinalIgnoreCase))
                ?? options.FirstOrDefault(o => o.Contains(target, StringComparison.OrdinalIgnoreCase))
                ?? (fallbackToFirst ? options.First() : null);

            if (choice == null)
            {
                LogStep($"{what} not found", $"No {what} option matches \"{target}\". Options: {string.Join(" | ", options)}");
                Assert.Fail($"No {what} option matches \"{target}\". Options: {string.Join(" | ", options)}");
            }

            if (!choice!.Equals(target, StringComparison.OrdinalIgnoreCase))
                LogStep($"{what} options", $"\"{target}\" matched \"{choice}\". Options: {string.Join(" | ", options)}");

            page.Select(dropdown, choice);
            return choice;
        }

        private string? SelectDestinationPayerId(ProfessionalPAPage page, string? payerId)
        {
            var element = TestWebDriver.FindElements(page.DestinationPayerId).FirstOrDefault(e => e.Displayed && e.Enabled);
            if (element == null) return null;

            if (!string.IsNullOrWhiteSpace(payerId))
            {
                page.Select(page.DestinationPayerId, payerId);
                return payerId.Trim();
            }

            var select = new OpenQA.Selenium.Support.UI.SelectElement(element);
            var current = select.SelectedOption?.Text?.Trim() ?? "";
            if (current.Length > 0 && !current.StartsWith("--") && !current.StartsWith("Select", StringComparison.OrdinalIgnoreCase))
                return current;

            var firstValid = select.Options.Select(o => o.Text.Trim())
                .FirstOrDefault(t => t.Length > 0 && !t.StartsWith("--") && !t.StartsWith("Select", StringComparison.OrdinalIgnoreCase));
            if (firstValid != null) page.Select(page.DestinationPayerId, firstValid);
            return firstValid;
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

            if (string.IsNullOrWhiteSpace(recipient.PatientTrackingNumber))
                recipient.PatientTrackingNumber = $"PROF{DateTime.Now:yyMMddHHmmss}";
            page.Type(page.PatientTrackingNumber, recipient.PatientTrackingNumber);
            LogStep("Recipient Information", $"Recipient {recipient.MedicaidBillingNumber} loaded, patient tracking number {recipient.PatientTrackingNumber}");
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
                page.Type(page.RequestedDollars, line.RequestedDollars.ToString("0.00", CultureInfo.InvariantCulture));
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
                var path = ResolveAttachmentPath(attachment.FilePath);
                if (path == null)
                    Assert.Fail($"Attachment file \"{attachment.FilePath}\" was not found on the machine running the test (also checked next to the test DLL).");

                TestWebDriver.FindElement(page.AttachmentUpload).SendKeys(path);
                page.Select(page.AttachmentDocumentType, attachment.DocumentType);
                page.Type(page.AttachmentNote, attachment.Note);
                page.Click(page.AttachmentAdd);
                BasePageHelper.WaitUntilDocumentIsReady(TestWebDriver, TimeoutConfiguration.Element);

                var fileName = Path.GetFileName(path);
                try
                {
                    new OpenQA.Selenium.Support.UI.WebDriverWait(TestWebDriver, TimeoutConfiguration.Element)
                        .Until(d => d.PageSource.Contains(fileName, StringComparison.OrdinalIgnoreCase));
                }
                catch (WebDriverTimeoutException)
                {
                    var errors = VisibleValidationErrors(page);
                    LogStep("Attachment failed", $"{fileName} didn't appear in the attachment grid. {string.Join(" | ", errors)}");
                    Assert.Fail($"Attachment {fileName} didn't appear in the attachment grid. {string.Join(" | ", errors)}");
                }
            }

            LogStep("Attachments", $"{attachments.Count} attachment(s) added: {string.Join(", ", attachments.Select(a => Path.GetFileName(a.FilePath ?? "")))}");
        }

        private static string? ResolveAttachmentPath(string? filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath)) return null;
            if (Path.IsPathRooted(filePath)) return File.Exists(filePath) ? filePath : null;

            var testFolder = Path.GetDirectoryName(typeof(SubmitPAProfessionalTest).Assembly.Location) ?? AppContext.BaseDirectory;
            var candidate = Path.GetFullPath(Path.Combine(testFolder, filePath));
            return File.Exists(candidate) ? candidate : null;
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
                var messages = PageTextLines().Where(line => ErrorText.IsMatch(line) || line.Contains("should not", StringComparison.OrdinalIgnoreCase)).Distinct().ToList();
                var detail = messages.Count > 0 ? string.Join(" | ", messages) : "Check the screenshot for a validation message.";
                LogStep("Line not added", detail);
                Assert.Fail($"The line wasn't added - the edit row ({by}) is still open. {detail}");
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
