using OpenQA.Selenium;
using Selenium.BaseComponents.Data;
using Selenium.BaseComponents.Utilities;
using SeleniumExtensions.Extensions;

namespace Selenium.BaseComponents.Services
{
    /// <summary>
    /// Service for login functionality
    /// Separates login concerns from BaseFeatureFixture
    /// </summary>
    public class LoginService
    {
        private IWebDriver _webDriver;
        private readonly string _environment;

        public LoginService(IWebDriver webDriver, string environment = null)
        {
            _webDriver = webDriver;
            _environment = environment ?? Users.CurrentEnvironment;
        }

        public void SetWebDriver(IWebDriver webDriver)
        {
            _webDriver = webDriver;
        }

        /// <summary>
        /// Gets the login URL based on environment
        /// </summary>
        public string GetLoginUrl()
        {
            switch (_environment)
            {
                case Users.Environment.INT01P3:
                    return "https://ohpnm-dev.omes.maximus.com/OH_PNM_INT01P3/Account/Login.aspx";
                case Users.Environment.INT01:
                    return "https://ohpnm-dev.omes.maximus.com/OH_PNM_INT01/Account/Login.aspx";
                case Users.Environment.DEV01:
                    return "https://ohpnm-dev.omes.maximus.com/OH_PNM_DEV/Account/Login.aspx";
                case Users.Environment.DEV01P3:
                    return "https://ohpnm-dev.omes.maximus.com/OH_PNM_DEVP3/Account/Login.aspx";
                case Users.Environment.E2E:
                    return "https://ohpnm-e2e.omes.maximus.com/OH_PNM_E2E/Account/Login.aspx";
                case Users.Environment.E2EP3:
                    return "https://ohpnm-e2e.omes.maximus.com/OH_PNM_E2E/Account/Login.aspx";
                case Users.Environment.PROD:
                    return "https://ohpnm.omes.maximus.com/OH_PNM_PROD/Account/Login.aspx";
                default:
                    return "https://ohpnm-dev.omes.maximus.com/OH_PNM_DEV/Account/Login.aspx";
            }
        }

        /// <summary>
        /// Performs login with URL, username, and password
        /// </summary>
        public void Login(string loginUrl, string userName, string password)
        {
            try
            {
                _webDriver.Navigate().GoToUrl(loginUrl);
                _webDriver.WaitUntilDocumentIsReady(TimeSpan.FromSeconds(5));

                // Step 1: Enter username and click Next
                var userNameElement = _webDriver.CreateSmartElement(By.XPath("//input[@id='ctl00_MainContent_Login1_UserName']")).Element;
                var nextButton = _webDriver.CreateSmartElement(By.XPath("//input[@id='ctl00_MainContent_Login1_btnNext']")).Element;

                userNameElement.Set(userName);
                nextButton.Click();

                // Step 2: Enter password and click Login (elements appear after Next click)
                var passwordElement = _webDriver.CreateSmartElement(By.XPath("//input[@id='ctl00_MainContent_Login1_Password']")).Element;
                var loginButton = _webDriver.CreateSmartElement(By.XPath("//input[@id='ctl00_MainContent_Login1_LoginButton']")).Element;

                passwordElement.Set(password);
                loginButton.Click();

                // Step 3: Accept terms (checkbox appears after login)
                var chkTerms = _webDriver.CreateSmartElement(By.XPath("//input[@id='ctl00_MainContent_chkTerms']")).Element;
                chkTerms.Click();

                if (_webDriver.Url.Contains("EmailVerification"))
                {
                    throw new Exception("Login requires two-factor authentication. " +
                        $"User Name: {userName}, Password: {password}. Please try running the test again.");
                }

                if (IsActive())
                {
                    ClickCancelButton();
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Login failed: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Performs login with username and password (uses default URL)
        /// </summary>
        public void Login(string userName, string password)
        {
            Login(GetLoginUrl(), userName, password);
        }

        /// <summary>
        /// Checks if user is on change password page
        /// </summary>
        public bool IsActive()
        {
            return _webDriver.Url.Contains("ChangePassword");
        }

        /// <summary>
        /// Clicks cancel button on change password page
        /// </summary>
        private void ClickCancelButton()
        {
            try
            {
                var cancelButton = _webDriver.CreateSmartElement(By.Id("cancel-button")).Element;
                cancelButton.Click();
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error clicking cancel button: {e.Message}");
            }
        }

        /// <summary>
        /// Logs out of the application
        /// </summary>
        public void Logout()
        {
            var logoutButton = _webDriver.CreateSmartElement(By.Id("ctl00_LoginView2_lnkLogout")).Element;
            logoutButton.Click();
        }
    }
}
