
using NUnit.Framework;
using TC.SearchRA.Pages;
 
using OpenQA.Selenium;
using SdetToolbox.Pages;
using Selenium.BaseComponents;
using Selenium.BaseComponents.Pages;
// Aliased, not a blanket `using Selenium.BaseComponents.Utilities;` - that would make
// TestWebDriver.FindElement(...) ambiguous between SdetToolbox.Pages.PageHelper and
// Selenium.BaseComponents.Utilities.PageHelper's identical extension method signatures
// (confirmed by direct testing - a full CS0121 build error).
using TestCaseExecutionLog = Selenium.BaseComponents.Utilities.TestCaseExecutionLog;
using TestCaseLogLevel = Selenium.BaseComponents.Utilities.TestCaseLogLevel;
using ExecutionStatus = Selenium.BaseComponents.Utilities.ExecutionStatus;
using Screeshots = Selenium.BaseComponents.Utilities.Screeshots;
using TestScreenshot = Selenium.BaseComponents.Utilities.TestScreenshot;
using Common = Selenium.BaseComponents.Utilities.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SeleniumExtensions.Extensions;
using OpenQA.Selenium.Support.UI;
using SeleniumExtensions.Configurations;
using static System.Net.Mime.MediaTypeNames;
using System.Xml.Linq;
using System.Diagnostics;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;

namespace TC.SearchRA.Tests
{
    [TestFixture("TechAdmin")]
    public class SearchRATest : BaseFeatureFixture
    {
        private Screeshots? _screenshots;

        public SearchRATest(string profile) : base(profile)
        {

        }


        [Test]
        [Author("Vishnuvardhan Reddy")]
        [Category("IntegrationTests")]
        [Property("Description", "Search Remittance Advice")]
        [Property("Priority", "Medium")]
        [Property("TestCaseId", "TCSearchRA")]
        [CancelAfter(600000)]
        public void SearchRA()
        {
            TestCaseExecutionLog testCaseExecutionLog = new TestCaseExecutionLog
            {
                AssignmentId = AssignmentId,
                AssignmentTestCaseId = AssignmentTestCaseId,
                LogLevel = TestCaseLogLevel.Info,
                ExecutionStatus = ExecutionStatus.Running,
                TestCaseId = "TCSearchRA",
                TestCaseDescription = "Search Remittance Advice"
            };

            _screenshots = new Screeshots
            {
                AssignmentTestCaseId = AssignmentTestCaseId,
                screenShot = new List<byte[]>()
            };

            TC.SearchRA.Models.SearchRA searchRA = (TC.SearchRA.Models.SearchRA)TC.SearchRA.Utilities.DataRepository.GetAutomationData("SearchRA");

            testCaseExecutionLog.LogMessage = "Login to PNM succesfully...!";
            testCaseExecutionLog.StepName = "Login to PNM";
            SaveLog(testCaseExecutionLog);
            _screenshots.screenShot.Add(Common.PrintScreenShot(TestWebDriver, "LoginCompletedSuccessfully"));

            SidebarMenu.Click();
            Thread.Sleep(4000);
            NavigateToSelfService();

            testCaseExecutionLog.LogMessage = "Navigating to self service...!";
            testCaseExecutionLog.StepName = "Self Service";
            SaveLog(testCaseExecutionLog);
            _screenshots.screenShot.Add(Common.PrintScreenShot(TestWebDriver, "NavigatingToSelfService"));

            TC.SearchRA.Pages.FinancialProviderInformationPage financialProviderInformationPage = new TC.SearchRA.Pages.FinancialProviderInformationPage(TestWebDriver);
            financialProviderInformationPage.WaitUntilElementIsVisible();
            financialProviderInformationPage.TxtMedicaid.Set(searchRA != null && !string.IsNullOrEmpty(searchRA.RegID) ? searchRA.RegID : "0005987");
            financialProviderInformationPage.lnkBtnPriorAuth.Click();

            testCaseExecutionLog.LogMessage = $"Entered {(searchRA != null && !string.IsNullOrEmpty(searchRA.RegID) ? searchRA.RegID : "0005987")} for search";
            testCaseExecutionLog.StepName = "Medicaid Search";
            SaveLog(testCaseExecutionLog);
            _screenshots.screenShot.Add(Common.PrintScreenShot(TestWebDriver, "MedicaidEntered"));

            TC.SearchRA.Pages.SearchRAPage searchRAPage = new TC.SearchRA.Pages.SearchRAPage(TestWebDriver);
            searchRAPage.lnkBtnSearchRA.Click();
            searchRAPage.WaitUntilElementIsVisible();

            testCaseExecutionLog.LogMessage = "Navigated to Search RA page";
            testCaseExecutionLog.StepName = "RA Search";
            SaveLog(testCaseExecutionLog);
            _screenshots.screenShot.Add(Common.PrintScreenShot(TestWebDriver, "SearchRAPageLoaded"));

            if (searchRA != null)
            {

                if (!string.IsNullOrEmpty(searchRA.DestinationPayerName))
                    searchRAPage.SetPrimaryDestinationPayer(searchRA.DestinationPayerName);

                if (!string.IsNullOrEmpty(searchRA.RANumber))
                    searchRAPage.txtRANumber.Set(searchRA.RANumber);

                if (!string.IsNullOrEmpty(searchRA.ICN))
                    searchRAPage.txtICN.Set(searchRA.ICN);

                if (!string.IsNullOrEmpty(searchRA.ReportRunDateFrom))
                    searchRAPage.txtDateAvailableTo.Set(searchRA.ReportRunDateFrom);

                if (!string.IsNullOrEmpty(searchRA.ToDate))
                    searchRAPage.txtDateAvailableTo.Set(searchRA.ToDate);

            }

            testCaseExecutionLog.LogMessage = "SearchButton Clicked";
            testCaseExecutionLog.StepName = "RA Search Submitted";
            SaveLog(testCaseExecutionLog);
            _screenshots.screenShot.Add(Common.PrintScreenShot(TestWebDriver, "SearchButton Clicked"));

            Thread.Sleep(3000);

            IJavaScriptExecutor jsExecutor;

            jsExecutor = (IJavaScriptExecutor)TestWebDriver;
            jsExecutor.ExecuteScript("arguments[0].click();", searchRAPage.btnSearch);
            Thread.Sleep(3000);

            IWebElement searchTable = TestWebDriver.FindElement(By.Id("ctl00_MainContent_ERemittanceAdvice_gvRemittanceAdvicesearch"));

            if (searchTable.Displayed)
            {
                IWebElement tbody = searchTable.FindElement(By.TagName("tbody"));
                if (tbody.Displayed)
                {
                    IWebElement emptydataRow = tbody.FindElement(By.ClassName("gridViewEmptyRow"));

                    if (emptydataRow.Displayed)
                    {
                        IWebElement emptytd = emptydataRow.FindElement(By.TagName("td"));

                        if (emptytd.Displayed)
                        {
                            string searchMessage = emptytd.Text;
                           NUnit.Framework.TestContext.WriteLine($"No data found with the search criteria.");
                        }
                    }
                }
            }

            testCaseExecutionLog.LogMessage = "Success";
            testCaseExecutionLog.StepName = "Test case executed successfully...!";
            SaveLog(testCaseExecutionLog);
            _screenshots.screenShot.Add(Common.PrintScreenShot(TestWebDriver, "Success"));
        }

        [TearDown]
        public void AfterTest()
        {
            if (_screenshots?.screenShot == null || !_screenshots.screenShot.Any())
                return;

            if (_screenshots.AssignmentTestCaseId == 0)
                return;

            var screenshots = _screenshots.screenShot
                .Select((screen, index) => new TestScreenshot
                {
                    ID = index + 1,
                    AssignmentTestCaseId = Convert.ToInt32(_screenshots.AssignmentTestCaseId),
                    Caption = $"Screenshot_{index + 1}",
                    Screenshot = $"data:image/png;base64,{Convert.ToBase64String(screen)}",
                    TakenAt = DateTime.Now,
                })
                .ToList();

            APIGateway.SaveMethodScreenShots(screenshots);
        }

        public void SaveLog(TestCaseExecutionLog testCaseExecutionLog)
        {
            APIGateway.SaveTestCaseLog(testCaseExecutionLog);
        }

        public IWebElement SidebarMenu
        {
            get
            {
                return TestWebDriver.FindElement(By.XPath("//button[contains(@class,'hamburger is-closed')]"), null);
            }
        }
        public void NavigateToSelfService()
        {
            if (SelfService != null)
            {
                SelfService.Click();
            }
        }
        public IWebElement SelfService
        {
            get
            {
                return TestWebDriver.FindElement(By.XPath($"//a[@title='Self Service']"), null);

            }
        }
    }
}