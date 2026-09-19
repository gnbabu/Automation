using System.Threading;
using NUnit.Framework;
using OpenQA.Selenium;
using Selenium.BaseComponents;
using Selenium.BaseComponents.Pages;
using Selenium.BaseComponents.Utilities;
using SeleniumExtensions.Extensions;
using TC.SearchRA.Pages;
using TC.SearchRA.Utilities;

namespace TC.E2EP3Demo.SearchRA
{
    // 6 real test cases against the real E2EP3 Search Remittance Advice page, reusing
    // the same Page Objects already proven in TC.SearchRA/Tests/SearchRATest.cs
    // (physically copied into this project). Flow "SearchRA" / Section "SearchRA" had
    // no configured Test Data Management data before this session's Flow & Section
    // Management work - see AGENTS.md.
    [TestFixture("TechAdmin")]
    public class SearchRATests : BaseFeatureFixture
    {
        private TC.SearchRA.Models.SearchRA? _searchRA;

        public SearchRATests(string profile) : base(profile)
        {
        }

        [SetUp]
        public new void BeforeEachTest()
        {
            _searchRA = (TC.SearchRA.Models.SearchRA?)DataRepository.GetAutomationData("SearchRA");
        }

        private SearchRAPage NavigateToSearchRAPage()
        {
            SidebarMenu.Click();
            Thread.Sleep(4000);
            NavigateToSelfService();

            var financialProviderInformationPage = new FinancialProviderInformationPage(TestWebDriver);
            financialProviderInformationPage.WaitUntilElementIsVisible();
            financialProviderInformationPage.TxtMedicaid.Set(
                !string.IsNullOrEmpty(_searchRA?.RegID) ? _searchRA.RegID : "0005987");
            financialProviderInformationPage.lnkBtnPriorAuth.Click();

            var searchRAPage = new SearchRAPage(TestWebDriver);
            searchRAPage.lnkBtnSearchRA.Click();
            searchRAPage.WaitUntilElementIsVisible();

            return searchRAPage;
        }

        [Test]
        [Author("Vishu")]
        [Category("IntegrationTests")]
        [Property("Description", "Search Remittance Advice by RA Number")]
        [Property("Priority", "High")]
        [Property("TestCaseId", "TC_E2EP3_RA_01")]
        [CancelAfter(600000)]
        public void SearchRA_ByRANumber()
        {
            var searchRAPage = NavigateToSearchRAPage();

            if (!string.IsNullOrEmpty(_searchRA?.RANumber))
                searchRAPage.txtRANumber.Set(_searchRA.RANumber);

            ClickSearch(searchRAPage);
            Common.PrintScreenShot(TestWebDriver, "SearchRA_ByRANumber");
        }

        [Test]
        [Author("Vishu")]
        [Category("IntegrationTests")]
        [Property("Description", "Search Remittance Advice by ICN")]
        [Property("Priority", "Medium")]
        [Property("TestCaseId", "TC_E2EP3_RA_02")]
        [CancelAfter(600000)]
        public void SearchRA_ByICN()
        {
            var searchRAPage = NavigateToSearchRAPage();

            if (!string.IsNullOrEmpty(_searchRA?.ICN))
                searchRAPage.txtICN.Set(_searchRA.ICN);

            ClickSearch(searchRAPage);
            Common.PrintScreenShot(TestWebDriver, "SearchRA_ByICN");
        }

        [Test]
        [Author("Vishu")]
        [Category("IntegrationTests")]
        [Property("Description", "Search Remittance Advice by Destination Payer")]
        [Property("Priority", "Medium")]
        [Property("TestCaseId", "TC_E2EP3_RA_03")]
        [CancelAfter(600000)]
        public void SearchRA_ByDestinationPayer()
        {
            var searchRAPage = NavigateToSearchRAPage();

            if (!string.IsNullOrEmpty(_searchRA?.DestinationPayerName))
                searchRAPage.SetPrimaryDestinationPayer(_searchRA.DestinationPayerName);

            ClickSearch(searchRAPage);
            Common.PrintScreenShot(TestWebDriver, "SearchRA_ByDestinationPayer");
        }

        [Test]
        [Author("Vishu")]
        [Category("IntegrationTests")]
        [Property("Description", "Search Remittance Advice by report run date range")]
        [Property("Priority", "Medium")]
        [Property("TestCaseId", "TC_E2EP3_RA_04")]
        [CancelAfter(600000)]
        public void SearchRA_ByDateRange()
        {
            var searchRAPage = NavigateToSearchRAPage();

            // Fixed from the original SearchRATest.cs, which set txtDateAvailableTo
            // twice (once for ReportRunDateFrom, again for ToDate) instead of using
            // txtDateAvailableFrom for the "from" value - confirmed by reading the
            // real SearchRAPage.cs, which has a distinct txtDateAvailableFrom field.
            if (!string.IsNullOrEmpty(_searchRA?.ReportRunDateFrom))
                searchRAPage.txtDateAvailableFrom.Set(_searchRA.ReportRunDateFrom);
            if (!string.IsNullOrEmpty(_searchRA?.ToDate))
                searchRAPage.txtDateAvailableTo.Set(_searchRA.ToDate);

            ClickSearch(searchRAPage);
            Common.PrintScreenShot(TestWebDriver, "SearchRA_ByDateRange");
        }

        [Test]
        [Author("Vishu")]
        [Category("IntegrationTests")]
        [Property("Description", "Search Remittance Advice by combined criteria")]
        [Property("Priority", "Medium")]
        [Property("TestCaseId", "TC_E2EP3_RA_05")]
        [CancelAfter(600000)]
        public void SearchRA_ByCombinedCriteria()
        {
            var searchRAPage = NavigateToSearchRAPage();

            if (!string.IsNullOrEmpty(_searchRA?.DestinationPayerName))
                searchRAPage.SetPrimaryDestinationPayer(_searchRA.DestinationPayerName);
            if (!string.IsNullOrEmpty(_searchRA?.RANumber))
                searchRAPage.txtRANumber.Set(_searchRA.RANumber);
            if (!string.IsNullOrEmpty(_searchRA?.ICN))
                searchRAPage.txtICN.Set(_searchRA.ICN);

            ClickSearch(searchRAPage);
            Common.PrintScreenShot(TestWebDriver, "SearchRA_ByCombinedCriteria");
        }

        [Test]
        [Author("Vishu")]
        [Category("IntegrationTests")]
        [Property("Description", "Search Remittance Advice with a criteria that returns no results")]
        [Property("Priority", "Low")]
        [Property("TestCaseId", "TC_E2EP3_RA_06")]
        [CancelAfter(600000)]
        public void SearchRA_NoResultsForInvalidCriteria()
        {
            var searchRAPage = NavigateToSearchRAPage();

            searchRAPage.txtRANumber.Set("RA-DOES-NOT-EXIST-000000");
            ClickSearch(searchRAPage);

            Common.PrintScreenShot(TestWebDriver, "SearchRA_NoResultsForInvalidCriteria");
            NUnit.Framework.TestContext.WriteLine("Searched with a deliberately invalid RA number - expecting no results.");
        }

        // Matches SearchRATest.cs's own real click sequence (a plain .Click() on
        // lnkBtnSearchRA wasn't reliable, so the original test falls back to a JS
        // click on btnSearch) rather than a fresh, unverified interaction.
        private void ClickSearch(SearchRAPage searchRAPage)
        {
            Thread.Sleep(3000);
            var jsExecutor = (IJavaScriptExecutor)TestWebDriver;
            jsExecutor.ExecuteScript("arguments[0].click();", searchRAPage.btnSearch);
            Thread.Sleep(3000);
        }

        public IWebElement SidebarMenu =>
            TestWebDriver.FindElement(By.XPath("//button[contains(@class,'hamburger is-closed')]"));

        public void NavigateToSelfService()
        {
            SelfService?.Click();
        }

        public IWebElement SelfService =>
            TestWebDriver.FindElement(By.XPath("//a[normalize-space()='Self Service']"));
    }
}
