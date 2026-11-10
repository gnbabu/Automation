using NUnit.Framework;
using OpenQA.Selenium;
using Selenium.BaseComponents.Attributes;
using Selenium.BaseComponents.Ohid;
using Selenium.BaseComponents.Utilities;
using SeleniumExtensions.Extensions;
using System.Globalization;
using TC.MemberEligibilitySearch.Pages;
using TestDataModels;

namespace TC.MemberEligibilitySearch.Tests
{
    [TestFixture("OHID")]
    [TestDataFlow(typeof(TestDataModels.MemberEligibilitySearch))]
    [TestDataNote("SearchCriteria", "Logs in through OH|ID with the authenticator (TOTP) code, then runs the same eligibility search as TC_ELIG_01. Only the outcome is logged; no screenshot is taken of the search result.")]
    public class OhidEligibilitySearchTest : OhidFeatureFixture
    {
        private const string TestCaseId = "TC_ELIG_20";
        private const string Description = "OH|ID login with authenticator code, then search member eligibility";

        private TestCaseExecutionLog _testCaseExecutionLog = null!;
        private Screeshots? _screenshots;
        private readonly List<string> _captions = new();

        public OhidEligibilitySearchTest(string profile) : base(profile)
        {
        }

        [Test]
        [Category("IntegrationTests")]
        [Property("Description", Description)]
        [Property("Priority", "High")]
        [Property("TestCaseId", TestCaseId)]
        [CancelAfter(900000)]
        public void OhidLoginAndSearchEligibility()
        {
            _testCaseExecutionLog = new TestCaseExecutionLog
            {
                AssignmentId = AssignmentId,
                AssignmentTestCaseId = AssignmentTestCaseId,
                LogLevel = TestCaseLogLevel.Info,
                ExecutionStatus = ExecutionStatus.Running,
                TestCaseId = TestCaseId,
                TestCaseDescription = Description
            };
            _screenshots = new Screeshots { AssignmentTestCaseId = AssignmentTestCaseId, screenShot = new List<byte[]>() };
            LogStep("Login to PNM", "Signed in to PNM through OH|ID with the authenticator code.");

            var data = TestData.Load<TestDataModels.MemberEligibilitySearch>();
            Assert.That(data.SearchCriteria?.MedicaidBillingNumber, Is.Not.Null.And.Not.Empty,
                "Enter a Medicaid Billing Number in the SearchCriteria section of the test data.");

            OpenSearchEligibility(data.Provider);
            var page = new MemberEligibilityPage(TestWebDriver);

            var problem = SearchMember(page, data.SearchCriteria!);
            if (problem != null)
            {
                LogStep("Eligibility search failed", problem, screenshot: false);
                Assert.Fail($"Eligibility search failed: {problem}");
            }

            LogStep("Test case executed successfully...!", "OH|ID login and eligibility search completed.", screenshot: false);
        }

        private void OpenSearchEligibility(EligibilityProvider provider)
        {
            SidebarMenu.Click();
            SelfService.Click();
            LogStep("Self Service", "Navigating to self service...!");

            var financialProviderInformationPage = new FinancialProviderInformationPage(TestWebDriver);
            financialProviderInformationPage.WaitUntilElementIsVisible();
            financialProviderInformationPage.TxtMedicaid.Set(provider.ProviderMedicaidId);
            financialProviderInformationPage.lnkBtnPriorAuth.Click();
            LogStep("Medicaid Search", $"Entered {provider.ProviderMedicaidId} for search");

            new MemberEligibilityPage(TestWebDriver).Open();
            LogStep("Search Eligibility", "Navigated to Search Eligibility page");
        }

        private string? SearchMember(MemberEligibilityPage page, EligibilitySearchCriteria member)
        {
            var errorBefore = page.VisibleText(page.ErrorMessage);

            page.Type(page.MedicaidBillingNumber, member.MedicaidBillingNumber);
            page.Type(page.Ssn, member.Ssn);
            page.Type(page.DateOfBirth, FormatDate(member.DateOfBirth));
            page.Type(page.FromDateOfService, FormatDate(member.FromDateOfService));
            if (TestWebDriver.FindElements(page.ToDateOfService).Any(e => e.Displayed))
                page.Type(page.ToDateOfService, FormatDate(member.ToDateOfService));

            page.Click(page.SearchButton);

            try
            {
                page.Wait(TimeSpan.FromSeconds(60)).Until(_ =>
                    (page.IsDisplayed(page.RecipientInformationHeader) && BodyText().Contains(member.MedicaidBillingNumber, StringComparison.OrdinalIgnoreCase))
                    || (page.IsDisplayed(page.ErrorMessage) && page.VisibleText(page.ErrorMessage) != errorBefore));
            }
            catch (WebDriverTimeoutException)
            {
                return "No eligibility result or error appeared within 60 seconds.";
            }

            var error = page.VisibleText(page.ErrorMessage);
            if (!string.IsNullOrEmpty(error) && error != errorBefore)
            {
                if (error.Contains("No records returned", StringComparison.OrdinalIgnoreCase))
                {
                    LogStep("Eligibility search", "Search completed: no eligibility records returned.", screenshot: false);
                    return null;
                }

                return error;
            }

            var plans = page.IsDisplayed(page.BenefitPlansHeader) ? "benefit/assignment plan section shown" : "no benefit/assignment plan section";
            LogStep("Eligibility search", $"Search completed: recipient information shown, {plans}.", screenshot: false);
            return null;
        }

        private string BodyText()
        {
            try
            {
                return TestWebDriver.FindElement(By.TagName("body")).Text ?? "";
            }
            catch (WebDriverException)
            {
                return "";
            }
        }

        private static string FormatDate(DateTime? value) =>
            value?.ToString("MM/dd/yyyy", CultureInfo.InvariantCulture) ?? "";

        private void LogStep(string stepName, string message, bool screenshot = true)
        {
            _testCaseExecutionLog.StepName = stepName;
            _testCaseExecutionLog.LogMessage = message;
            APIGateway.SaveTestCaseLog(_testCaseExecutionLog);
            if (!screenshot || _screenshots?.screenShot == null) return;
            _screenshots.screenShot.Add(Common.PrintScreenShot(TestWebDriver, stepName));
            _captions.Add(stepName);
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
                    Caption = index < _captions.Count ? _captions[index] : $"Screenshot_{index + 1}",
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
