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

            var queueId = TestContext.Parameters["queueId"];

            if (queueId != null)
            {
                bool status = APIGateway.UpdateQueue(queueId, "InProgress").Result;
            }

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

            
            string className = TestContext.CurrentContext.Test.ClassName.Split('.').ToList().LastOrDefault();

            var onCIEnv = Environment.GetEnvironmentVariable(WebOptions.AGENT_MACHINENAME) != null;
            var fixturePassed = TestContext.CurrentContext.Result.Outcome.Status == TestStatus.Passed;

            NUnit.Framework.Internal.TestResult result = NUnit.Framework.Internal.TestExecutionContext.CurrentContext.CurrentResult;

            IEnumerable<ITestResult> resultList = result.Children.ToList();

            List<TestResults> results = new List<TestResults>();

            //foreach (ITestResult testResult in resultList)
            //{
            //    TestResults testResults = new TestResults();
            //    testResults.Name = testResult.Name;
            //    testResults.ResultStatus = testResult.ResultState.Status.ToString();
            //    testResults.Message = testResult.Message;
            //    testResults.Duration = testResult.Duration.ToString();
            //    testResults.StartTime = testResult.StartTime;
            //    testResults.EndTime = testResult.EndTime;
            //    testResults.ClassName = className;
           
            //    results.Add(testResults);
            //}
 
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
