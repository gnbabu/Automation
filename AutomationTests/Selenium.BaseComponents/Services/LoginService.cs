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
        /// Was a hard-coded per-environment URL switch (confirmed at least one entry -
        /// E2EP3 - was outright wrong, pointing at the E2E domain instead). Removed per
        /// "getting rid of hardcoded test project values"
        /// BaseFeatureFixture.Url always prefers its own API-resolved EnvironmentUrl
        /// (aut.Environment.EnvironmentUrl, configured via Environment Management) and
        /// only falls back to calling this when that's unavailable - so reaching this
        /// method at all means no EnvironmentUrl is configured for the target
        /// environment. Fails clearly instead of silently guessing.
        /// </summary>
        public string GetLoginUrl()
        {
            throw new InvalidOperationException(
                $"No EnvironmentUrl is configured for environment '{_environment}'. " +
                "Set it via Environment Management, or run this test via the Portal " +
                "against an environment that has one configured.");
        }

        /// <summary>
        /// Performs login with URL, username, and password
        /// </summary>
        public void Login(string loginUrl, string userName, string password)
        {
            try
            {
                for (var attempt = 1; ; attempt++)
                {
                    try
                    {
                        PerformLoginSteps(loginUrl, userName, password);
                        break;
                    }
                    catch (Exception ex) when (attempt == 1 && ex is StaleElementReferenceException or WebDriverTimeoutException or NoSuchElementException)
                    {
                        Console.WriteLine($"Login attempt {attempt} failed ({ex.GetType().Name}: {ex.Message}). Retrying once.");
                    }
                }

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

        private void PerformLoginSteps(string loginUrl, string userName, string password)
        {
            var userNameBox = By.Id("ctl00_MainContent_Login1_UserName");
            var nextButton = By.Id("ctl00_MainContent_Login1_btnNext");
            var passwordBox = By.Id("ctl00_MainContent_Login1_Password");
            var loginButton = By.Id("ctl00_MainContent_Login1_LoginButton");
            var terms = By.Id("ctl00_MainContent_chkTerms");
            var timeout = TimeSpan.FromSeconds(60);

            _webDriver.Navigate().GoToUrl(loginUrl);
            _webDriver.WaitUntilDocumentIsReady(TimeSpan.FromSeconds(5));

            WaitForLoginStep(userNameBox, timeout, "The user name box didn't appear on the login page.");
            _webDriver.CreateSmartElement(userNameBox).Set(userName);
            _webDriver.CreateSmartElement(nextButton).Click();

            _webDriver.WaitForPostback(timeout);
            WaitForLoginStep(passwordBox, timeout, $"The password box didn't appear after entering user name '{userName}' and clicking Next.");
            _webDriver.CreateSmartElement(passwordBox).Set(password);
            _webDriver.CreateSmartElement(loginButton).Click();

            _webDriver.WaitForPostback(timeout);
            WaitForLoginStep(terms, timeout, $"The terms checkbox didn't appear after logging in as '{userName}'. The password stored for this login user may be wrong or expired.");
            _webDriver.CreateSmartElement(terms).Click();
            _webDriver.WaitForPostback(timeout);
        }

        private void WaitForLoginStep(By locator, TimeSpan timeout, string failureMessage)
        {
            try
            {
                _webDriver.WaitUntilElementIsVisibleAndReturn(locator, timeout);
            }
            catch (WebDriverTimeoutException ex)
            {
                throw new WebDriverTimeoutException($"{failureMessage} (waited {timeout.TotalSeconds:0} s)", ex);
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
        /// Navigates to the environment's URL without attempting any login form
        /// interaction - used for environments that don't need a manual login step (no
        /// authentication at all, or SSO - a real user still browses to the app's URL in
        /// both cases, they just never see/fill in this app's own login form). Unlike
        /// Login(...), this has no app-specific assumptions (no OHPNM login field IDs),
        /// so it's safe to call against any URL.
        /// </summary>
        public void NavigateOnly(string url)
        {
            try
            {
                _webDriver.Navigate().GoToUrl(url);
                _webDriver.WaitUntilDocumentIsReady(TimeSpan.FromSeconds(5));
            }
            catch (Exception ex)
            {
                throw new Exception($"Navigation failed: {ex.Message}", ex);
            }
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
