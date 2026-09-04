using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Edge;

namespace Selenium.BaseComponents.Services
{
    /// <summary>
    /// Service for WebDriver initialization and management
    /// Separates WebDriver concerns from BaseFeatureFixture
    /// </summary>
    public class WebDriverService
    {
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

            // Check if running in CI environment
            if (Environment.GetEnvironmentVariable("AGENT_MACHINENAME") != null)
            {
                chromeOptions.AddArgument("--headless");
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

            // Check if running in CI environment
            if (Environment.GetEnvironmentVariable("AGENT_MACHINENAME") != null)
            {
                edgeOptions.AddArgument("--headless");
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
