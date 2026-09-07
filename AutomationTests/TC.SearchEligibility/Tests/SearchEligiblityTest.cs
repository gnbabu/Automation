
using NUnit.Framework;
using TC.MemberEligibilitySearch.Pages;
using OpenQA.Selenium;
using SdetToolbox.Pages;
using Selenium.BaseComponents;
using Selenium.BaseComponents.Pages;
using Selenium.BaseComponents.Utilities;
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
using TC.MemberEligibilitySearch.Utilities;
using TC.MemberEligibilitySearch.Models;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;

namespace TC.MemberEligibilitySearch.Tests
{
    [TestFixture("TechAdmin")]
    public class SearchEligiblityTest : BaseFeatureFixture
    {
        private Screeshots? _screenshots;

        public SearchEligiblityTest(string profile) : base(profile)
        {

        }


        [Test]
        [Author("Vishnuvardhan Reddy")]
        [Category("IntegrationTests")]
        [Property("Description", "Search Member Eligibility")]
        [Property("Priority", "Medium")]
        [Property("TestCaseId", "TCSearchEligibility")]
        [CancelAfter(600000)]
        public void SearchMemberEligibility()
        {
            TestCaseExecutionLog testCaseExecutionLog = new TestCaseExecutionLog
            {
                AssignmentId = AssignmentId,
                AssignmentTestCaseId = AssignmentTestCaseId,
                LogLevel = TestCaseLogLevel.Info,
                ExecutionStatus = ExecutionStatus.Running,
                TestCaseId = "TCSearchEligibility",
                TestCaseDescription = "Search Member Eligibility"
            };

            _screenshots = new Screeshots
            {
                AssignmentTestCaseId = AssignmentTestCaseId,
                screenShot = new List<byte[]>()
            };

            TC.MemberEligibilitySearch.Models.SearchEligibility searchEligibility = (TC.MemberEligibilitySearch.Models.SearchEligibility)DataRepository.GetAutomationData("SearchMemberEligiblity");

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

            FinancialProviderInformationPage financialProviderInformationPage = new FinancialProviderInformationPage(TestWebDriver);
            financialProviderInformationPage.WaitUntilElementIsVisible();
            financialProviderInformationPage.TxtMedicaid.Set(searchEligibility != null && !string.IsNullOrEmpty(searchEligibility.RegID) ? 
                searchEligibility.RegID : "0005987");

            financialProviderInformationPage.lnkBtnPriorAuth.Click();

            testCaseExecutionLog.LogMessage = $"Entered {(searchEligibility != null && !string.IsNullOrEmpty(searchEligibility.RegID) ? searchEligibility.RegID : "0005987")} for search";
            testCaseExecutionLog.StepName = "Medicaid Search";
            SaveLog(testCaseExecutionLog);
            _screenshots.screenShot.Add(Common.PrintScreenShot(TestWebDriver, "MedicaidEntered"));

            TC.MemberEligibilitySearch.Pages.SearchEligibility searchEligibilityPage = new TC.MemberEligibilitySearch.Pages.SearchEligibility(TestWebDriver);
            searchEligibilityPage.lnkBtnSearchEligibility.Click();
            searchEligibilityPage.WaitUntilElementIsVisible();

            testCaseExecutionLog.LogMessage = "Navigated to Search Eligibility page";
            testCaseExecutionLog.StepName = "Eligibility Search";
            SaveLog(testCaseExecutionLog);
            _screenshots.screenShot.Add(Common.PrintScreenShot(TestWebDriver, "SearchEligibilityPageLoaded"));

            if (searchEligibility != null)
            {

                if (!string.IsNullOrEmpty(searchEligibility.MedicaidBillingNumber))
                    searchEligibilityPage.txtMedicaidBillingNumber.Set(searchEligibility.MedicaidBillingNumber);

                if (!string.IsNullOrEmpty(searchEligibility.DateOfBirth))
                    searchEligibilityPage.txtBirthDate.Set(searchEligibility.DateOfBirth);

                if (!string.IsNullOrEmpty(searchEligibility.FromDOS))
                    searchEligibilityPage.txtFromDos.Set(searchEligibility.FromDOS);

                if (!string.IsNullOrEmpty(searchEligibility.ToDOS))
                    searchEligibilityPage.txtToDos.Set(searchEligibility.ToDOS);

                if (!string.IsNullOrEmpty(searchEligibility.SSN))
                    searchEligibilityPage.txtSSN.Set(searchEligibility.SSN);

            }

            Thread.Sleep(3000);

            IJavaScriptExecutor jsExecutor;

            jsExecutor = (IJavaScriptExecutor)TestWebDriver;
            jsExecutor.ExecuteScript("arguments[0].click();", searchEligibilityPage.btnSearch);
            Thread.Sleep(3000);

            if (searchEligibilityPage.lblErrorMsg.Displayed)
            {
                NUnit.Framework.Assert.Fail($"No data found with the search criteria.");
            }

            string medicaidBillingNumber = searchEligibilityPage.txtRecinfoMedicaidbillNumber.GetAttribute("value");
            string lastName = searchEligibilityPage.txtLast.GetAttribute("value");
            string firstname = searchEligibilityPage.txtFirstName.GetAttribute("value");
            string dob = searchEligibilityPage.txtDOB.GetAttribute("value");

            if (!string.IsNullOrEmpty(medicaidBillingNumber) && !string.IsNullOrEmpty(lastName) && !string.IsNullOrEmpty(firstname)
                && !string.IsNullOrEmpty(dob))
            {
                NUnit.Framework.TestContext.WriteLine($"Member eligibility data found.");
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
                return TestWebDriver.CreateSmartElement(By.XPath("//button[contains(@class,'hamburger is-closed')]")).Element;
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
                return TestWebDriver.CreateSmartElement(By.XPath($"//a[@title='Self Service']")).Element;

            }
        }
    }
}