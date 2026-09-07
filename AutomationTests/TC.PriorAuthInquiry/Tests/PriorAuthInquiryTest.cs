using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using SdetToolbox.Pages;
using Selenium.BaseComponents;
using Selenium.BaseComponents.Pages;
using SeleniumExtensions.Configurations;
using SeleniumExtensions.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using static System.Net.Mime.MediaTypeNames;
// Aliased, not a blanket `using Selenium.BaseComponents.Utilities;` - see
// TC.SearchRA/Tests/SearchRATest.cs for why (avoids a TestWebDriver.FindElement(...)
// extension-method ambiguity with SdetToolbox.Pages.PageHelper).
using TestCaseExecutionLog = Selenium.BaseComponents.Utilities.TestCaseExecutionLog;
using TestCaseLogLevel = Selenium.BaseComponents.Utilities.TestCaseLogLevel;
using ExecutionStatus = Selenium.BaseComponents.Utilities.ExecutionStatus;
using Screeshots = Selenium.BaseComponents.Utilities.Screeshots;
using TestScreenshot = Selenium.BaseComponents.Utilities.TestScreenshot;
using Common = Selenium.BaseComponents.Utilities.Common;

namespace TC.PriorAuthInquiry.Tests
{
    [TestFixture("TechAdmin")]
    public class PriorAuthInquiryTest : BaseFeatureFixture
    {
        private Screeshots? _screenshots;

        public PriorAuthInquiryTest(string profile) : base(profile)
        {

        }

        // No real test steps exist yet in this project (confirmed: DataRepository's own
        // GetAutomationData is unused/broken - checks "SearchPA", not this project's
        // flow - and this method's body was previously completely empty) - so there's
        // nothing meaningful to log step-by-step around. Wired up start/complete
        // logging + one screenshot anyway so the mechanism is proven and ready the
        // moment real test steps get added here, matching TC.PriorAuthSearch/
        // TC.SearchRA/TC.SubmitClaims.
        [Test]
        [Author("Vishnuvardhan Reddy")]
        [Category("IntegrationTests")]
        [Property("Description", "Prior Auth Inquiry - sample/placeholder")]
        [Property("Priority", "Low")]
        [Property("TestCaseId", "TCPriorAuthInquiry")]
        [CancelAfter(600000)]
        public void PriorAuthInquiry()
        {
            TestCaseExecutionLog testCaseExecutionLog = new TestCaseExecutionLog
            {
                AssignmentId = AssignmentId,
                AssignmentTestCaseId = AssignmentTestCaseId,
                LogLevel = TestCaseLogLevel.Info,
                ExecutionStatus = ExecutionStatus.Running,
                TestCaseId = "TCPriorAuthInquiry",
                TestCaseDescription = "Prior Auth Inquiry - sample/placeholder",
                LogMessage = "Login to PNM succesfully...!",
                StepName = "Login to PNM"
            };

            _screenshots = new Screeshots
            {
                AssignmentTestCaseId = AssignmentTestCaseId,
                screenShot = new List<byte[]>()
            };

            SaveLog(testCaseExecutionLog);
            _screenshots.screenShot.Add(Common.PrintScreenShot(TestWebDriver, "LoginCompletedSuccessfully"));

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
    }
}
