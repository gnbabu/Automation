using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using OpenQA.Selenium;
using Selenium.BaseComponents.CommonPages;
using Selenium.BaseComponents.CommonPages.Login;
using Selenium.BaseComponents.Configuration;
using Selenium.BaseComponents.Data;
//using OpenQA.Selenium.DevTools.V117.Page;
using Selenium.BaseComponents.Services;
using Selenium.BaseComponents.Utilities;
using SeleniumExtensions.Extensions;


namespace Selenium.BaseComponents.Pages
{
    //[Parallelizable]
    public abstract class BaseFeatureFixture : ILoginPage
    {


        public IWebDriver TestWebDriver;
        private string Username;
        private string pswd;
        private string environment = Users.CurrentEnvironment;

        // Stashed from the constructor, resolved in OneTimeSetUp instead (see
        // InitializeTestSuite) - TestContext.Parameters (EnvironmentId/LoginUserId,
        // needed for the API-backed resolution below) isn't reliably available yet
        // during construction, same reasoning already established for AssignmentId/
        // AssignmentTestCaseId.
        private string? _profile;

        // Set once resolved via the API in OneTimeSetUp; Url below prefers this over
        // LoginService's hard-coded GetLoginUrl() switch when present.
        private string? _resolvedLoginUrl;

        // Service provider for dependency injection
        protected IServiceProvider ServiceProvider;

        // Service layer instances for better separation of concerns
        protected WebDriverService WebDriverService;
        protected LoginService LoginService;
        protected APIGatway APIGateway;

        // Page object for login page
        protected LoginPage LoginPage;

        // Populated automatically from TestParameters (threaded through by TestQueueWorker/
        // NUnitEngineTestRunner, same mechanism as Browser/AccessToken) so any subclass can
        // use them for step-level log/screenshot uploads (e.g. TC.PriorAuthSearch's
        // SaveTestCaseLog/SaveMethodScreenShots calls) without needing its own wiring -
        // moved up here from SearchPATest, which used to declare these locally with nothing
        // ever populating them (always defaulted to 0). Stay 0 when running outside the
        // queue pipeline (e.g. locally via Test Explorer) - same graceful degradation
        // AccessToken already has.
        public int AssignmentId { get; set; }
        public int AssignmentTestCaseId { get; set; }


        //  public TestContext TestContext { get; set; }

        public string Url
        {
            get
            {
                return _resolvedLoginUrl ?? LoginService.GetLoginUrl();
            }
        }

        public BaseFeatureFixture(string username = null, string password = null)
        {
            Username = username;
            pswd = password;
            InitializeServices();
        }

        public BaseFeatureFixture(string profile = null)
        {
            // Deliberately NOT resolving Username/pswd here anymore - see
            // ResolveCredentialsAndUrl, called from OneTimeSetUp once TestContext.
            // Parameters is actually available. profile is still exactly what
            // [TestFixture("...")] declares (e.g. "TechAdmin") - stashed for the
            // hard-coded fallback path, and also used as the human-readable Role label
            // when looking up an API-resolved login user.
            _profile = profile;
            InitializeServices();
        }

        private void InitializeServices()
        {
            // Use Dependency Injection for service creation
            ServiceProvider = ServiceConfig.CreateServiceProvider();
            WebDriverService = ServiceProvider.GetRequiredService<WebDriverService>();
            APIGateway = ServiceProvider.GetRequiredService<APIGatway>();
            // LoginService will be initialized after WebDriver is created
        }


        [SetUp]
        public void BeforeEachTest()
        {

        }

        [OneTimeSetUp]
        public virtual async Task InitializeTestSuite()
        {
            // Read here, not in the constructor - confirmed by direct testing that
            // TestContext.Parameters isn't reliably populated yet during fixture
            // construction (NUnit sets up the current test context around actual
            // execution, not object creation/discovery), even though the same
            // TestParametersDictionary package setting is what ultimately supplies it.
            // Matches where every other TestContext.Parameters read in this codebase
            // already happens (Browser/AccessToken/the old queueId), never a constructor.
            if (int.TryParse(TestContext.Parameters["AssignmentId"], out var assignmentId))
                AssignmentId = assignmentId;
            if (int.TryParse(TestContext.Parameters["AssignmentTestCaseId"], out var assignmentTestCaseId))
                AssignmentTestCaseId = assignmentTestCaseId;

            // The queue item is now marked "InProgress" server-side by TestQueueWorker,
            // immediately before this isolated process is even started - more reliable
            // than a push from in here (which would never fire if this process failed to
            // launch at all) and needs no API call/auth from the test's side. See
            // AGENTS.md "Phase 3" for the reasoning; this used to push queueId/"InProgress"
            // via APIGatway.UpdateQueue, now retired.
            await ResolveCredentialsAndUrlAsync();

            InitializeBrowserAndLogin();
        }

        // Selects Chrome vs Edge based on the already-threaded TestContext.Parameters
        // ["Browser"] (queue -> TestQueueWorker -> NUnitEngineTestRunner -> NUnit
        // TestParameters, same mechanism as AssignmentId/AssignmentTestCaseId above -
        // that whole pipeline already worked correctly; this method was the actual gap
        // - InitializeTestSuite used to call InitializeChromeAndLogin() unconditionally,
        // so InitializeEdgeAndLogin() below existed but was dead code, never reachable,
        // regardless of what was ever selected anywhere upstream). Defaults to Chrome
        // when absent/anything other than "Edge" - matches today's exact behavior for a
        // local Test Explorer run outside the queue pipeline (no Browser parameter at
        // all) or an explicit "Chrome" selection.
        private void InitializeBrowserAndLogin()
        {
            string? browser = TestContext.Parameters["Browser"];
            if (string.Equals(browser, "Edge", StringComparison.OrdinalIgnoreCase))
            {
                InitializeEdgeAndLogin();
            }
            else
            {
                InitializeChromeAndLogin();
            }
        }

        // Resolution order (see AGENTS.md - "per-environment login users, selected
        // explicitly at Run Now/Schedule time"):
        //   1. Environment + LoginUserId available (queue-driven run): call the API.
        //      - RequiresAuthentication == false: skip login entirely (Username stays
        //        null, matching InitializeChromeAndLogin's existing guard) - no behavior
        //        change from today for any environment that doesn't need auth.
        //      - RequiresAuthentication == true and a LoginUserId was supplied (the
        //        person running/scheduling it explicitly picked one): use its
        //        username/password + the resolved EnvironmentUrl.
        //   2. If a profile was declared (e.g. [TestFixture("TechAdmin")]) but nothing
        //      above resolved (no EnvironmentId/LoginUserId supplied, a local Test
        //      Explorer run outside the queue pipeline, an environment with no
        //      EnvironmentUrl/LoginUser data configured yet, API unreachable, etc.):
        //      fail clearly instead of silently logging in with hard-coded credentials
        //      (removed - see AGENTS.md "getting rid of hardcoded test project values").
        //      No profile at all means this fixture was never meant to log in (matches
        //      today's existing "Username stays null" no-op behavior for that case).
        private async Task ResolveCredentialsAndUrlAsync()
        {
            var environmentDetails = await APIGateway.GetEnvironmentDetails();

            if (environmentDetails != null)
            {
                if (!string.IsNullOrWhiteSpace(environmentDetails.EnvironmentUrl))
                    _resolvedLoginUrl = environmentDetails.EnvironmentUrl;

                if (!environmentDetails.RequiresAuthentication)
                {
                    // Environment explicitly doesn't need a login step - leave
                    // Username/pswd null so InitializeChromeAndLogin's existing
                    // `if (Username != null)` guard skips login entirely.
                    Username = null;
                    pswd = null;
                    return;
                }

                var credentials = await APIGateway.GetLoginUserCredentials();
                if (credentials != null && !string.IsNullOrWhiteSpace(credentials.UserName))
                {
                    Username = credentials.UserName;
                    pswd = credentials.Password;
                    return;
                }
            }

            if (_profile != null)
            {
                throw new InvalidOperationException(
                    $"Could not resolve login credentials for profile '{_profile}'. " +
                    "Run this test via the Portal with a Login User selected for the target " +
                    "environment (Environment Management > Login Users), or configure " +
                    "EnvironmentUrl/RequiresAuthentication for this environment.");
            }
        }

        private void InitializeChromeAndLogin()
        {
            // Use WebDriverService for initialization
            TestWebDriver = WebDriverService.CreateChromeDriver();
            
            // Create LoginService with WebDriver using DI
            LoginService = new LoginService(TestWebDriver, environment);
            
            // Create LoginPage for element access
            LoginPage = new LoginPage(TestWebDriver);

            if (Username != null)
            {
                LoginService.Login(Url, Username, pswd);
            }
        }

        private void InitializeEdgeAndLogin()
        {
            // Use WebDriverService for initialization
            TestWebDriver = WebDriverService.CreateEdgeDriver();
            
            // Create LoginService with WebDriver using DI
            LoginService = new LoginService(TestWebDriver, environment);
            
            // Create LoginPage for element access
            LoginPage = new LoginPage(TestWebDriver);

            if (Username != null)
            {
                LoginService.Login(Url, Username, pswd);
            }
        }

      

        [TearDown]
        public void LogFailureAfterEachTest()
        {
            // Catches a failure inside a [Test] method itself. Runs independently of
            // (and in addition to) any subclass's own [TearDown] (e.g. SearchPATest.
            // AfterTest() for success-path screenshots) - NUnit runs every [TearDown]
            // found across the inheritance chain.
            LogFailureIfAny("Test Execution");
        }

        [OneTimeTearDown]
        public void TearDownTestSuite()
        {
            // Catches a failure inside [OneTimeSetUp] itself (e.g. login/credential/URL
            // resolution - the most common real failure category in this project's
            // history) - a [Test]-level [TearDown] alone would never see these, since
            // NUnit doesn't run per-test TearDown when OneTimeSetUp fails (no test ever
            // starts). Called before disposing TestWebDriver below so an opportunistic
            // screenshot still has a live browser to capture, if one was ever launched.
            LogFailureIfAny("OneTimeSetUp");

            if (TestWebDriver != null)
            {
                // Use WebDriverService for proper disposal
                WebDriverService.DisposeWebDriver(TestWebDriver);
                TestWebDriver = null;
            }
        }

        // Posts a Fail-level TestCaseExecutionLog (plus an opportunistic screenshot)
        // whenever the current test/fixture actually failed - no existing test class
        // does this today (confirmed by reading every real Tests/*.cs - failures are
        // only ever caught for a defensive retry or a rethrow, never logged), so the
        // step-log trail (GET api/TestCaseExecutionLogs) previously just stopped
        // abruptly at the last successful step with no indication of what/why. Doesn't
        // touch AssignedTestCases.ErrorMessage/overall status at all - that's a
        // separate, already-working mechanism (NUnitEngineTestRunner's result-XML
        // parsing) - this is purely an additional, more detailed log entry.
        private void LogFailureIfAny(string stepName)
        {
            // Matches the existing guard already used by every test class's own
            // [TearDown] (e.g. SearchPATest.AfterTest()) - skip entirely when not
            // running through the queue pipeline (e.g. locally via Test Explorer).
            if (AssignmentTestCaseId == 0)
                return;

            try
            {
                var result = TestContext.CurrentContext.Result;
                if (result.Outcome.Status != TestStatus.Failed)
                    return;

                var test = TestContext.CurrentContext.Test;
                string testCaseId = test.Properties.Get("TestCaseId")?.ToString() ?? string.Empty;
                string description = test.Properties.Get("Description")?.ToString() ?? string.Empty;

                // Captured/uploaded first (not after the log) so the log entry below can
                // link to it via ScreenshotId - lets the Portal show exactly what the
                // browser looked like for this specific failure, instead of an unlinked
                // pile of screenshots per test case. Also records the current URL - the
                // single most useful piece of context for a navigation-related failure
                // (e.g. the VPN/wrong-environment-URL issues already seen repeatedly in
                // this project), essentially free to capture.
                int? screenshotId = null;
                string? currentUrl = null;
                if (TestWebDriver != null)
                {
                    try { currentUrl = TestWebDriver.Url; } catch { /* driver may already be in a bad state */ }

                    var screenshotBytes = Common.PrintScreenShot(TestWebDriver, $"Failure_{stepName}");
                    if (screenshotBytes != null && screenshotBytes.Length > 0)
                    {
                        screenshotId = APIGateway.SaveMethodScreenShot(new TestScreenshot
                        {
                            AssignmentTestCaseId = AssignmentTestCaseId,
                            Caption = $"Failure_{stepName}",
                            Screenshot = $"data:image/png;base64,{Convert.ToBase64String(screenshotBytes)}",
                            TakenAt = DateTime.Now,
                        });
                    }
                }

                string logMessage = result.Message ?? "Test failed with no message.";
                if (!string.IsNullOrWhiteSpace(currentUrl))
                    logMessage = $"{logMessage}\n\nURL at failure: {currentUrl}";

                var failureLog = new TestCaseExecutionLog
                {
                    AssignmentId = AssignmentId,
                    AssignmentTestCaseId = AssignmentTestCaseId,
                    TestCaseId = testCaseId,
                    TestCaseDescription = description,
                    StepName = stepName,
                    LogMessage = logMessage,
                    LogLevel = TestCaseLogLevel.Fail,
                    ExecutionStatus = ExecutionStatus.Failed,
                    ErrorStackTrace = result.StackTrace,
                    ScreenshotId = screenshotId,
                };

                APIGateway.SaveTestCaseLog(failureLog);
            }
            catch (Exception ex)
            {
                // A failure in this logging/screenshot mechanism itself must never mask
                // the original test failure or throw out of TearDown/OneTimeTearDown -
                // same defensive reasoning as APIGatway.SaveTestCaseLog's own catch.
                TestContext.WriteLine($"LogFailureIfAny failed: {ex.Message}");
            }
        }



        // Used for a mid-test role switch (e.g. TC.Registration logging in as a
        // different role partway through a test) - a genuinely different, unattended
        // use case from the initial OneTimeSetUp login (an explicitly-picked
        // LoginUserId, resolved once up front). Resolves via the API first
        // (Environment + role, see APIGatway.GetLoginUserCredentialsByRole); no more
        // hard-coded UserCredentials fallback - fails clearly instead, since silently
        // logging in with the wrong (or no) credentials for a role switch is worse than
        // a clear error.
        public void LoginByProfile(string profile)
        {
            var credentials = APIGateway.GetLoginUserCredentialsByRole(profile).GetAwaiter().GetResult();
            if (credentials == null || string.IsNullOrWhiteSpace(credentials.UserName))
            {
                throw new InvalidOperationException(
                    $"No login user is configured for role '{profile}' in this environment. " +
                    "Add one via the Environment Management > Login Users screen.");
            }

            // 3-arg overload with this fixture's own resolved Url (API-backed, falls
            // back to LoginService.GetLoginUrl() only if nothing else resolved it) -
            // deliberately not the 2-arg Login(userName, password), which always calls
            // LoginService.GetLoginUrl() directly and has no way to see this fixture's
            // own _resolvedLoginUrl.
            LoginService.Login(Url, credentials.UserName, credentials.Password);
        }

        public bool IsActive()
        {
            return LoginService.IsActive();
        }

        #region Cancel Change Password
        public void ClickCancelButton()
        {
            // Handled by LoginPage
            if (LoginPage != null)
            {
                LoginPage.ClickCancelButton();
            }
        }
        #endregion

        public void Login(string loginUrl, string userName, string password)
        {
            try
            {
                LoginService.Login(loginUrl, userName, password);
            }
            catch (Exception ex)
            {
                if (TestWebDriver != null)
                {
                    WebDriverService.DisposeWebDriver(TestWebDriver);
                    TestWebDriver = null;
                }
                InitializeChromeAndLogin();
            }
        }

        public void Login(string userName, string password)
        {
            LoginService.Login(userName, password);
        }

        public void LoginEmc(string userName, string password)
        {
            //TestWebDriver.Navigate().GoToUrl(Url);

            if (LoginPage != null)
            {
                LoginPage.UserName.Set(userName);
                LoginPage.Password.Set(password);
                LoginPage.LoginButton.Click();
            }
        }

        public void LogOut()
        {
            LoginService.Logout();
        }
    }
}
