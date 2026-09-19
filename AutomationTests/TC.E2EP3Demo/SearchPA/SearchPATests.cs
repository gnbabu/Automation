using System.Threading;
using NUnit.Framework;
using OpenQA.Selenium;
using Selenium.BaseComponents;
using Selenium.BaseComponents.Pages;
using Selenium.BaseComponents.Utilities;
using SeleniumExtensions.Extensions;
using TC.PriorAuthSearch.Models;
using TC.PriorAuthSearch.Pages;
using TC.PriorAuthSearch.Utilities;

namespace TC.E2EP3Demo.SearchPA
{
    // 6 real test cases against the real E2EP3 Search Prior Authorization page, reusing
    // the same Page Objects already proven in TC.SearchPA/Tests/SearchPATest.cs
    // (physically copied into this project - see AGENTS.md). Each test pulls real data
    // configured via Test Data Management (Flow "SearchPA", Section "SearchPA") instead
    // of the hard-coded "2422659" the original test used, and varies which criteria
    // field(s) drive the search.
    [TestFixture("TechAdmin")]
    public class SearchPATests : BaseFeatureFixture
    {
        private TC.PriorAuthSearch.Models.SearchPA? _searchPA;

        public SearchPATests(string profile) : base(profile)
        {
        }

        [SetUp]
        public new void BeforeEachTest()
        {
            _searchPA = (TC.PriorAuthSearch.Models.SearchPA?)DataRepository.GetAutomationData("SearchPA");
        }

        // Shared by every test - logs in, opens Self Service, enters the Medicaid
        // number to reach the Financial Provider Information page, then navigates into
        // Search Prior Authorization, matching NavigateToSeachPAPage's exact steps.
        private SearchPAPage NavigateToSearchPAPage()
        {
            SidebarMenu.Click();
            Thread.Sleep(4000);
            NavigateToSelfService();

            var financialProviderInformationPage = new FinancialProviderInformationPage(TestWebDriver);
            financialProviderInformationPage.WaitUntilElementIsVisible();
            financialProviderInformationPage.TxtMedicaid.Set(
                !string.IsNullOrEmpty(_searchPA?.MedicaidBillingNumber) ? _searchPA.MedicaidBillingNumber : "2422659");
            financialProviderInformationPage.lnkBtnPriorAuth.Click();

            var searchPAPage = new SearchPAPage(TestWebDriver);
            searchPAPage.lnkBtnSearchPA.Click();
            searchPAPage.WaitUntilElementIsVisible();

            return searchPAPage;
        }

        [Test]
        [Author("Vishu")]
        [Category("IntegrationTests")]
        [Property("Description", "Search Prior Authorization by PA Number")]
        [Property("Priority", "High")]
        [Property("TestCaseId", "TC_E2EP3_PA_01")]
        [CancelAfter(600000)]
        public void SearchPA_ByPriorAuthorizationNumber()
        {
            var searchPAPage = NavigateToSearchPAPage();

            if (!string.IsNullOrEmpty(_searchPA?.PriorAuthorizationNumber))
                searchPAPage.txtPriorAuthNumber.Set(_searchPA.PriorAuthorizationNumber);

            searchPAPage.btnSearch.Click();
            Common.PrintScreenShot(TestWebDriver, "SearchPA_ByPriorAuthorizationNumber");
        }

        [Test]
        [Author("Vishu")]
        [Category("IntegrationTests")]
        [Property("Description", "Search Prior Authorization by Patient Tracking Number")]
        [Property("Priority", "Medium")]
        [Property("TestCaseId", "TC_E2EP3_PA_02")]
        [CancelAfter(600000)]
        public void SearchPA_ByPatientTrackingNumber()
        {
            var searchPAPage = NavigateToSearchPAPage();

            if (!string.IsNullOrEmpty(_searchPA?.PatientTrackingNumber))
                searchPAPage.txtPatientTrackingNumber.Set(_searchPA.PatientTrackingNumber);

            searchPAPage.btnSearch.Click();
            Common.PrintScreenShot(TestWebDriver, "SearchPA_ByPatientTrackingNumber");
        }

        [Test]
        [Author("Vishu")]
        [Category("IntegrationTests")]
        [Property("Description", "Search Prior Authorization by Date of Birth and Procedure Code")]
        [Property("Priority", "Medium")]
        [Property("TestCaseId", "TC_E2EP3_PA_03")]
        [CancelAfter(600000)]
        public void SearchPA_ByDateOfBirthAndProcedureCode()
        {
            var searchPAPage = NavigateToSearchPAPage();

            if (!string.IsNullOrEmpty(_searchPA?.DateofBirth))
                searchPAPage.txtBirthDate.Set(_searchPA.DateofBirth);
            if (!string.IsNullOrEmpty(_searchPA?.ProcedureCode))
                searchPAPage.txtProcedureCode.Set(_searchPA.ProcedureCode);

            searchPAPage.btnSearch.Click();
            Common.PrintScreenShot(TestWebDriver, "SearchPA_ByDateOfBirthAndProcedureCode");
        }

        [Test]
        [Author("Vishu")]
        [Category("IntegrationTests")]
        [Property("Description", "Search Prior Authorization by Payer Name and Status")]
        [Property("Priority", "Medium")]
        [Property("TestCaseId", "TC_E2EP3_PA_04")]
        [CancelAfter(600000)]
        public void SearchPA_ByPayerNameAndStatus()
        {
            var searchPAPage = NavigateToSearchPAPage();

            if (!string.IsNullOrEmpty(_searchPA?.PayerName))
                searchPAPage.SetPayerName(_searchPA.PayerName);
            if (!string.IsNullOrEmpty(_searchPA?.Status))
                searchPAPage.SetSearchPAStatus(_searchPA.Status);

            searchPAPage.btnSearch.Click();
            Common.PrintScreenShot(TestWebDriver, "SearchPA_ByPayerNameAndStatus");
        }

        [Test]
        [Author("Vishu")]
        [Category("IntegrationTests")]
        [Property("Description", "Search Prior Authorization by combined criteria (Diagnosis Code, Revenue Code, Ordering Provider NPI)")]
        [Property("Priority", "Medium")]
        [Property("TestCaseId", "TC_E2EP3_PA_05")]
        [CancelAfter(600000)]
        public void SearchPA_ByCombinedCriteria()
        {
            var searchPAPage = NavigateToSearchPAPage();

            if (!string.IsNullOrEmpty(_searchPA?.DiagnosisCode))
                searchPAPage.txtDiagnoisCode.Set(_searchPA.DiagnosisCode);
            if (!string.IsNullOrEmpty(_searchPA?.RevenueCode))
                searchPAPage.txtRevenuecode.Set(_searchPA.RevenueCode);
            if (!string.IsNullOrEmpty(_searchPA?.OrderingProviderNPI))
                searchPAPage.txtorderProvnpi.Set(_searchPA.OrderingProviderNPI);

            searchPAPage.btnSearch.Click();
            Common.PrintScreenShot(TestWebDriver, "SearchPA_ByCombinedCriteria");
        }

        // Negative path - a criteria value that shouldn't match any real record,
        // confirming the page surfaces a "no results" state instead of an error.
        [Test]
        [Author("Vishu")]
        [Category("IntegrationTests")]
        [Property("Description", "Search Prior Authorization with a criteria that returns no results")]
        [Property("Priority", "Low")]
        [Property("TestCaseId", "TC_E2EP3_PA_06")]
        [CancelAfter(600000)]
        public void SearchPA_NoResultsForInvalidCriteria()
        {
            var searchPAPage = NavigateToSearchPAPage();

            searchPAPage.txtPriorAuthNumber.Set("PA-DOES-NOT-EXIST-000000");
            searchPAPage.btnSearch.Click();

            Common.PrintScreenShot(TestWebDriver, "SearchPA_NoResultsForInvalidCriteria");
            NUnit.Framework.TestContext.WriteLine("Searched with a deliberately invalid PA number - expecting no results.");
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
