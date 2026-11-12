using System.Globalization;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using Selenium.BaseComponents.Ohid;
using Selenium.BaseComponents.Services;

namespace TC.OhidEligibilityDemo
{
    [TestFixture]
    [NonParallelizable]
    public class OhidEligibilityDemoTest
    {
        private static readonly TimeSpan PageTimeout = TimeSpan.FromSeconds(60);

        private readonly WebDriverService _webDriverService = new();
        private IWebDriver _driver = null!;
        private string _screenshotFolder = null!;
        private int _screenshotNumber;

        [OneTimeSetUp]
        public void LoginThroughOhid()
        {
            _screenshotFolder = Path.Combine(AppContext.BaseDirectory, "DemoScreenshots", DateTime.Now.ToString("yyyyMMdd_HHmmss"));
            Directory.CreateDirectory(_screenshotFolder);

            _driver = DemoData.Browser == "Edge"
                ? _webDriverService.CreateEdgeDriver()
                : _webDriverService.CreateChromeDriver();

            new OhidLoginService(_driver, (step, message) => Step(step, message, screenshot: false))
                .Login(DemoData.PnmLoginUrl, DemoData.OhidUserName, DemoData.OhidPassword,
                    new TotpOtpProvider(DemoData.TotpKey), DemoData.PnmUserId);

            Step("Signed in", "Signed in to PNM through OH|ID.");
        }

        [Test]
        [Property("TestCaseId", "TC_ELIG_DEMO")]
        [Property("Description", "Demo: OH|ID login with authenticator code, then search member eligibility (hard-coded data)")]
        [CancelAfter(900000)]
        public void OhidLoginAndSearchEligibility()
        {
            Click(By.XPath("//button[contains(@class,'hamburger is-closed')]"));
            Click(By.XPath("//a[normalize-space()='Self Service']"));
            WaitVisible(By.Id("lblTitle"), "the Self Service Information page");
            Step("Self Service", "Opened Self Service.");

            Type(By.Id("ctl00_MainContent_txtMedicaidNumber"), DemoData.ProviderMedicaidId);
            if (!IsVisible(By.Id("ctl00_MainContent_lnkBtnPriorAuth")))
                Assert.Fail($"The Self Service link isn't available for provider {DemoData.ProviderMedicaidId}. " +
                            $"Give OH|ID user {DemoData.OhidUserName} Self Service access for this provider, or change DemoData.ProviderMedicaidId.");
            Click(By.Id("ctl00_MainContent_lnkBtnPriorAuth"));
            Step("Provider", $"Entered provider {DemoData.ProviderMedicaidId}.");

            Click(By.XPath("//img[@alt='Search Eligibility']"));
            WaitVisible(By.Id("txtMBN"), "the Search Eligibility page");
            Step("Search Eligibility", "Opened Search Eligibility.");

            var errorLocator = By.XPath("//span[contains(normalize-space(),'Error Code') or contains(normalize-space(),'Error Description')]");
            var recipientHeader = By.XPath("//h1[normalize-space()='RECIPIENT INFORMATION']");
            var errorBefore = VisibleText(errorLocator);

            Type(By.Id("txtMBN"), DemoData.MedicaidBillingNumber);
            Type(By.Id("txtSSN"), DemoData.Ssn);
            Type(By.Id("txtDOB"), Format(DemoData.DateOfBirth));
            Type(By.Id("txtFDOS"), Format(DemoData.FromDateOfService));
            var toDos = By.XPath("//input[@id='txtTDOS' or contains(@id,'TDOS') or contains(@id,'ToDOS') or contains(@id,'ToDos')]");
            if (IsVisible(toDos))
                Type(toDos, Format(DemoData.ToDateOfService));
            Click(By.Id("btnSearch"));

            Wait(PageTimeout).Until(_ => IsVisible(recipientHeader)
                || (IsVisible(errorLocator) && VisibleText(errorLocator) != errorBefore));

            var error = VisibleText(errorLocator);
            if (!string.IsNullOrEmpty(error) && error != errorBefore)
            {
                if (!error.Contains("No records returned", StringComparison.OrdinalIgnoreCase))
                {
                    Step("Eligibility search failed", error);
                    Assert.Fail($"Eligibility search failed: {error}");
                }

                Step("Eligibility result", "Search completed: no eligibility records returned.");
                return;
            }

            Step("Eligibility result", $"Recipient information shown for {DemoData.MedicaidBillingNumber}.");
        }

        [OneTimeTearDown]
        public void CloseBrowser()
        {
            if (TestContext.CurrentContext.Result.Outcome.Status == NUnit.Framework.Interfaces.TestStatus.Failed && _driver != null)
                SaveScreenshot("Failure");

            TestContext.Progress.WriteLine($"Screenshots: {_screenshotFolder}");
            if (_driver != null)
                _webDriverService.DisposeWebDriver(_driver);
        }

        private void Step(string step, string message, bool screenshot = true)
        {
            TestContext.Progress.WriteLine($"[{DateTime.Now:HH:mm:ss}] {step}: {message}");
            if (screenshot) SaveScreenshot(step);
        }

        private void SaveScreenshot(string name)
        {
            try
            {
                var file = $"{++_screenshotNumber:D2}_{string.Concat(name.Where(char.IsLetterOrDigit))}.png";
                ((ITakesScreenshot)_driver).GetScreenshot().SaveAsFile(Path.Combine(_screenshotFolder, file));
            }
            catch (WebDriverException)
            {
            }
        }

        private static string Format(DateTime value) => value.ToString("MM/dd/yyyy", CultureInfo.InvariantCulture);

        private WebDriverWait Wait(TimeSpan timeout)
        {
            var wait = new WebDriverWait(_driver, timeout) { PollingInterval = TimeSpan.FromMilliseconds(500) };
            wait.IgnoreExceptionTypes(typeof(StaleElementReferenceException), typeof(NoSuchElementException));
            return wait;
        }

        private void WaitVisible(By by, string what)
        {
            try
            {
                Wait(PageTimeout).Until(_ => IsVisible(by));
            }
            catch (WebDriverTimeoutException)
            {
                throw new WebDriverTimeoutException($"Timed out after {PageTimeout.TotalSeconds:0} s waiting for {what}.");
            }
        }

        private bool IsVisible(By by)
        {
            try { return _driver.FindElements(by).Any(e => e.Displayed); }
            catch (WebDriverException) { return false; }
        }

        private string? VisibleText(By by)
        {
            try { return _driver.FindElements(by).FirstOrDefault(e => e.Displayed)?.Text?.Trim(); }
            catch (WebDriverException) { return null; }
        }

        private void Click(By by)
        {
            WaitVisible(by, by.ToString());
            Retry(() => _driver.FindElements(by).First(e => e.Displayed).Click());
            WaitForPage();
        }

        private void Type(By by, string value)
        {
            WaitVisible(by, by.ToString());
            Retry(() =>
            {
                var element = _driver.FindElements(by).First(e => e.Displayed);
                element.Clear();
                if (!string.IsNullOrEmpty(value)) element.SendKeys(value);
                element.SendKeys(Keys.Tab);
            });
            WaitForPage();
        }

        private void WaitForPage()
        {
            try
            {
                Wait(TimeSpan.FromSeconds(30)).Until(d =>
                    Equals(((IJavaScriptExecutor)d).ExecuteScript(
                        "return document.readyState === 'complete' && !(window.Sys && Sys.WebForms && Sys.WebForms.PageRequestManager && Sys.WebForms.PageRequestManager.getInstance().get_isInAsyncPostBack());"), true));
            }
            catch (WebDriverTimeoutException)
            {
            }
        }

        private static void Retry(Action action)
        {
            for (var attempt = 1; ; attempt++)
            {
                try
                {
                    action();
                    return;
                }
                catch (StaleElementReferenceException) when (attempt < 3)
                {
                }
            }
        }
    }
}
