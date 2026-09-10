using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Edge;
using Selenium.BaseComponents;

namespace Selenium.BaseComponents.Services
{
    /// <summary>
    /// Service for WebDriver initialization and management
    /// Separates WebDriver concerns from BaseFeatureFixture
    /// </summary>
    public class WebDriverService
    {
        // Own SettingsReader instance (same pattern as APIGatway's own) - reads
        // AppSettings:HeadlessMode from the deployed appSettings.json, i.e. a
        // per-deployment Base Framework setting, not something picked per run/queue
        // item (unlike Browser/LoginUserId, which are threaded through TestContext.
        // Parameters instead). Read once and cached - matches the file's own
        // per-process lifetime (a fresh isolated process is started for every run
        // anyway, see AGENTS.md's ProcessModel=Separate notes, so there's no
        // "picked up a stale value mid-run" concern).
        private readonly bool _headlessFromSettings;

        public WebDriverService()
        {
            var settingsReader = new SettingsReader();
            _headlessFromSettings = bool.TryParse(
                settingsReader.GetSetting("AppSettings:HeadlessMode"), out var configured) && configured;
        }

        /// <summary>
        /// Creates and configures Chrome WebDriver
        /// </summary>
        public IWebDriver CreateChromeDriver(bool headless = false)
        {
            ChromeOptions chromeOptions = new ChromeOptions();
            chromeOptions.AddArgument("--disable-notifications");
            chromeOptions.AddArgument("--start-maximized");
            chromeOptions.AddArgument("--disable-extensions");
            chromeOptions.AddArgument("--no-sandbox");
            chromeOptions.AddArgument("--ignore-certificate-errors");
            chromeOptions.AddArgument("--disable-search-engine-choice-screen");

            // Headless if: the caller explicitly asked for it (this parameter was
            // previously accepted but never actually read - a pre-existing dead-
            // parameter bug, fixed here), OR AppSettings:HeadlessMode is true in the
            // deployed appSettings.json (the new Base Framework "Silent/Headless
            // Browser Mode" setting), OR the existing CI-environment-variable check.
            if (headless || _headlessFromSettings || Environment.GetEnvironmentVariable("AGENT_MACHINENAME") != null)
            {
                chromeOptions.AddArgument("--headless");

                // --start-maximized above is a no-op in headless mode (confirmed by
                // direct testing: without this, real pages rendered at a small default
                // viewport, causing intermittent ElementClickInterceptedException/
                // StaleElementReferenceException failures that don't happen with a
                // real visible window) - there's no real window to maximize, so an
                // explicit size is required for headless to behave equivalently.
                chromeOptions.AddArgument("--window-size=1920,1080");
            }

            var service = ChromeDriverService.CreateDefaultService();
            var chromeDriver = new ChromeDriver(service, chromeOptions, TimeSpan.FromMinutes(5));

            // Fix for "thenCore is not a function" error caused by html2pdf.js/jsPDF
            chromeDriver.ExecuteCdpCommand("Page.addScriptToEvaluateOnNewDocument",
                new Dictionary<string, object>
                {
                    { "source", @"
                        window.cdc_adoQpoasnfa76pfcZLmcfl_Promise = window.Promise;
                        window.cdc_adoQpoasnfa76pfcZLmcfl_JSON = window.JSON;
                        if (!Promise.prototype.thenCore) {
                            Promise.prototype.thenCore = Promise.prototype.then;
                        }
                    " }
                });

            return chromeDriver;
        }

        /// <summary>
        /// Creates and configures Edge WebDriver
        /// </summary>
        public IWebDriver CreateEdgeDriver(bool headless = false)
        {
            EdgeOptions edgeOptions = new EdgeOptions();
            edgeOptions.AddArgument("--disable-notifications");
            edgeOptions.AddArgument("--start-maximized");
            edgeOptions.AddArgument("--disable-extensions");
            edgeOptions.AddArgument("--no-sandbox");
            edgeOptions.AddArgument("--ignore-certificate-errors");
            edgeOptions.AddArgument("--disable-search-engine-choice-screen");

            // Same three-way check (and --window-size reasoning) as CreateChromeDriver
            // above.
            if (headless || _headlessFromSettings || Environment.GetEnvironmentVariable("AGENT_MACHINENAME") != null)
            {
                edgeOptions.AddArgument("--headless");
                edgeOptions.AddArgument("--window-size=1920,1080");
            }

            var service = EdgeDriverService.CreateDefaultService();
            return new EdgeDriver(service, edgeOptions, TimeSpan.FromMinutes(5));
        }

        /// <summary>
        /// Safely disposes of WebDriver
        /// </summary>
        public void DisposeWebDriver(IWebDriver webDriver)
        {
            if (webDriver != null)
            {
                try
                {
                    webDriver.Close();
                    webDriver.Quit();
                    webDriver.Dispose();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error disposing WebDriver: {ex.Message}");
                }
            }
        }
    }
}
