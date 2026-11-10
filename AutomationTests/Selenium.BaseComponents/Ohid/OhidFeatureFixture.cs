using NUnit.Framework;
using Selenium.BaseComponents.Pages;
using Selenium.BaseComponents.Utilities;

namespace Selenium.BaseComponents.Ohid
{
    public abstract class OhidFeatureFixture : BaseFeatureFixture
    {
        protected OhidFeatureFixture(string profile) : base(profile)
        {
        }

        [OneTimeSetUp]
        public override async Task InitializeTestSuite()
        {
            if (int.TryParse(TestContext.Parameters["AssignmentId"], out var assignmentId))
                AssignmentId = assignmentId;
            if (int.TryParse(TestContext.Parameters["AssignmentTestCaseId"], out var assignmentTestCaseId))
                AssignmentTestCaseId = assignmentTestCaseId;

            var totp = new TotpOtpProvider(OhidSettings.GetTotpKey());

            var environment = await APIGateway.GetEnvironmentDetails();
            if (string.IsNullOrWhiteSpace(environment?.EnvironmentUrl))
                throw new InvalidOperationException("No environment URL is available. Run this test from the Portal against an environment with an Environment URL.");

            var credentials = await APIGateway.GetLoginUserCredentials();
            if (credentials == null || string.IsNullOrWhiteSpace(credentials.UserName))
                throw new InvalidOperationException("No OH|ID login user was resolved. Pick the environment's OH|ID login user in Run Now / Schedule.");

            TestWebDriver = string.Equals(TestContext.Parameters["Browser"], "Edge", StringComparison.OrdinalIgnoreCase)
                ? WebDriverService.CreateEdgeDriver()
                : WebDriverService.CreateChromeDriver();

            new OhidLoginService(TestWebDriver, LogOhidStep)
                .Login(environment!.EnvironmentUrl!, credentials.UserName!, credentials.Password ?? "", totp, OhidSettings.GetPnmUserId());
        }

        protected void LogOhidStep(string stepName, string message)
        {
            TestContext.Progress.WriteLine($"[OH|ID] {stepName}: {message}");
            if (AssignmentTestCaseId == 0) return;

            APIGateway.SaveTestCaseLog(new TestCaseExecutionLog
            {
                AssignmentId = AssignmentId,
                AssignmentTestCaseId = AssignmentTestCaseId,
                TestCaseId = GetType().Name,
                TestCaseDescription = "OH|ID login",
                StepName = stepName,
                LogMessage = message,
                LogLevel = TestCaseLogLevel.Info,
                ExecutionStatus = ExecutionStatus.Running,
            });
        }
    }
}
