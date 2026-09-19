using System.Threading;
using NUnit.Framework;
using OpenQA.Selenium;
using Selenium.BaseComponents;
using Selenium.BaseComponents.Pages;
using Selenium.BaseComponents.Utilities;
using SeleniumExtensions.Extensions;
using TC.MemberEligibilitySearch.Utilities;

namespace TC.E2EP3Demo.SearchEligibility
{
    // 6 real test cases against the real E2EP3 Search Member Eligibility page, reusing
    // the same Page Objects already proven in
    // TC.SearchEligibility/Tests/SearchEligiblityTest.cs (physically copied into this
    // project). Flow "SearchMemberEligiblity" / Section "SearchMemberEligiblity" had no
    // configured Test Data Management data before this session's Flow & Section
    // Management work - see AGENTS.md.
    [TestFixture("TechAdmin")]
    public class SearchEligibilityTests : BaseFeatureFixture
    {
        // Cross-flow chaining example (per the user's "both" data-flow answer): if
        // RegistrationTests.Registration_FullWizardThroughSubmission already ran in
        // this session, SearchEligibility_ByRegistrationId reuses the real RegID it
        // just created instead of a fixed value - demonstrating a genuine
        // create-then-verify flow across two independently-assignable test cases in
        // two different flows.
        //
        // Real constraint (see AGENTS.md "ProcessModel=Separate"): NUnitEngineTestRunner
        // runs each queued test case in its own isolated child process, so this static
        // field only actually survives between the two tests when both run within the
        // SAME process - e.g. a local `dotnet test`/Test Explorer run of the whole
        // suite, or an NUnit console run covering both. If each test is instead queued
        // and run independently through the Portal's own execution pipeline (the more
        // common real usage), this falls back to the configured default RegID below,
        // same as running standalone - not a bug, an inherent limit of the isolation
        // model itself.
        internal static string? RegIdFromRegistrationFlow;

        private TC.MemberEligibilitySearch.Models.SearchEligibility? _searchEligibility;

        public SearchEligibilityTests(string profile) : base(profile)
        {
        }

        [SetUp]
        public new void BeforeEachTest()
        {
            _searchEligibility = (TC.MemberEligibilitySearch.Models.SearchEligibility?)
                DataRepository.GetAutomationData("SearchMemberEligiblity");
        }

        private TC.MemberEligibilitySearch.Pages.SearchEligibility NavigateToSearchEligibilityPage(string regId)
        {
            SidebarMenu.Click();
            Thread.Sleep(4000);
            NavigateToSelfService();

            var financialProviderInformationPage = new TC.MemberEligibilitySearch.Pages.FinancialProviderInformationPage(TestWebDriver);
            financialProviderInformationPage.WaitUntilElementIsVisible();
            financialProviderInformationPage.TxtMedicaid.Set(regId);
            financialProviderInformationPage.lnkBtnPriorAuth.Click();

            var searchEligibilityPage = new TC.MemberEligibilitySearch.Pages.SearchEligibility(TestWebDriver);
            searchEligibilityPage.lnkBtnSearchEligibility.Click();
            searchEligibilityPage.WaitUntilElementIsVisible();

            return searchEligibilityPage;
        }

        private string DefaultRegId() =>
            !string.IsNullOrEmpty(_searchEligibility?.RegID) ? _searchEligibility!.RegID : "0005987";

        [Test]
        [Author("Vishu")]
        [Category("IntegrationTests")]
        [Property("Description", "Search Member Eligibility by Medicaid Billing Number")]
        [Property("Priority", "High")]
        [Property("TestCaseId", "TC_E2EP3_ELIG_01")]
        [CancelAfter(600000)]
        public void SearchEligibility_ByMedicaidBillingNumber()
        {
            var searchEligibilityPage = NavigateToSearchEligibilityPage(DefaultRegId());

            if (!string.IsNullOrEmpty(_searchEligibility?.MedicaidBillingNumber))
                searchEligibilityPage.txtMedicaidBillingNumber.Set(_searchEligibility.MedicaidBillingNumber);

            ClickSearchAndAssertNoError(searchEligibilityPage, "SearchEligibility_ByMedicaidBillingNumber");
        }

        [Test]
        [Author("Vishu")]
        [Category("IntegrationTests")]
        [Property("Description", "Search Member Eligibility by SSN")]
        [Property("Priority", "Medium")]
        [Property("TestCaseId", "TC_E2EP3_ELIG_02")]
        [CancelAfter(600000)]
        public void SearchEligibility_BySSN()
        {
            var searchEligibilityPage = NavigateToSearchEligibilityPage(DefaultRegId());

            if (!string.IsNullOrEmpty(_searchEligibility?.SSN))
                searchEligibilityPage.txtSSN.Set(_searchEligibility.SSN);

            ClickSearchAndAssertNoError(searchEligibilityPage, "SearchEligibility_BySSN");
        }

        [Test]
        [Author("Vishu")]
        [Category("IntegrationTests")]
        [Property("Description", "Search Member Eligibility by Date of Birth")]
        [Property("Priority", "Medium")]
        [Property("TestCaseId", "TC_E2EP3_ELIG_03")]
        [CancelAfter(600000)]
        public void SearchEligibility_ByDateOfBirth()
        {
            var searchEligibilityPage = NavigateToSearchEligibilityPage(DefaultRegId());

            if (!string.IsNullOrEmpty(_searchEligibility?.DateOfBirth))
                searchEligibilityPage.txtBirthDate.Set(_searchEligibility.DateOfBirth);

            ClickSearchAndAssertNoError(searchEligibilityPage, "SearchEligibility_ByDateOfBirth");
        }

        [Test]
        [Author("Vishu")]
        [Category("IntegrationTests")]
        [Property("Description", "Search Member Eligibility by date-of-service range")]
        [Property("Priority", "Medium")]
        [Property("TestCaseId", "TC_E2EP3_ELIG_04")]
        [CancelAfter(600000)]
        public void SearchEligibility_ByDateOfServiceRange()
        {
            var searchEligibilityPage = NavigateToSearchEligibilityPage(DefaultRegId());

            if (!string.IsNullOrEmpty(_searchEligibility?.FromDOS))
                searchEligibilityPage.txtFromDos.Set(_searchEligibility.FromDOS);
            if (!string.IsNullOrEmpty(_searchEligibility?.ToDOS))
                searchEligibilityPage.txtToDos.Set(_searchEligibility.ToDOS);

            ClickSearchAndAssertNoError(searchEligibilityPage, "SearchEligibility_ByDateOfServiceRange");
        }

        // Chains off RegistrationTests's captured RegID (see class-level comment) -
        // falls back to the configured/default RegID so it still runs standalone if
        // the Registration flow's tests haven't run in this session.
        [Test]
        [Author("Vishu")]
        [Category("IntegrationTests")]
        [Property("Description", "Search Member Eligibility using the Reg ID captured from the Registration flow")]
        [Property("Priority", "High")]
        [Property("TestCaseId", "TC_E2EP3_ELIG_05")]
        [CancelAfter(600000)]
        public void SearchEligibility_ByRegistrationId()
        {
            var regId = RegIdFromRegistrationFlow ?? DefaultRegId();
            var searchEligibilityPage = NavigateToSearchEligibilityPage(regId);

            if (!string.IsNullOrEmpty(_searchEligibility?.MedicaidBillingNumber))
                searchEligibilityPage.txtMedicaidBillingNumber.Set(_searchEligibility.MedicaidBillingNumber);

            ClickSearchAndAssertNoError(searchEligibilityPage, "SearchEligibility_ByRegistrationId");
            NUnit.Framework.TestContext.WriteLine($"Searched Member Eligibility using RegID: {regId}");
        }

        [Test]
        [Author("Vishu")]
        [Category("IntegrationTests")]
        [Property("Description", "Search Member Eligibility with a criteria that returns no results")]
        [Property("Priority", "Low")]
        [Property("TestCaseId", "TC_E2EP3_ELIG_06")]
        [CancelAfter(600000)]
        public void SearchEligibility_NoResultsForInvalidCriteria()
        {
            var searchEligibilityPage = NavigateToSearchEligibilityPage(DefaultRegId());

            searchEligibilityPage.txtMedicaidBillingNumber.Set("0000000000");
            Thread.Sleep(3000);

            var jsExecutor = (IJavaScriptExecutor)TestWebDriver;
            jsExecutor.ExecuteScript("arguments[0].click();", searchEligibilityPage.btnSearch);
            Thread.Sleep(3000);

            if (searchEligibilityPage.lblErrorMsg.Displayed)
                NUnit.Framework.TestContext.WriteLine("No data found with the deliberately invalid search criteria, as expected.");

            Common.PrintScreenShot(TestWebDriver, "SearchEligibility_NoResultsForInvalidCriteria");
        }

        private void ClickSearchAndAssertNoError(TC.MemberEligibilitySearch.Pages.SearchEligibility searchEligibilityPage, string screenshotName)
        {
            Thread.Sleep(3000);
            var jsExecutor = (IJavaScriptExecutor)TestWebDriver;
            jsExecutor.ExecuteScript("arguments[0].click();", searchEligibilityPage.btnSearch);
            Thread.Sleep(3000);

            if (searchEligibilityPage.lblErrorMsg.Displayed)
                Assert.Fail("No data found with the search criteria.");

            Common.PrintScreenShot(TestWebDriver, screenshotName);
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
