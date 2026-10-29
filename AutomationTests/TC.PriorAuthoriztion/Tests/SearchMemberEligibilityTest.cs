using NUnit.Framework;
using OpenQA.Selenium;
using Selenium.BaseComponents.Attributes;
using Selenium.BaseComponents.Pages;
using Selenium.BaseComponents.Utilities;
using SeleniumExtensions.Extensions;
using System.Globalization;
using TC.PriorAuthoriztion.Pages;
using TestDataModels;

namespace TC.PriorAuthoriztion.Tests
{
    [TestFixture("TechAdmin")]
    [TestDataFlow(typeof(MemberEligibilitySearch))]
    [TestDataNote("SearchCriteria", "One search per test case: the member to look up and the From/To DOS eligibility window. \"No records returned\" counts as a completed search.")]
    [TestDataNote("SearchCriteria", "Optional. Leave empty to search by Medicaid Billing Number and date of birth only.", Field = "Ssn")]
    public class SearchMemberEligibilityTest : BaseFeatureFixture
    {
        private TestCaseExecutionLog _testCaseExecutionLog = null!;
        private Screeshots? _screenshots;

        public SearchMemberEligibilityTest(string profile) : base(profile)
        {
        }

        [Test]
        [Category("IntegrationTests")]
        [Property("Description", "Search member eligibility by Medicaid Billing Number, date of birth and dates of service")]
        [Property("Priority", "High")]
        [Property("TestCaseId", "TC_ELIG_01")]
        [CancelAfter(900000)]
        public void SearchMemberEligibility()
        {
            _testCaseExecutionLog = new TestCaseExecutionLog
            {
                AssignmentId = AssignmentId,
                AssignmentTestCaseId = AssignmentTestCaseId,
                LogLevel = TestCaseLogLevel.Info,
                ExecutionStatus = ExecutionStatus.Running,
                TestCaseId = "TC_ELIG_01",
                TestCaseDescription = "Search member eligibility by Medicaid Billing Number, date of birth and dates of service"
            };
            _screenshots = new Screeshots { AssignmentTestCaseId = AssignmentTestCaseId, screenShot = new List<byte[]>() };
            LogStep("Login to PNM", "Login to PNM succesfully...!");

            var data = TestData.Load<MemberEligibilitySearch>();
            Assert.That(data.SearchCriteria?.MedicaidBillingNumber, Is.Not.Null.And.Not.Empty,
                "Enter a Medicaid Billing Number in the SearchCriteria section of the test data.");

            OpenSearchEligibility(data.Provider);
            var page = new MemberEligibilityPage(TestWebDriver);

            var problem = SearchMember(page, data.SearchCriteria!);
            if (problem != null)
            {
                LogStep("Eligibility search failed", problem);
                Assert.Fail($"Eligibility search failed: {problem}");
            }

            LogStep("Test case executed successfully...!", $"Eligibility search completed for {data.SearchCriteria!.MedicaidBillingNumber}");
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
            var who = member.MedicaidBillingNumber;
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
                    (page.IsDisplayed(page.RecipientInformationHeader) && BodyText().Contains(who, StringComparison.OrdinalIgnoreCase))
                    || (page.IsDisplayed(page.ErrorMessage) && page.VisibleText(page.ErrorMessage) != errorBefore));
            }
            catch (WebDriverTimeoutException)
            {
                LogStep($"Member {who}", "No eligibility result or error appeared within 60 seconds.");
                return $"{who}: no result within 60 seconds";
            }

            var error = page.VisibleText(page.ErrorMessage);
            if (!string.IsNullOrEmpty(error) && error != errorBefore)
            {
                if (error.Contains("No records returned", StringComparison.OrdinalIgnoreCase))
                {
                    LogStep($"Member {who}", $"Search completed with no eligibility records: {error}");
                    return null;
                }

                LogStep($"Member {who}", $"Search failed: {error}");
                return $"{who}: {error}";
            }

            var recipient = Shorten(page.SectionText(page.RecipientInformationHeader));
            var plans = page.IsDisplayed(page.BenefitPlansHeader) ? Shorten(page.SectionText(page.BenefitPlansHeader)) : "no benefit/assignment plan section shown";
            LogStep($"Member {who}",
                $"Eligibility found for DOB {FormatDate(member.DateOfBirth)}, DOS {FormatDate(member.FromDateOfService)}-{FormatDate(member.ToDateOfService)}. Recipient: {recipient}. Plans: {plans}");
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

        private static string Shorten(string? text)
        {
            var flat = string.Join(" ", (text ?? "").Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries).Select(t => t.Trim()));
            return flat.Length > 300 ? flat.Substring(0, 300) + "…" : flat;
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
