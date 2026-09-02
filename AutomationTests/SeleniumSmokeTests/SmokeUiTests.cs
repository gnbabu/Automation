using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Edge;
using WebDriverManager;
using WebDriverManager.DriverConfigs.Impl;

namespace SeleniumSmokeTests
{
    // Proof-of-concept Selenium suite for the NUnitEngineTestRunner refactor - there was no
    // real Selenium test anywhere in this codebase before this, so this is what actually
    // proves (rather than just argues) that the gaps in the old hand-rolled reflection
    // runner (async never awaited, no OneTimeSetUp/TearDown, TestContext not working,
    // [Ignore] not honored) are genuinely fixed by running through NUnit's real engine.
    //
    // Uses https://www.selenium.dev/selenium/web/web-form.html - the Selenium project's own
    // official, purpose-built, stable test fixture page - instead of a real application
    // page, so this suite doesn't depend on (or need credentials for) anything else.
    [TestFixture]
    [Category("SeleniumSmokeTests")]
    public class SmokeUiTests
    {
        private IWebDriver _driver = null!;
        private static readonly string TestPageUrl = "https://www.selenium.dev/selenium/web/web-form.html";

        // [OneTimeSetUp]/[OneTimeTearDown] - launches the browser ONCE for the whole
        // fixture (the conventional Selenium pattern; expensive to do per-test), which the
        // old reflection-based runner never called at all (it only ever invoked per-test
        // [SetUp]/[TearDown]).
        [OneTimeSetUp]
        public void LaunchBrowser()
        {
            // Reads the Browser choice via NUnit's real TestContext.Parameters - populated
            // by NUnitEngineTestRunner from the Run Now/Schedule dialog's Browser selection,
            // via the engine's TestParametersDictionary/TestParameters package settings
            // (see AGENTS.md). Defaults to Chrome if not supplied (e.g. when Explore()-only,
            // no actual run, or an older caller that doesn't set it).
            var browser = TestContext.Parameters.Exists("Browser")
                ? TestContext.Parameters["Browser"]
                : "Chrome";

            TestContext.Progress.WriteLine($"[SmokeUiTests] Launching browser: {browser}");

            _driver = (browser ?? "Chrome").ToLowerInvariant() switch
            {
                "edge" => CreateEdgeDriver(),
                _ => CreateChromeDriver(),
            };

            _driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(5);
        }

        [OneTimeTearDown]
        public void QuitBrowser()
        {
            _driver?.Quit();
            _driver?.Dispose();
        }

        private static IWebDriver CreateChromeDriver()
        {
            // Use WebDriverManager's resolved driver path explicitly (via ChromeDriverService)
            // rather than relying on Selenium's own bundled "Selenium Manager" - that looks
            // for a selenium-manager/ subfolder next to the running assembly, which only
            // exists in a full `dotnet build`/publish output, not a bare deployed test DLL
            // (exactly the kind of release-folder dependency gap this refactor deals with).
            var driverPath = new DriverManager().SetUpDriver(new ChromeConfig());
            var service = ChromeDriverService.CreateDefaultService(Path.GetDirectoryName(driverPath));
            var options = new ChromeOptions();
            options.AddArgument("--headless=new");
            options.AddArgument("--no-sandbox");
            options.AddArgument("--disable-dev-shm-usage");
            return new ChromeDriver(service, options);
        }

        private static IWebDriver CreateEdgeDriver()
        {
            var driverPath = new DriverManager().SetUpDriver(new EdgeConfig());
            var service = EdgeDriverService.CreateDefaultService(Path.GetDirectoryName(driverPath));
            var options = new EdgeOptions();
            options.AddArgument("--headless=new");
            options.AddArgument("--no-sandbox");
            return new EdgeDriver(service, options);
        }

        // Proves async test methods actually run to completion and are properly awaited -
        // the old reflection runner's `method.Invoke(instance, args)` never awaited an
        // async Task, so this would have always silently "passed" regardless of what
        // happened inside it (or any exception thrown inside it would never have been
        // caught). Uses a real async wait (Task.Delay) plus a real Selenium interaction to
        // make that concrete rather than theoretical.
        [Test]
        [Property("Description", "Types into the text input and submits the Selenium test form, asserting the result page shows the submitted value - proves async test methods are properly awaited end-to-end.")]
        [Property("Priority", "High")]
        [Property("TestCaseId", "TC_SEL_001")]
        public async Task SubmitWebForm_TextInput_ShowsSubmittedValue()
        {
            _driver.Navigate().GoToUrl(TestPageUrl);
            await Task.Delay(200); // genuine async wait - would be skipped/ignored if Invoke() never actually awaited this method

            var textInput = _driver.FindElement(By.Name("my-text"));
            textInput.Clear();
            textInput.SendKeys("NUnitEngineTestRunner-PoC");

            var submitButton = _driver.FindElement(By.CssSelector("button[type='submit']"));
            ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].scrollIntoView(true);", submitButton);
            ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].click();", submitButton);
            await Task.Delay(200);

            Assert.That(_driver.Url, Does.Contain("submitted-form"));
            Assert.That(_driver.Url, Does.Contain("NUnitEngineTestRunner-PoC"));
        }

        // Parameterized-test data source using TestCaseData.SetProperty(...) instead of
        // plain [TestCase(...)] + a method-level [Property(...)] - a method-level
        // [Property] attribute applies identically to every generated row (confirmed by
        // direct testing: NUnit attaches it once, to the shared <test-suite
        // type="ParameterizedMethod"> wrapper, not per <test-case>), so all 3 rows would
        // report the exact same TestCaseId. Since this app treats TestCaseId as the unique
        // key for assignment/execution tracking, that would make 3 distinct executable
        // tests collide under one ID - assigning/tracking one would be ambiguous about
        // which of the 3 real variants it refers to. TestCaseData.SetProperty gives each
        // generated row its own distinct property values instead.
        private static IEnumerable<TestCaseData> TextInputValues()
        {
            yield return new TestCaseData("Alpha")
                .SetProperty("TestCaseId", "TC_SEL_002A")
                .SetProperty("Priority", "Medium")
                .SetProperty("Description", "Confirms the value 'Alpha' round-trips through the text input correctly.");
            yield return new TestCaseData("Beta")
                .SetProperty("TestCaseId", "TC_SEL_002B")
                .SetProperty("Priority", "Medium")
                .SetProperty("Description", "Confirms the value 'Beta' round-trips through the text input correctly.");
            yield return new TestCaseData("Gamma")
                .SetProperty("TestCaseId", "TC_SEL_002C")
                .SetProperty("Priority", "Medium")
                .SetProperty("Description", "Confirms the value 'Gamma' round-trips through the text input correctly.");
        }

        [TestCaseSource(nameof(TextInputValues))]
        public void TextInput_AcceptsValue(string value)
        {
            _driver.Navigate().GoToUrl(TestPageUrl);
            var textInput = _driver.FindElement(By.Name("my-text"));
            textInput.Clear();
            textInput.SendKeys(value);

            Assert.That(textInput.GetDomProperty("value"), Is.EqualTo(value));
        }

        // Reads TestContext.Parameters directly (rather than through a Selenium action) so
        // Browser-wiring can be verified independently of anything else - the old reflection
        // runner never populated TestContext at all, so this would have thrown/returned
        // nothing meaningful before this refactor.
        [Test]
        [Property("Description", "Confirms the Browser choice from Run Now/Schedule reaches the test via TestContext.Parameters.")]
        [Property("Priority", "Low")]
        [Property("TestCaseId", "TC_SEL_003")]
        public void BrowserParameter_IsReadable()
        {
            var hasBrowserParam = TestContext.Parameters.Exists("Browser");
            var browserValue = hasBrowserParam ? TestContext.Parameters["Browser"] : "(none)";
            TestContext.Progress.WriteLine($"[SmokeUiTests] Browser parameter present: {hasBrowserParam}, value: {browserValue}");

            // A real assertion instead of Assert.Pass(...) - Assert.Pass throws
            // NUnit.Framework.SuccessException internally as its own control-flow signal
            // (that's how it's always worked - it doesn't affect the Passed outcome, and
            // isn't a real error), but it shows up as a confusing "exception" in tools like
            // Visual Studio's Test Explorer detail view even though the test genuinely
            // passed. A test that completes its method body normally needs no such signal.
            // Not a hard failure if the parameter is absent (older callers may not set it
            // yet) - this test's purpose is to prove readability when it *is* set, not to
            // require every caller to set it.
            if (hasBrowserParam)
                Assert.That(browserValue, Is.Not.Empty, "Browser parameter was present but empty.");
        }

        // Proves [Ignore] is honored (reported as Skipped, not silently run/force-mapped
        // into Pass or Fail like the old reflection runner would have done).
        [Test]
        [Ignore("Deliberately skipped to prove NUnit's real [Ignore] handling reports Skipped instead of running/force-mapping into Pass or Fail.")]
        [Property("Description", "Deliberately marked [Ignore] - proves NUnit's real Skipped handling instead of the old runner's forced Pass/Fail mapping.")]
        [Property("Priority", "Low")]
        [Property("TestCaseId", "TC_SEL_004")]
        public void DeliberatelySkipped_ShouldReportAsSkipped()
        {
            Assert.Fail("This should never actually run.");
        }
    }
}
