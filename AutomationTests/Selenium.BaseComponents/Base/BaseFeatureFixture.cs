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

            InitializeChromeAndLogin();
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
        //   2. Fallback, on any failure/absence (API unreachable, no EnvironmentId/
        //      LoginUserId supplied, local Test Explorer run outside the queue
        //      pipeline, an environment not yet migrated to EnvironmentUrl/LoginUser
        //      data, etc.): today's exact hard-coded UserCredentials.UserNameGenerator/
        //      PasswordGenerator/LoginService.GetLoginUrl() behavior via _profile -
        //      unchanged from before this feature existed.
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

            // Fallback: today's exact hard-coded behavior, unchanged.
            if (_profile != null)
            {
                Username = UserCredentials.UserNameGenerator.GetUserName(_profile, environment);
                pswd = UserCredentials.PasswordGenerator.GetPassword(environment);
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

      

        [OneTimeTearDown]
        public void TearDownTestSuite()
        {
            if (TestWebDriver != null)
            {
                // Use WebDriverService for proper disposal
                WebDriverService.DisposeWebDriver(TestWebDriver);
                TestWebDriver = null;
            }
        }



        public void LoginByProfile(string profile)
        {
            var uname = UserCredentials.UserNameGenerator.GetUserName(profile, Users.CurrentEnvironment);
            LoginService.Login(uname, UserCredentials.PasswordGenerator.GetPassword(Users.CurrentEnvironment));
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
