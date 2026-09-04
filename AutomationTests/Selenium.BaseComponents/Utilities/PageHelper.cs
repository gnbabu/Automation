
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;
using System.Collections.ObjectModel;
using System.Globalization;


namespace Selenium.BaseComponents.Utilities
{
    public static class PageHelper
    {
        public const int ONEMINUTE = 60;
        public static int PageTimeOut = ONEMINUTE;// Convert.ToInt32(ConfigurationManager.AppSettings["PageTimeout"]);
        public static int ControlTimeOut = ONEMINUTE;// Convert.ToInt32(ConfigurationManager.AppSettings["ControlTimeout"]);
        public static int SleepTimeOut = ONEMINUTE;// Convert.ToInt32(ConfigurationManager.AppSettings["SleepTimeOut"]);
        public static int accountPageTimeout = ONEMINUTE;//Convert.ToInt32(ConfigurationManager.AppSettings["AccountPageTimeout"]);

        #region Private Methods

        /// <summary>
        /// Get the index of a column in the html table.
        /// </summary>
        /// <param name="htmlTable">Instance of web element i.e. html table</param>
        /// <param name="columnHeader">Name of the column header.</param>
        /// <returns>Returns the 0-based position of the column header inside the html table.</returns>
        private static int GetColumnIndex(IWebElement htmlTable, string columnHeader)
        {
            var headerRow = htmlTable.FindElement(By.CssSelector("tr[class='headerRow']"));
            var headers = headerRow.FindElements(By.TagName("th"));
            int column1Index = -1;
            for (int i = 0; i < headers.Count; i++)
            {
                IWebElement currentHeader = headers[i];
                if (currentHeader.Text.Trim().Equals(columnHeader.Trim(), StringComparison.CurrentCultureIgnoreCase))
                {
                    column1Index = i;
                    break;
                }
            }

            if (column1Index < 0)
                throw new ArgumentOutOfRangeException(string.Format("Unable to get the index of column {0}", columnHeader));

            return column1Index;
        }

        public static void WaitUntilElementNotVisible(IWebDriver webDriver, IWebElement selectPermissionsetvalue, TimeSpan timeSpan)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region General Helper Methods

        public static DateTime? ParseDateToStandardFormat(string date)
        {
            if (string.IsNullOrWhiteSpace(date))
                return null;

            if (date.Contains(""))
                date = date.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries)[0];

            int day, month, year;
            string[] dateParts = date.Split(new string[] { "/", "-" }, StringSplitOptions.RemoveEmptyEntries);

            if (dateParts.Length != 3)
                throw new ArgumentException(string.Format("Date {0} is not in a correct format", date));

            bool parsingSucessfull = false;
            parsingSucessfull = int.TryParse(dateParts[0], out month);
            parsingSucessfull = int.TryParse(dateParts[1], out day);
            parsingSucessfull = int.TryParse(dateParts[2], out year);

            if (!parsingSucessfull)
                throw new ArgumentException(string.Format("Date {0} is not in a correct format", date));

            return new DateTime(year, month, day);
        }

        #endregion

        #region IWebDriver Extensions

        private static IWebElement WaitForElement(IWebDriver webDriver, By locator, IWebElement containerElement)
        {
            WebDriverWait _driverWait = new WebDriverWait(webDriver, TimeSpan.FromSeconds(PageTimeOut));
            _driverWait.PollingInterval = TimeSpan.FromSeconds(1);
            int counter = PageTimeOut;

            try
            {
                return _driverWait.Until<IWebElement>(d =>
                {
                    if (counter == 0)
                        return webDriver.FindElement(locator);
                    counter--;
                    try
                    {
                        if (containerElement != null)
                            return containerElement.FindElement(locator);
                        else
                            return webDriver.FindElement(locator);
                    }
                    catch (UnhandledAlertException)
                    {
                        webDriver.SwitchTo().Alert().Accept();
                        return null;
                    }
                    catch (NoSuchElementException)
                    {
                        return null;
                    }
                    catch (WebDriverException)
                    {
                        try
                        {
                            return webDriver.FindElement(locator);
                        }
                        catch
                        {
                            return null;
                        }
                    }
                });
            }
            catch (WebDriverTimeoutException)
            {
                return webDriver.FindElement(locator);
            }
        }

        /// <summary>
        /// Find UI Element on a page using an instance of WebDriver.
        /// </summary>
        /// <param name="webDriver">An instance of WebDriver.</param>
        /// <param name="locator">Locator to use while finding the UI Element on a page.</param>
        /// <param name="containerElement">A containing or parent element, if any, inside which to look for the UI Element.</param>
        /// <returns>Returns the located UI Element.</returns>
        public static IWebElement FindElement(this IWebDriver webDriver, By locator, IWebElement containerElement)
        {
            try
            {
                return WaitForElement(webDriver, locator, containerElement);
            }
            catch (WebDriverTimeoutException)
            {
                return WaitForElement(webDriver, locator, containerElement);
            }
        }

        /// <summary>
        /// Wait until the html document is finished processing or loading.
        /// </summary>
        /// <param name="webDriver">An instance of web driver.</param>
        /// <param name="timeInSeconds">Time in seconds to use as a timeout for document to load.</param>
        public static void WaitUntilDocumentIsReady(this IWebDriver webDriver, TimeSpan timeInSeconds)
        {
            try
            {
                IJavaScriptExecutor jsEngine = (IJavaScriptExecutor)webDriver;

                WebDriverWait _waiter = new WebDriverWait(webDriver, timeInSeconds.Add(TimeSpan.FromSeconds(30)));
                _waiter.PollingInterval = TimeSpan.FromSeconds(1);
                _waiter.IgnoreExceptionTypes(typeof(FormatException));
                _waiter.Until<bool>((e) =>
                {
                    try
                    {
                        string documentState = jsEngine.ExecuteScript("return document.readyState").ToString();
                        return documentState.Equals("complete", StringComparison.InvariantCultureIgnoreCase);
                    }
                    catch (UnhandledAlertException)
                    {
                        try
                        {
                            webDriver.SwitchTo().Alert().Accept();
                            webDriver.SwitchTo().DefaultContent();
                        }
                        catch (NoAlertPresentException)
                        {
                            return false;
                        }

                        return false;
                    }
                    catch (WebDriverException)
                    {
                        return true;
                    }
                });
            }
            catch (WebDriverException ex) { Console.WriteLine(ex.Message); }
            catch (Exception ex) { Console.WriteLine(ex.Message); }
        }

        /// <summary>
        /// Wait until the web element is not visible in the DOM.
        /// </summary>
        /// <param name="webDriver">An instance of a web driver.</param>
        /// <param name="locator">The locator of the web element for which to wait until invisible.</param>
        /// <param name="timeout">Timeout, in seconds, for the web element to become invisible in the DOM.</param>
        public static void WaitUntilElementNotVisible(this IWebDriver webDriver, By locator, TimeSpan timeout)
        {
            IJavaScriptExecutor jsEngine = (IJavaScriptExecutor)webDriver;

            WebDriverWait _waiter = new WebDriverWait(webDriver, timeout);
            _waiter.PollingInterval = TimeSpan.FromSeconds(1);
            _waiter.Until<bool>((e) =>
            {
                IWebElement element = null;
                try
                {
                    element = webDriver.FindElement(locator);
                }
                catch (NoSuchElementException)
                {
                    return true;
                }

                return !element.Displayed;
            });
        }

        public static void WaitUntilElementIsVisible(this IWebDriver webDriver, By locator, TimeSpan timeout)
        {
            WebDriverWait waiter = new WebDriverWait(webDriver, timeout);
            waiter.PollingInterval = TimeSpan.FromSeconds(1);

            waiter.Until(driver =>
            {
                try
                {
                    var element = driver.FindElement(locator);
                    return element.Displayed;
                }
                catch (NoSuchElementException)
                {
                    return false;
                }
                catch (StaleElementReferenceException)
                {
                    return false;
                }
                catch (Exception)
                {
                    return false;
                }
            });
        }

        public static bool IsElementIsVisible(this IWebDriver webDriver, By locator, TimeSpan timeout)
        {
            // IJavaScriptExecutor jsEngine = (IJavaScriptExecutor)webDriver;
            bool isExist = false;
            WebDriverWait _waiter = new WebDriverWait(webDriver, timeout);
            _waiter.PollingInterval = TimeSpan.FromSeconds(1);
            _waiter.Until<bool>((e) =>
            {
                IWebElement element = null;
                try
                {
                    element = webDriver.FindElement(locator);
                    if (element != null)
                    {
                        isExist = true;
                    }
                }
                catch (NoSuchElementException)
                {
                    return false;
                }
                catch (Exception)
                {
                    return false;
                }
                return isExist;
            });
            return isExist;
        }

        public static IWebElement WaitUntilElementIsVisibleAndReturn(this IWebDriver webDriver, By locator, TimeSpan timeout)
        {
            WebDriverWait wait = new WebDriverWait(webDriver, timeout);
            webDriver.WaitUntilElementIsVisible(locator,timeout);
            return webDriver.FindElement(locator);
        }

        /// <summary>
        /// Stale-safe element finding with automatic retry
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
        /// Stale-safe element finding in container with automatic retry
        /// </summary>
        public static IWebElement FindElementStaleSafe(this IWebElement container, By locator, int maxRetries = 3)
        {
            int retryCount = 0;
            while (retryCount < maxRetries)
            {
                try
                {
                    return container.FindElement(locator);
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
            throw new NoSuchElementException($"Element not found in container after {maxRetries} retries: {locator}");
        }

        /// <summary>
        /// Stale-safe click using locator with automatic element refresh
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

        /// <summary>
        /// Stale-safe text retrieval using locator with automatic element refresh
        /// </summary>
        public static string GetTextStaleSafe(this IWebDriver webDriver, By locator, int maxRetries = 3)
        {
            int retryCount = 0;
            while (retryCount < maxRetries)
            {
                try
                {
                    IWebElement element = webDriver.FindElement(locator);
                    return element.Text;
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
            throw new NoSuchElementException($"Cannot get text after {maxRetries} retries: {locator}");
        }

        /// <summary>
        /// Smart wait that replaces Thread.Sleep with intelligent condition checking
        /// Usage: SmartWait(webDriver, 3000, () => element.Displayed);
        /// </summary>
        public static void SmartWait(this IWebDriver webDriver, int timeoutMs, Func<bool> condition = null, int checkInterval = 100)
        {
            if (condition == null)
            {
                // If no condition provided, just wait (similar to Thread.Sleep but more efficient)
                Thread.Sleep(timeoutMs);
                return;
            }

            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            while (stopwatch.ElapsedMilliseconds < timeoutMs)
            {
                try
                {
                    if (condition())
                        return;
                }
                catch
                {
                    // Ignore exceptions during condition check
                }
                Thread.Sleep(checkInterval);
            }
        }

        /// <summary>
        /// Smart wait for element to be visible (replaces Thread.Sleep before element interaction)
        /// Usage: SmartWaitForVisible(webDriver, By.Id("myElement"), 5000);
        /// </summary>
        public static void SmartWaitForVisible(this IWebDriver webDriver, By locator, int timeoutMs = 5000)
        {
            SmartWait(webDriver, timeoutMs, () =>
            {
                try
                {
                    return webDriver.FindElement(locator).Displayed;
                }
                catch
                {
                    return false;
                }
            });
        }

        /// <summary>
        /// Smart wait for element to be clickable (replaces Thread.Sleep before click)
        /// Usage: SmartWaitForClickable(webDriver, By.Id("myButton"), 5000);
        /// </summary>
        public static void SmartWaitForClickable(this IWebDriver webDriver, By locator, int timeoutMs = 5000)
        {
            SmartWait(webDriver, timeoutMs, () =>
            {
                try
                {
                    var element = webDriver.FindElement(locator);
                    return element.Displayed && element.Enabled;
                }
                catch
                {
                    return false;
                }
            });
        }

        /// <summary>
        /// Smart wait for element to contain text (replaces Thread.Sleep after text entry)
        /// Usage: SmartWaitForText(webDriver, By.Id("myElement"), "expected text", 5000);
        /// </summary>
        public static void SmartWaitForText(this IWebDriver webDriver, By locator, string expectedText, int timeoutMs = 5000)
        {
            SmartWait(webDriver, timeoutMs, () =>
            {
                try
                {
                    return webDriver.FindElement(locator).Text.Contains(expectedText);
                }
                catch
                {
                    return false;
                }
            });
        }

        /// <summary>
        /// Smart wait for element to be invisible (replaces Thread.Sleep after closing/disappearing elements)
        /// Usage: SmartWaitForInvisible(webDriver, By.Id("loadingSpinner"), 5000);
        /// </summary>
        public static void SmartWaitForInvisible(this IWebDriver webDriver, By locator, int timeoutMs = 5000)
        {
            SmartWait(webDriver, timeoutMs, () =>
            {
                try
                {
                    return !webDriver.FindElement(locator).Displayed;
                }
                catch (NoSuchElementException)
                {
                    return true; // Element not found means it's invisible
                }
                catch
                {
                    return false;
                }
            });
        }

        /// <summary>
        /// Smart wait for AJAX completion (replaces Thread.Sleep after dynamic content updates)
        /// Usage: SmartWaitForAjax(webDriver, 5000);
        /// </summary>
        public static void SmartWaitForAjax(this IWebDriver webDriver, int timeoutMs = 5000)
        {
            SmartWait(webDriver, timeoutMs, () =>
            {
                try
                {
                    var js = (IJavaScriptExecutor)webDriver;
                    return (bool)js.ExecuteScript("return jQuery.active == 0");
                }
                catch
                {
                    // If jQuery is not available, assume AJAX is complete
                    return true;
                }
            });
        }

        /// <summary>
        /// Smart wait for page load (replaces Thread.Sleep after navigation)
        /// Usage: SmartWaitForPageLoad(webDriver, 5000);
        /// </summary>
        public static void SmartWaitForPageLoad(this IWebDriver webDriver, int timeoutMs = 5000)
        {
            SmartWait(webDriver, timeoutMs, () =>
            {
                try
                {
                    var js = (IJavaScriptExecutor)webDriver;
                    return js.ExecuteScript("return document.readyState").ToString() == "complete";
                }
                catch
                {
                    return false;
                }
            });
        }

        /// <summary>
        /// Smart wait for custom condition (replaces complex Thread.Sleep scenarios)
        /// Usage: SmartWaitUntil(webDriver, 5000, () => webDriver.FindElements(By.ClassName("item")).Count > 5);
        /// </summary>
        public static void SmartWaitUntil(this IWebDriver webDriver, int timeoutMs, Func<bool> condition)
        {
            SmartWait(webDriver, timeoutMs, condition);
        }

        public static IWebElement WaitUntilElementIsClickableAndReturn(this IWebDriver webDriver, By locator, TimeSpan timeout)
        {
            WebDriverWait wait = new WebDriverWait(webDriver, timeout);
            wait.Until(ExpectedConditions.ElementToBeClickable(locator));
            return webDriver.FindElement(locator);
        }

        public static void WaitUntilElementNotAvailable(this IWebDriver webDriver, By locator, TimeSpan timeout)
        {
            IJavaScriptExecutor jsEngine = (IJavaScriptExecutor)webDriver;

            WebDriverWait _waiter = new WebDriverWait(webDriver, timeout);
            _waiter.PollingInterval = TimeSpan.FromSeconds(1);
            _waiter.Until<bool>((e) =>
            {
                try
                {
                    IWebElement element = webDriver.FindElement(locator);
                }
                catch (NoSuchElementException)
                {
                    return true;
                }
                catch (WebDriverException)
                {
                    return true;
                }

                return false;
            });
        }

        /// <summary>
        /// Wait until the web element is not visible in the DOM.
        /// </summary>
        /// <param name="webDriver">An instance of a web driver.</param>
        /// <param name="containerElement">A parent or top level element inside the element is being loaded.</param>
        /// <param name="locator">The locator of the web element for which to wait until invisible.</param>
        /// <param name="timeout">Timeout, in seconds, for the web element to become invisible in the DOM.</param>
        public static void WaitUntilElementNotVisible(this IWebDriver webDriver, IWebElement containerElement, By locator, TimeSpan timeout)
        {
            IJavaScriptExecutor jsEngine = (IJavaScriptExecutor)webDriver;

            WebDriverWait _waiter = new WebDriverWait(webDriver, timeout);
            _waiter.PollingInterval = TimeSpan.FromSeconds(1);
            bool elementFound = false;
            int counter = 0;
            _waiter.Until<bool>((e) =>
            {
                try
                {
                    IWebElement element = containerElement.FindElement(locator);
                    elementFound = true;
                    return !element.Displayed;
                }
                catch (NoSuchElementException)
                {
                    if (counter > 5)
                        return true;

                    if (elementFound)
                        return true;
                    else
                        return false;
                }
                finally
                {
                    counter++;
                }
            });
        }


        /// <summary>
        /// Navigate to the url and wait until the document is ready.
        /// </summary>
        /// <param name="webDriver">An instance of the web driver.</param>
        /// <param name="url">The url to go to.</param>
        public static void GoToUrl(this IWebDriver webDriver, string url)
        {
            webDriver.WaitUntilDocumentIsReady(TimeSpan.FromSeconds(PageTimeOut));
            webDriver.Navigate().GoToUrl(url);
            webDriver.WaitUntilDocumentIsReady(TimeSpan.FromSeconds(PageTimeOut));
        }

        /// <summary>
        /// Double the web element.
        /// </summary>
        /// <param name="webDriver">An instance of the web driver.</param>
        /// <param name="webElement">An instance of the web element that should be double clicked.</param>
        public static void DoubleClick(this IWebDriver webDriver, IWebElement webElement)
        {
            OpenQA.Selenium.Interactions.Actions _actions = new OpenQA.Selenium.Interactions.Actions(webDriver);
            _actions.DoubleClick(webElement).Perform();
        }

        /// <summary>
        /// Navigate back to previous page.
        /// </summary>
        /// <param name="webDriver">An instance of the web driver.</param>
        public static void NavigateBack(this IWebDriver webDriver)
        {
            webDriver.Navigate().Back();
        }

        /// <summary>
        /// Switch to a window with a title
        /// </summary>
        /// <param name="webDriver"></param>
        /// <param name="title"></param>
        /// <param name="t"></param>
        /// <returns></returns>
        public static bool SwitchToWindow(this IWebDriver webDriver, string title, TimeSpan t)
        {
            IWait<IWebDriver> wait = new WebDriverWait(webDriver, t);
            return wait.Until<bool>(d => d.WindowHandles.Any(wh => webDriver.SwitchTo().Window(wh).Title.Contains(title)));
        }

        /// <summary>
        /// Scroll to the web element.
        /// </summary>
        /// <param name="_webDriver">An instance of web driver current in use.</param>
        /// <param name="webElement">A web element to scroll into.</param>
        public static void ScrollIntoView(this IWebDriver _webDriver, IWebElement webElement)
        {
            ((IJavaScriptExecutor)_webDriver).ExecuteScript("arguments[0].scrollIntoView(true);", webElement);
        }

        public static void ScrollIntoCenterView(this IWebDriver _webDriver, IWebElement webElement)
        {
            Thread.Sleep(TimeSpan.FromSeconds(2));
            ((IJavaScriptExecutor)_webDriver).ExecuteScript("arguments[0].scrollIntoView({block: 'center'});", webElement);
        }
        public static void ScrollDown(this IWebDriver _webDriver, string val = "1000")
        {
            //((IJavaScriptExecutor)_webDriver).ExecuteScript("window.scrollTo(0, document.body.scrollHeight)");
            ((IJavaScriptExecutor)_webDriver).ExecuteScript($"window.scrollBy(0, {val})");
        }
        public static void LaunchInternalApp(this IWebDriver _webDriver, String openInternalURL)
        {
            ((IJavaScriptExecutor)_webDriver).ExecuteScript("window.open('" + openInternalURL + "');");
            ((IJavaScriptExecutor)_webDriver).ExecuteScript("window.close();");
        }

        /// <summary>
        /// Switch to a pop-up window
        /// </summary>
        /// <param name="_webDriver">An instance of web driver current in use.</param>
        /// <param name="windowTitle">A title of the pop-up window</param>
        /// <param name="partialWindowTitle">True if the title of the window is partially provided else false. Defaults to true.</param>
        /// <returns>Returns the webdriver referencing the pop-up window.</returns>
        public static IWebDriver SwitchToWindow(this IWebDriver _webDriver, string windowTitle, bool partialWindowTitle = true)
        {
            var openedWindowHandles = _webDriver.WindowHandles;
            var searchTimeout = 20000;
            while (openedWindowHandles.Count <= 1)
            {
                if (searchTimeout <= 0)
                    return null;

                searchTimeout--;
                System.Threading.Thread.Sleep(1000);
                openedWindowHandles = _webDriver.WindowHandles;
            }

            string currentWindowHandle = _webDriver.CurrentWindowHandle;

            foreach (var handle in openedWindowHandles)
            {
                if (handle.Equals(currentWindowHandle))
                    continue;

                _webDriver.SwitchTo().Window(handle);

                if (partialWindowTitle == true && _webDriver.Title.Contains(windowTitle))
                {
                    return _webDriver;
                }

                if (partialWindowTitle == false && _webDriver.Title.Equals(windowTitle))
                {
                    return _webDriver;
                }
            }

            _webDriver.SwitchTo().DefaultContent();
            return null;
        }
     
        public static FileInfo SaveFileToUserDownloadLocation(this IWebDriver _webDriver, string fileExtension)
        {
            //string browserType = ((RemoteWebDriver)_webDriver).Capabilities.BrowserName;

            //string browserName = string.Empty;
            //if (Capabilities.HasCapability(“browserName”))
            //{
            //    browserName = Capabilities.GetCapability(“browserName”).ToString();
            //}
            // Wait for the download to complete
            // Note: Windows UI Automation APIs (AutomationElement, etc.) are not available in .NET 8.
            // The browser should be configured to auto-download files without a dialog.
            System.Threading.Thread.Sleep(5000);

            return _webDriver.GetRecentlyDownloadedFile(fileExtension);
        }

        public static FileInfo GetRecentlyDownloadedFile(this IWebDriver _webDriver, string extension)
        {
            DirectoryInfo downloadFolderPath = new DirectoryInfo(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + @"\Downloads");
            extension = "*." + extension;
            bool isDownloadInProgress = true;
            int waitCounter = 120;
            while (isDownloadInProgress)
            {
                System.Threading.Thread.Sleep(TimeSpan.FromSeconds(1));
                waitCounter--;
                FileInfo[] downloadingFiles = downloadFolderPath.GetFiles(extension + ".part", SearchOption.TopDirectoryOnly);
                FileInfo latestFile = downloadingFiles.OrderByDescending(e => e.CreationTime).FirstOrDefault();
                if (latestFile == null)
                    isDownloadInProgress = false;
            }

            var files = downloadFolderPath.GetFiles(extension, SearchOption.TopDirectoryOnly);
            return files.OrderByDescending(e => e.CreationTime).FirstOrDefault();
        }

        public static void WaitUntilElementIsDisabled(this IWebDriver _webDriver, By locator)
        {
            WebDriverWait _wait = new WebDriverWait(_webDriver, TimeSpan.FromSeconds(PageHelper.ControlTimeOut));
            _wait.Until<bool>(d =>
            {
                try
                {
                    IWebElement element = _webDriver.FindElement(locator);
                    string attrDisabled = element.GetAttribute("disabled");
                    if (!string.IsNullOrWhiteSpace(attrDisabled) && (attrDisabled.ToLower() == "disabled" || attrDisabled.ToLower() == "true"))
                        return true;
                }
                catch (StaleElementReferenceException)
                {
                    return false;
                }
                return false;
            });
        }

        public static IWebDriver GetWebDriver(IWebElement element)
        {
            if (element.GetType().ToString() == "OpenQA.Selenium.Support.PageObjects.WebElementProxy")
            {
                return ((IWrapsDriver)element
                                 .GetType().GetProperty("WrappedElement")
                                 .GetValue(element, null)).WrappedDriver;
            }
            else
            {
                return ((IWrapsDriver)element).WrappedDriver;
            }

        }

        public static int generateRandomValuesWithMax(int max)
        {
            long value = DateTime.Now.Ticks;
            long result = (value % max == 0) ? ((value % max) + 1) : (value % max);
            return (int)(result);
        }

        public static int generateRandomValuesWithinRange(int min, int max)
        {
            long value = DateTime.Now.Ticks;
            long result;
            int intRange = min - max;
            result = ((value % intRange) + min);
            return (int)(result);
        }

        public static void SwitchToWindowwithoneTab(IWebDriver webDriver)
        {
            var tabs = webDriver.WindowHandles;
            if (tabs != null)
            {
                webDriver.SwitchTo().Window(tabs[1]);

            }

        }
        #endregion

        #region IWebElement Extensions

        /// <summary>
        /// Find children of web element.
        /// </summary>
        /// <param name="webElement">An instance of web element.</param>
        /// <returns>Returns the read only collection of web element which are children of webElement parameter.</returns>
        public static ReadOnlyCollection<IWebElement> FindChildren(this IWebElement webElement)
        {
            return webElement.FindElements(By.XPath("./*"));
        }

        /// <summary>
        /// Find the matching sibling of current web element.
        /// </summary>
        /// <param name="webElement">An instance of web element.</param>
        /// <param name="selector">A selector of the sibling web element. This could be id, class name or css selector but it needs to be of only the needed sibling.</param>
        /// <returns>Returns the matched sibling web element.</returns>
        public static IWebElement FindSibling(this IWebElement webElement, string selector)
        {
            return webElement.FindElement(By.XPath("./parent::*[1]" + selector));
        }

        /// <summary>
        /// Get the next sibling of the web element that immediately follows.
        /// </summary>
        /// <param name="webElement">An instance of the web element of which the next sibling is requested.</param>
        /// <returns>Returns the sibling of the web element that immediately follows.</returns>
        public static IWebElement GetImmediateNextSibling(this IWebElement webElement)
        {
            return webElement.FindElement(By.XPath("./following-sibling::*[1]"));
        }

        /// <summary>
        /// Get the next sibling of the web element that immediately follows.
        /// </summary>
        /// <param name="webElement">An instance of the web element of which the next sibling is requested.</param>
        /// <param name="tagName">The name of the tag that matches the following sibling of the web element.</param>
        /// <returns>Returns the sibling of the web element that immediately follows.</returns>
        public static IWebElement GetImmediateNextSibling(this IWebElement webElement, string tagName)
        {
            return webElement.FindElement(By.XPath("./following-sibling::" + tagName + "[1]"));
        }

        /// <summary>
        /// Verify the font size of text inside the html element like div, label, span, etc.
        /// </summary>
        /// <param name="webElement">An instance of web element representing the html element.</param>
        /// <param name="expectedFontSize">Expected font size (for e.g. 32px or 32em) of the text.</param>
        /// <param name="messageOnFailure">Message to display if the actual font size of the text is not equal to expected font size.</param>
        public static void AssertFontSize(this IWebElement webElement, string expectedFontSize, string messageOnFailure)
        {
            webElement.GetCssValue("font-size").Should().BeEquivalentTo(expectedFontSize, messageOnFailure);
        }

        /// <summary>
        /// Verify the height and width of a logo i.e. image element
        /// </summary>
        /// <param name="imageElement">An instance of web element representing the image.</param>
        /// <param name="expectedWidth">Expected width of the image element.</param>
        /// <param name="expectedHeight">Expected height of the image element.</param>
        /// <param name="messageOnFailure">Message to display if the actual size of the image or logo is not equal to expected size.</param>
        public static void AssertLogoSize(this IWebElement imageElement, string expectedWidth, string expectedHeight, string messageOnFailure)
        {
            string actualWidth, actualHeight;
            System.Drawing.Size sizeOfLogo = imageElement.Size;
            actualWidth = sizeOfLogo.Width.ToString();
            actualHeight = sizeOfLogo.Height.ToString();
            actualWidth.Should().BeEquivalentTo(expectedWidth, messageOnFailure);
            actualHeight.Should().BeEquivalentTo(expectedHeight, messageOnFailure);
        }

        /// <summary>
        /// Check whether the element exist or not.
        /// </summary>
        /// <param name="_webDriver">An instance of web driver.</param>
        /// <param name="locator">The locator of the element that needs to be verified for existence.</param>
        /// <returns>Returns true if the element available or exist on the page else returns false.</returns>
        public static bool IsElementExist(this IWebDriver _webDriver, By locator)
        {
            try
            {
                IWebElement element = _webDriver.FindElement(locator);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static bool IsElementNotExist(this IWebDriver _webDriver, By locator)
        {
            try
            {
                IWebElement element = _webDriver.FindElement(locator);
                return false;
            }
            catch (NoSuchElementException)
            {
                return true;
            }
        }

        #endregion

        #region DatePicker helpers

        /// <summary>
        /// Select date and month from date picker control.
        /// </summary>
        /// <param name="datePickerElement">An instance of web element representing the date picker ui element.</param>
        /// <param name="date">The day of the month to select.</param>
        /// <param name="month">The month to select from the date picker.</param>
        public static void SelectDateFromDatePicker(this IWebElement datePickerElement, string date, string month)
        {
            datePickerElement.FindElement(By.Id("calMonthPicker")).SelectItemByText(month);
            datePickerElement.FindElement(By.XPath("//tr/td[text()='" + date + "']")).Click();
        }

        /// <summary>
        /// Select date, month and year from date picker control.
        /// </summary>
        /// <param name="datePickerElement">An instance of web element representing the date picker ui element.</param>
        /// <param name="date">The day of the month to select.</param>
        /// <param name="month">The month to select from the date picker.</param>
        /// <param name="year">The year to select from the date picker.</param>
        public static void SelectDateFromDatePicker(this IWebElement datePickerElement, string date, string month, string year)
        {
            datePickerElement.FindElement(By.Id("calMonthPicker")).SelectItemByText(month);
            datePickerElement.FindElement(By.Id("calYearPicker")).SelectItemByText(year);
            datePickerElement.FindElement(By.XPath("//tr/td[text()='" + date + "']")).Click();
        }

        /// <summary>
        /// Select date, month and year from date picker control.
        /// </summary>
        /// <param name="datePickerElement">An instance of web element representing the date picker ui element.</param>
        /// <param name="dayOfMonth">The day of the month to select.</param>
        /// <param name="month">The month to select from the date picker.</param>
        /// <param name="year">The year to select from the date picker.</param>
        public static void SelectDateFromDatePicker(this IWebElement datePickerElement, int dayOfMonth, int month, int year)
        {
            datePickerElement.FindElement(By.Id("calMonthPicker")).SelectItemByText(CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(month));
            datePickerElement.FindElement(By.Id("calYearPicker")).SelectItemByText(year.ToString());
            datePickerElement.FindElement(By.XPath(".//table[@id='datePickerCalendar']//tr[@class='calRow']/td[not(contains(@class,'prevMonth'))][text()='" + dayOfMonth + "']")).Click();
        }

        public static void SelectTodayFromDatePicker(this IWebElement datePickerElement)
        {
            datePickerElement.FindElement(By.XPath(".//table[@id='datePickerCalendar']/following-sibling::div[@class='buttonBar']/a[text()='Today']")).Click();
        }

        #endregion

        #region HTML Select helpers

        public static void SelectItemByText(this IWebElement dropDownElement, string text, bool ignoreCase)
        {
            if (text == null)
                throw new ArgumentNullException("text");

            if (text == string.Empty)
                return;

            /* Sometimes, when reading data from excel the space character (ASCII value is 32) is returned as a non-breaking space 
             * (ASCII value is 160) which also looks like a space. So below line is to replace the non-breaking space with the actual space. */
            string valueToSelect = text.Replace((char)160, (char)32);
            if (ignoreCase)
            {
                string[] options = dropDownElement.GetListItems();
                foreach (string item in options)
                {
                    if (item.Equals(valueToSelect, StringComparison.OrdinalIgnoreCase))
                        (new SelectElement(dropDownElement)).SelectByText(item);
                }
            }
            else
            {
                (new SelectElement(dropDownElement)).SelectByText(valueToSelect);
            }
        }

        /// <summary>
        /// Select an item from html dropdown element.
        /// </summary>
        /// <param name="dropDownElement">Html Select element</param>
        /// <param name="text">The item's text to select.</param>
        public static void SelectItemByText(this IWebElement dropDownElement, string text)
        {
            if (text == null)
                throw new ArgumentNullException("text");

            if (text == string.Empty)
                return;

            /* Sometimes, when reading data from excel the space character (ASCII value is 32) is returned as a non-breaking space 
             * (ASCII value is 160) which also looks like a space. So below line is to replace the non-breaking space with the actual space. */
            string valueToSelect = text.Replace((char)160, (char)32);
            (new SelectElement(dropDownElement)).SelectByText(valueToSelect);
        }

        public static void SelectItemByIndex(this IWebElement dropDownElement, int index)
        {
            if (index < 0)
                throw new ArgumentOutOfRangeException("Index cannot be less than 0");

            (new SelectElement(dropDownElement)).SelectByIndex(index);
        }

        /// <summary>
        /// Get the 0-based index of the selected item from the pick list.
        /// </summary>
        /// <param name="dropDownElement">The html select element</param>
        /// <returns>Returns the 0-based index of the selected item from the pick list.</returns>
        public static int GetSelectedIndex(this IWebElement dropDownElement)
        {
            SelectElement list = new SelectElement(dropDownElement);
            IWebElement selectedItem = list.SelectedOption;
            var listItems = list.Options;
            for (int i = 0; i < listItems.Count; i++)
            {
                if (listItems[i].Text == selectedItem.Text)
                    return i;
            }
            return -1;
        }

        /// <summary>
        /// Get the count of options element present in the Select element.
        /// </summary>
        /// <param name="dropDownElement">The html select element</param>
        /// <returns>Returns the count of options element present in the Select element.</returns>
        public static int GetListItemsCount(this IWebElement dropDownElement)
        {
            return (new SelectElement(dropDownElement)).Options.Count;
        }

        /// <summary>
        /// Select an item from html dropdown element whose text matches partially.
        /// </summary>
        /// <param name="dropDownElement">Html Select element</param>
        /// <param name="partialText">The partial text of the item</param>
        public static void SelectItemByPartialText(this IWebElement dropDownElement, string partialText)
        {
            var options = dropDownElement.FindElements(By.TagName("option"));
            int index = 0;
            for (int i = index; i < options.Count; i++)
            {
                if (options[i].Text.Contains(partialText))
                {
                    (new SelectElement(dropDownElement)).SelectByIndex(i);
                    break;
                }
            }
        }

        public static void SelectItemByTextValue(this IWebElement Element, string Text)
        {
            var options = Element.FindElements(By.TagName("option"));
            int index = 0;
            for (int i = index; i < options.Count; i++)
            {
                if (options[i].Text.Equals(Text))
                {
                    (new SelectElement(Element)).SelectByIndex(i);
                    break;
                }
            }
        }

      

        public static void SelectMultipleByIndexes(this IWebElement listBoxElement, params int[] indexes)
        {
            SelectElement multiOptionPicklist = new SelectElement(listBoxElement);
            foreach (int index in indexes)
                multiOptionPicklist.SelectByIndex(index);
        }

        /// <summary>
        /// Get the selected item from the drop down i.e. html select element.
        /// </summary>
        /// <param name="dropDownElement">The html select element (a web element instance).</param>
        /// <returns>Returns the selected item from drop down element.</returns>
        public static string GetSelectedItem(this IWebElement dropDownElement)
        {
            SelectElement _selectElement = new SelectElement(dropDownElement);
            return _selectElement.SelectedOption.Text;
        }

        /// <summary>
        /// Get the list items i.e. the text of all option tags inside select tag.
        /// </summary>
        /// <param name="dropDownElement">The list box or pick list element i.e. html select tag.</param>
        /// <returns>Returns an array of string for the text of all the options containing in list box or pick list.</returns>
        public static string[] GetListItems(this IWebElement dropDownElement)
        {
            var options = dropDownElement.FindElements(By.TagName("option"));
            string[] listItems = new string[options.Count];
            for (int i = 0; i < listItems.Length; i++)
                listItems[i] = options[i].Text;
            return listItems;
        }

        public static string[] GetListItems(this IWebDriver webDriver, By locator)
        {
            WebDriverWait explicitWait = new WebDriverWait(webDriver, TimeSpan.FromSeconds(60));
            SelectElement dropDown = explicitWait.Until<SelectElement>(d =>
            {
                SelectElement element;
                try
                {
                    element = new SelectElement(webDriver.FindElement(locator));
                    return element;
                }
                catch (NoSuchElementException)
                {
                    return null;
                }
            });
            var options = dropDown.Options;
            List<string> items = new List<string>();
            for (int i = 0; i < options.Count; i++)
            {
                items.Add(options[i].Text.Trim());
            }
            return items.ToArray();
        }

        /// <summary>
        /// Verify the html pick list values. The list of values passed by user should be present in the Lead Routing pick list with no extra and no less values.
        /// </summary>
        /// <param name="dropDown">Html select element.</param>
        /// <param name="rangeOfValues">A range of values to verify it exists in the html select control.</param>
        public static void VerifyDropDownValues(this IWebElement dropDown, IList<string> rangeOfValues)
        {
            List<string> values = new List<string>(rangeOfValues);
            var options = dropDown.FindElements(By.TagName("option"));

            foreach (var option in options)
            {
                if (values.Contains(option.Text) == false)
                    Assert.Fail(string.Format("The value {0} is available in a drop down which is not expected.", option.Text));
                else
                    values.Remove(option.Text);
            }

            if (values.Count > 0)
            {
                string message = string.Format("The following values are not available in a {0} drop down", dropDown.Text);
                values.ForEach(s => message += s + ";");
                Assert.Fail(message);
            }
        }
        public static void VerifyDropDownValuesandSequence(this IWebElement dropDown, IList<string> rangeOfValues)
        {
            List<string> values = new List<string>(rangeOfValues);
            var options = dropDown.FindElements(By.TagName("option")).Select(t => t.Text);
            values.SequenceEqual(options).Should().BeTrue();
        }
        public static void VerifyDropDownValuesNotPresent(this IWebElement dropDown, IList<string> rangeOfValues)
        {
            List<string> values = new List<string>(rangeOfValues);
            List<string> unexpectedValues = new List<string>();
            var options = dropDown.FindElements(By.TagName("option"));

            foreach (var option in options)
            {
                if (values.Contains(option.Text) == true)
                    unexpectedValues.Add(option.Text);
            }

            if (unexpectedValues.Count > 0)
            {
                string message = string.Format("The following values are present in picklist");
                unexpectedValues.ForEach(s => message += s + ";");
                Assert.Fail(message);
            }
        }

        /// <summary>
        /// Returns true if the list item exists in the dropdown (i.e. pick list control)
        /// </summary>
        /// <param name="pickList">An instance of web element representing the pick list control.</param>
        /// <param name="listItem">One of the value available in the pick list control.</param>
        /// <returns>Return true if the list item exists in the pick list control else return false.</returns>
        public static bool IsPickListItemExist(this IWebElement pickList, string listItem)
        {
            return ((new SelectElement(pickList)).Options.FirstOrDefault(e => e.Text.Trim().Equals(listItem, StringComparison.CurrentCultureIgnoreCase)) != null);
        }

        /// <summary>
        /// Verify the items are present in the drop down list (i.e. web element "select").
        /// </summary>
        /// <param name="pickList">An instance of web element that represents the drop down list.</param>
        /// <param name="listItems">A list of items to verify if present in the drop down list. Just pass items like "item1", "item2", "item3", and so on.</param>
        public static void AssertPickListForItems(this IWebElement pickList, params string[] listItems)
        {
            List<string> unavailableListItems = new List<string>();
            foreach (var item in listItems)
            {
                if (pickList.IsPickListItemExist(item) == false)
                {
                    unavailableListItems.Add(item);
                }
            }

            string failureMessage = "Below list items are not available in the Primary Program Focus pick list" + Environment.NewLine;
            unavailableListItems.ForEach(item => failureMessage += (item + Environment.NewLine));

            unavailableListItems.Should().BeEmpty(failureMessage);
        }

        public static void WaitUntilItemAvailableInPickList(this IWebElement pickList, string value)
        {
            int timeout = PageHelper.ControlTimeOut;
            while (timeout > 0)
            {
                if (IsPickListItemExist(pickList, value))
                    return;
                System.Threading.Thread.Sleep(1000);
                timeout--;
            }
        }

        #endregion

        #region HTML Table helpers

        public static int GetIndexOf(this IWebElement htmlTable, string columnName, bool ignoreActionColumn)
        {
            int index = 0;
            IWebElement headerRow = htmlTable.FindElement(By.XPath(".//tr[@class='headerRow']"));
            IReadOnlyCollection<IWebElement> headers = null;
            if (ignoreActionColumn)
                headers = headerRow.FindElements(By.XPath("./th[not(@class='actionColumn')]"));
            else
                headers = headerRow.FindElements(By.TagName("th"));
            foreach (var item in headers)
            {
                if (item.Text.Equals(columnName, StringComparison.InvariantCultureIgnoreCase))
                    return index;
                else
                    index++;
            }

            return index;
        }

        public static int LightningGetIndexOf(this IWebElement htmlTable, string columnName, bool ignoreActionColumn)
        {
            int index = 0;
            IWebElement headerRow = htmlTable.FindElement(By.XPath(".//tr[@class='slds-text-title--caps']"));
            IReadOnlyCollection<IWebElement> headers = null;
            if (ignoreActionColumn)
                headers = headerRow.FindElements(By.XPath("./th[not(@class='actionColumn')]"));
            else
                headers = headerRow.FindElements(By.TagName("th"));
            foreach (var item in headers)
            {
                if (item.Text.Equals(columnName, StringComparison.InvariantCultureIgnoreCase))
                    return index;
                else
                    index++;
            }

            return index;
        }

        /// <summary>
        /// Finds the 0-based index of the column in the table.
        /// </summary>
        /// <param name="htmlTable">A web element instance representing the html table.</param>
        /// <param name="columnName">The name of the column to search in the table.</param>
        /// <returns>Returns the 0-based index of the column in the table.</returns>
        public static int GetColumnPosition(this IWebElement htmlTable, string columnName)
        {
            return GetColumnIndex(htmlTable, columnName);
        }

        /// <summary>
        /// Find all rows i.e. 'tr' tags of html table element.
        /// </summary>
        /// <param name="htmlTable">An instance of web element which is html table.</param>
        /// <returns>Returns a read only collection of web element i.e. html tr elements which are present in the html table.</returns>
        public static ReadOnlyCollection<IWebElement> FindAllRows(IWebElement htmlTable)
        {
            return htmlTable.FindElements(By.CssSelector("tr[class*='dataRow']")); ;
        }

        /// <summary>
        /// Find a particular row inside the html table using the header name and the respective cell's value.
        /// </summary>
        /// <param name="htmlTable">Web element instance which is html table.</param>
        /// <param name="column1Header">Text of the column header to use as a filter for locating the required row.</param>
        /// <param name="column1Value">The cell's value under the column header to use as a filter for locating the required row.</param>
        /// <returns>Returns the located row i.e. html 'tr' which satisfies the filter where header is column1Header and its value is column1Value.</returns>
        public static IWebElement FindRow(this IWebElement htmlTable, string column1Header, string column1Value)
        {
            int column1Index = GetColumnIndex(htmlTable, column1Header);

            IWebElement requiredRow = null;
            var dataRows = FindAllRows(htmlTable);
            foreach (var row in dataRows)
            {
                if (row.FindChildren()[column1Index].Text.Equals(column1Value, StringComparison.CurrentCultureIgnoreCase))
                {
                    requiredRow = row;
                    break;
                }
            }

            return requiredRow;
        }

        /// <summary>
        /// Find a particular row inside the html table using the header name and the respective cell's value. This overloaded function uses a range of headers and cell's value.
        /// </summary>
        /// <param name="htmlTable">Web element instance which is html table.</param>
        /// <param name="column1Header">Text of the column header to use as a filter for locating the required row.</param>
        /// <param name="column1Value">The cell's value under the column header to use as a filter for locating the required row.</param>
        /// <param name="column2Header">A 2nd column header to further filter down the rows that is requested.</param>
        /// <param name="column2Value">A column value respective to the 2nd column header.</param>
        /// <returns>Returns the located row i.e. html 'tr' which satisfies the filter where header is column1Header, column2Header and its value is column1Value, column2Value.</returns>
        public static IWebElement FindRow(this IWebElement htmlTable, string column1Header, string column1Value, string column2Header, string column2Value)
        {
            int column1Index = GetColumnIndex(htmlTable, column1Header);
            int column2Index = GetColumnIndex(htmlTable, column2Header);

            IWebElement requiredRow = null;
            var dataRows = FindAllRows(htmlTable);
            foreach (var row in dataRows)
            {
                var cells = row.FindChildren();
                if (cells[column1Index].Text.Equals(column1Value, StringComparison.CurrentCultureIgnoreCase) &&
                    cells[column2Index].Text.Equals(column2Value, StringComparison.CurrentCultureIgnoreCase))
                {
                    requiredRow = row;
                    break;
                }
            }

            return requiredRow;
        }

        /// <summary>
        /// Get the value of a cell of html table.
        /// </summary>
        /// <param name="htmlTable">Web element instance which is html table.</param>
        /// <param name="columnHeader">Text of the column header for which the cell's value is needed.</param>
        /// <returns>Returns the collection of values under a particular cell.</returns>
        public static List<string> GetCellValues(this IWebElement htmlTable, string columnHeader)
        {
            List<string> columnValues = new List<string>();
            int index = GetColumnIndex(htmlTable, columnHeader);

            var dataRows = htmlTable.FindElements(By.CssSelector("tr[class*='dataRow']"));
            foreach (var row in dataRows)
            {
                columnValues.Add(row.FindChildren()[index].Text);
            }

            return columnValues;
        }

        /// <summary>
        /// Returns the text of the 'td' element under the specific 'tr' element of the 'table' element.
        /// </summary>
        /// <param name="htmlTable">The html table (i.e. an instance of web element) that represents the html table.</param>
        /// <param name="tableRow">The table row (i.e. an instance of web element) that represents the 'tr' node of 'table'.</param>
        /// <param name="columnName">The name of the column (i.e. 'td' under 'tr' of 'table') for which the value is requested.</param>
        /// <returns>Returns the text of the cell (i.e. 'td').</returns>
        public static string GetCellValue(this IWebElement htmlTable, IWebElement tableRow, string columnName)
        {
            int cellIndex = GetColumnIndex(htmlTable, columnName);
            var columns = tableRow.FindChildren();
            return columns[cellIndex].Text;
        }

        /// <summary>
        /// Get the html table cell (i.e. a td element of a html table).
        /// </summary>
        /// <param name="htmlTable">The html table (a web element instance representing the html table).</param>
        /// <param name="identifierCellName">The name of the cell that uniquely represents a row of the html table.</param>
        /// <param name="identifierCellValue">The value of the call that uniquely represents a row of the html table.</param>
        /// <param name="columnName">The name of the column that needs to be returned.</param>
        /// <returns>Returns the web element instance of the td element of html table.</returns>
        public static IWebElement GetHtmlTableCell(this IWebElement htmlTable, string identifierCellName, string identifierCellValue, string columnName)
        {
            int identifierColumnIndex = GetColumnIndex(htmlTable, identifierCellName);
            int index = GetColumnIndex(htmlTable, columnName);
            var dataRows = htmlTable.FindElements(By.CssSelector("tr:not(.headerRow)"));
            foreach (var row in dataRows)
            {
                var columns = row.FindChildren();
                try
                {
                    IWebElement link = columns[identifierColumnIndex].FindElement(By.TagName("a"));
                    if (link.Text.Equals(identifierCellValue, StringComparison.CurrentCultureIgnoreCase))
                    {
                        return columns[index];
                    }
                }
                catch (NoSuchElementException)
                {
                    if (columns[identifierColumnIndex].Text.Equals(identifierCellValue, StringComparison.CurrentCultureIgnoreCase))
                    {
                        return columns[index];
                    }
                }
            }

            return null;
        }
        public static void ClickHtmlTableCell(this IWebElement htmlTable, string identifierCellName, string identifierCellValue, string columnName)
        {
            int identifierColumnIndex = GetColumnIndex(htmlTable, identifierCellName);
            int index = GetColumnIndex(htmlTable, columnName);
            var dataRows = htmlTable.FindElements(By.CssSelector("tr:not(.headerRow)"));
            foreach (var row in dataRows)
            {
                var columns = row.FindChildren();
                try
                {
                    IWebElement link = columns[identifierColumnIndex].FindElement(By.TagName("a"));
                    if (link.Text.Equals(identifierCellValue, StringComparison.CurrentCultureIgnoreCase))
                    {
                        //return columns[index];
                        link.Click();
                        break;
                    }
                }
                catch (NoSuchElementException)
                {
                    if (columns[identifierColumnIndex].Text.Equals(identifierCellValue, StringComparison.CurrentCultureIgnoreCase))
                    {
                        columns[index].Click();
                        break;
                    }
                }
            }


        }
        public static string GetTableCellData(IWebDriver webDriver, IWebElement element, string strColName, int intRowNum, string columnAttribute)
        {
            string tableCellValue = null;
            IWebElement oTbl;
            IWebElement firstRow;
            int cc = 0;
            try
            {
                //oTbl = webDriver.FindElement(locator);
                oTbl = element;
                IList<IWebElement> rows = oTbl.FindElements(By.TagName("tr"));
                firstRow = rows[0];
                IList<IWebElement> cols = firstRow.FindElements(By.TagName(columnAttribute));
                for (int c = 1; c < cols.Count; c++)
                {
                    IWebElement column = cols[c];
                    string val = column.Text;
                    if (val.Equals(strColName.Trim()))
                    {
                        cc = c;
                        break;
                    }
                }
                IList<IWebElement> rows1 = oTbl.FindElements(By.TagName("tr"));
                tableCellValue = rows1[(intRowNum - 1)].FindElements(By.TagName("td"))[cc - 1].Text;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            return tableCellValue;
        }

        public static IWebElement[] GetAllRows(this IWebElement htmlTable)
        {
            return htmlTable.FindElements(By.XPath(".//tr[contains(@class,'dataRow')]")).ToArray<IWebElement>();
        }

        /// <summary>
        /// Get the html table cell (i.e. a td element of a html table).
        /// </summary>
        /// <param name="htmlTable">The html table (a web element instance representing the html table).</param>
        /// <param name="rowIndex">A zero-based index of a row from which to pick up the required column.</param>
        /// <param name="columnName">The name of the column that needs to be returned.</param>
        /// <returns>Returns the web element instance of the td element of html table.</returns>
        public static IWebElement GetHtmlTableCell(this IWebElement htmlTable, int rowIndex, string columnName)
        {
            int index = GetColumnIndex(htmlTable, columnName);
            var row = htmlTable.FindElements(By.CssSelector("tr:not(.headerRow)"))[rowIndex];
            return row.FindChildren()[index];
        }

        /// <summary>
        /// Get the column header (i.e. a th element) inside the html table.
        /// </summary>
        /// <param name="htmlTable">A web element instance representing the html table.</param>
        /// <param name="columnHeaderName">Name of the column header.</param>
        /// <returns>Returns the instance of web element representing the column header i.e. th of html table.</returns>
        public static IWebElement GetColumnHeader(this IWebElement htmlTable, string columnHeaderName)
        {
            var headerRow = htmlTable.FindElement(By.CssSelector("tr[class='headerRow']"));
            var columns = headerRow.FindElements(By.TagName("th"));
            foreach (var cell in columns)
            {
                if (cell.Text.Equals(columnHeaderName, StringComparison.CurrentCultureIgnoreCase))
                {
                    return cell;
                }
            }

            return null;
        }

        /// <summary>
        /// Verify if a cell with a value exists in html table.
        /// </summary>
        /// <param name="htmlTable">Web element instance which is html table.</param>
        /// <param name="columnHeader">Text of the column header for which the cell's value is needed.</param>
        /// <param name="columnValue">The cell's value under the column header to use as a filter for locating the required row.</param>
        /// <returns>Returns true if the cell's value 'columnValue' is found under the header 'columnHeader'.</returns>
        public static bool VerifyCellWithValueExist(IWebElement htmlTable, string columnHeader, string columnValue)
        {
            var values = GetCellValues(htmlTable, columnHeader);
            return values.Where(e => e.Equals(columnValue, StringComparison.CurrentCultureIgnoreCase)).Count() > 0;
        }

        /// <summary>
        /// Sort the html table in ascending order.
        /// </summary>
        /// <param name="webDriver">An instance of the web driver.</param>
        /// <param name="htmlTable">An instance of web element representing the html table.</param>
        /// <param name="sortByColumn">The column of the html table that needs to be sorted.</param>
        public static void SortInAscending(this IWebDriver webDriver, IWebElement htmlTable, string sortByColumn)
        {
            var columnHeader = htmlTable.GetColumnHeader(sortByColumn);
            columnHeader.Click();
            webDriver.WaitUntilDocumentIsReady(TimeSpan.FromSeconds(PageTimeOut));
            columnHeader = htmlTable.GetColumnHeader(sortByColumn);
            if (columnHeader.FindElement(By.TagName("img")).GetAttribute("title") != "Sorted Ascending")
            {
                columnHeader.Click();
                webDriver.WaitUntilDocumentIsReady(TimeSpan.FromSeconds(PageTimeOut));
            }
        }

        /// <summary>
        /// Sort the html table in descending order.
        /// </summary>
        /// <param name="webDriver">An instance of the web driver.</param>
        /// <param name="htmlTable">An instance of web element representing the html table.</param>
        /// <param name="sortByColumn">The column of the html table that needs to be sorted.</param>
        public static void SortInDescending(this IWebDriver webDriver, IWebElement htmlTable, string sortByColumn)
        {
            var columnHeader = htmlTable.GetColumnHeader(sortByColumn);
            columnHeader.Click();
            webDriver.WaitUntilDocumentIsReady(TimeSpan.FromSeconds(PageTimeOut));
            columnHeader = htmlTable.GetColumnHeader(sortByColumn);
            if (columnHeader.FindElement(By.TagName("img")).GetAttribute("title") != "Sorted Descending")
            {
                columnHeader.Click();
                webDriver.WaitUntilDocumentIsReady(TimeSpan.FromSeconds(PageTimeOut));
            }
        }

        /// <summary>
        /// Export the rows of the html table to excel.
        /// </summary>
        /// <param name="_htmlTable">An instance of web element that represents the html table.</param>
        /// <param name="saveInPath">Path for the saving the excel file.</param>
        //public static void ExportToExcel(this IWebElement _htmlTable, string saveInPath)
        //{
        //    ExcelHelper _excelHelper = new ExcelHelper(false, true);
        //    _excelHelper.Export(_htmlTable, saveInPath);
        //}

        #endregion

        #region HTML Checkbox helpers

        public static bool IsChecked(this IWebElement _webElement)
        {
            return _webElement.Selected;
        }

        public static void Check(this IWebElement _webElement)
        {
            if (!_webElement.Selected)
                _webElement.Click();
        }

        public static void UnCheck(this IWebElement _webElement)
        {
            if (_webElement.Selected)
                _webElement.Click();
        }

        #endregion

        #region HTML Textbox helpers

        public static string GetTextBoxValue(this IWebDriver _webDriver, IWebElement textBox)
        {
            return (string)((IJavaScriptExecutor)_webDriver).ExecuteScript("return arguments[0].value", textBox);
        }

        #endregion
        
    }
}
