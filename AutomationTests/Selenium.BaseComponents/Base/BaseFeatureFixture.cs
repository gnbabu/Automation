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
                return LoginService.GetLoginUrl();
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
            if (profile != null)
            {
                Username = UserCredentials.UserNameGenerator.GetUserName(profile, environment);
                pswd = UserCredentials.PasswordGenerator.GetPassword(environment);
            }
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
        public virtual void InitializeTestSuite()
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
            InitializeChromeAndLogin();
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
