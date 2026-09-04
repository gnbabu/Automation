
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using SdetToolbox.Pages;
using Selenium.BaseComponents;
using Selenium.BaseComponents.Pages;
using Selenium.BaseComponents.Utilities;
using SeleniumExtensions.Configurations;
using SeleniumExtensions.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using TC.PriorAuthSearch.Models;
using TC.PriorAuthSearch.Utilities;
using static System.Net.Mime.MediaTypeNames;
using PageHelper = SdetToolbox.Pages.PageHelper;

namespace TC.PriorAuthSearch.Tests
{
    [TestFixture("TechAdmin")]
    public class SearchPATest : BaseFeatureFixture
    {
        public int AssignmentTestCaseId { get; set; }
        public int AssignmentId { get; set; }

        public string? MethodName { get; set; }

        private Screeshots? _screenshots;

        public SearchPATest(string profile) : base(profile)
        {
            // APIGateway is now injected via BaseFeatureFixture
        }

        [Test]
        [Author("Vishnuvardhan Reddy")]
        [Category("IntegrationTests")]
        [Property("Description", "Navigate to Search PA Page")]
        [Property("Priority", "High")]
        [Property("TestCaseId", "TCSearchPA")]
        public void NavigateToSeachPAPage()
        {
            TestCaseExecutionLog testCaseExecutionLog = new TestCaseExecutionLog();

            testCaseExecutionLog.AssignmentId = AssignmentId;
            testCaseExecutionLog.AssignmentTestCaseId = AssignmentTestCaseId;
            testCaseExecutionLog.LogLevel = TestCaseLogLevel.Info;
            testCaseExecutionLog.ExecutionStatus = ExecutionStatus.Running;
            testCaseExecutionLog.LogMessage = "Login to PNM succesfully...!";
            testCaseExecutionLog.StepName = "Login to PNM";
            testCaseExecutionLog.TestCaseId = "TCSearchPA";
            testCaseExecutionLog.TestCaseDescription = "Navigate to Search PA Page";

            //SaveLog(testCaseExecutionLog);

            //_screenshots = new Screeshots
            //{
            //    AssignmentTestCaseId = AssignmentTestCaseId,
            //    screenShot = new List<byte[]>()
            //};


            //_screenshots.screenShot = new List<byte[]>();
            //_screenshots.screenShot.Add(Common.PrintScreenShot(TestWebDriver, "LoginCompletedSuccessfully"));

            SidebarMenu.Click();
            Thread.Sleep(4000);

            //_screenshots.screenShot.Add(Common.PrintScreenShot(TestWebDriver, "Navingating to SelfService"));

            testCaseExecutionLog.LogMessage = "Navigating to self service...!";
            testCaseExecutionLog.StepName = "Self Service";
            //SaveLog(testCaseExecutionLog);

            NavigateToSelfService();

            TC.PriorAuthSearch.Pages.FinancialProviderInformationPage financialProviderInformationPage = new TC.PriorAuthSearch.Pages.FinancialProviderInformationPage(TestWebDriver);
            financialProviderInformationPage.WaitUntilElementIsVisible();
            financialProviderInformationPage.TxtMedicaid.Set("2422659");

            testCaseExecutionLog.LogMessage = "Entered 2422659 for search";
            testCaseExecutionLog.StepName = "Medicaid Search";
            //SaveLog(testCaseExecutionLog);

            //  _screenshots.screenShot.Add(Common.PrintScreenShot(TestWebDriver, "MedicaidEntered"));

            financialProviderInformationPage.lnkBtnPriorAuth.Click();

            TC.PriorAuthSearch.Pages.SearchPAPage SearchPAPage = new TC.PriorAuthSearch.Pages.SearchPAPage(TestWebDriver);

            SearchPAPage.lnkBtnSearchPA.Click();

            testCaseExecutionLog.LogMessage = "SearchButton Clicked";
            testCaseExecutionLog.StepName = "PA Search";
            // SaveLog(testCaseExecutionLog);

            // _screenshots.screenShot.Add(Common.PrintScreenShot(TestWebDriver, "SearchButton Clicked"));

            SearchPAPage.WaitUntilElementIsVisible();

            // _screenshots.screenShot.Add(Common.PrintScreenShot(TestWebDriver, "Success"));

            testCaseExecutionLog.LogMessage = "Success";
            testCaseExecutionLog.StepName = "Test case executed successfully...!";
            // SaveLog(testCaseExecutionLog);
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
                return TestWebDriver.CreateSmartElement(By.XPath($"//a[normalize-space()='Self Service']")).Element;

            }
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
    }

}
