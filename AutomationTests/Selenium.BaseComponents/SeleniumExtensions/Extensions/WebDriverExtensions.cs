using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using static SeleniumExtras.WaitHelpers.ExpectedConditions;

namespace SeleniumExtensions.Extensions
{
    public static class WebDriverExtensions
    {
        public static IWebElement FindElementWithRetry(this IWebDriver webDriver, By locator, IWait<IWebDriver> elementWait)
        {
            try
            {
                elementWait.Until(ElementExists(locator));
                return webDriver.FindElement(locator);
            }
            catch(WebDriverTimeoutException ex)
            {
                elementWait.Until(ElementExists(locator));
                return webDriver.FindElement(locator);
            }
        }

        /// <summary>
        /// Stale-safe find element with automatic retry logic
        /// </summary>
        public static IWebElement FindElementStaleSafe(this IWebDriver webDriver, By locator, int maxRetries = 3)
        {
            int retryCount = 0;
            while (retryCount < maxRetries)
            {
                try
                {
                    return webDriver.FindElement(locator);
                }
                catch (StaleElementReferenceException)
                {
                    retryCount++;
                    if (retryCount >= maxRetries)
                        throw;
                    Thread.Sleep(500);
                }
                catch (NoSuchElementException)
                {
                    retryCount++;
                    if (retryCount >= maxRetries)
                        throw;
                    Thread.Sleep(500);
                }
            }
            throw new NoSuchElementException($"Element not found after {maxRetries} retries: {locator}");
        }

        /// <summary>
        /// Stale-safe find elements with automatic retry logic
        /// </summary>
        public static System.Collections.ObjectModel.ReadOnlyCollection<IWebElement> FindElementsStaleSafe(this IWebDriver webDriver, By locator, int maxRetries = 3)
        {
            int retryCount = 0;
            while (retryCount < maxRetries)
            {
                try
                {
                    return webDriver.FindElements(locator);
                }
                catch (StaleElementReferenceException)
                {
                    retryCount++;
                    if (retryCount >= maxRetries)
                        throw;
                    Thread.Sleep(500);
                }
            }
            throw new StaleElementReferenceException($"Elements became stale after {maxRetries} retries: {locator}");
        }

        /// <summary>
        /// Stale-safe click operation using locator
        /// </summary>
        public static void ClickStaleSafe(this IWebDriver webDriver, By locator, int maxRetries = 3)
        {
            int retryCount = 0;
            while (retryCount < maxRetries)
            {
                try
                {
                    IWebElement element = webDriver.FindElement(locator);
                    element.Click();
                    return;
                }
                catch (StaleElementReferenceException)
                {
                    retryCount++;
                    if (retryCount >= maxRetries)
                        throw;
                    Thread.Sleep(500);
                }
                catch (NoSuchElementException)
                {
                    retryCount++;
                    if (retryCount >= maxRetries)
                        throw;
                    Thread.Sleep(500);
                }
            }
            throw new NoSuchElementException($"Cannot click element after {maxRetries} retries: {locator}");
        }

        public static void SaveScreenshot(this IWebDriver webDriver, string name)
        {
            Screenshot ss = ((ITakesScreenshot)webDriver).GetScreenshot();
            string screenshot = ss.AsBase64EncodedString;
            byte[] screenshotAsByteArray = ss.AsByteArray;
            ss.SaveAsFile(name + ".png");
        }
    }
}
