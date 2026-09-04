using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using Selenium.BaseComponents.Utilities;
using SeleniumExtensions.Configurations;
using System.Collections.ObjectModel;
using static SeleniumExtras.WaitHelpers.ExpectedConditions;

namespace SeleniumExtensions.Extensions
{
    public static class WebElementExtensions
    {
        /// <summary>
        /// Safely performs an action on an element with automatic stale element retry logic
        /// </summary>
        public static T SafeExecute<T>(this IWebElement element, Func<IWebElement, T> action, int maxRetries = 3)
        {
            int retryCount = 0;
            while (retryCount < maxRetries)
            {
                try
                {
                    return action(element);
                }
                catch (StaleElementReferenceException)
                {
                    retryCount++;
                    if (retryCount >= maxRetries)
                        throw;
                    Thread.Sleep(500); // Brief pause before retry
                }
            }
            throw new StaleElementReferenceException("Element is stale after maximum retries");
        }

        /// <summary>
        /// Safely performs an action on an element with automatic stale element retry logic
        /// </summary>
        public static void SafeExecute(this IWebElement element, Action<IWebElement> action, int maxRetries = 3)
        {
            int retryCount = 0;
            while (retryCount < maxRetries)
            {
                try
                {
                    action(element);
                    return;
                }
                catch (StaleElementReferenceException)
                {
                    retryCount++;
                    if (retryCount >= maxRetries)
                        throw;
                    Thread.Sleep(500); // Brief pause before retry
                }
            }
            throw new StaleElementReferenceException("Element is stale after maximum retries");
        }

        public static IWebElement SelectByIndex(this IWebElement webElement, int index)
        {
            return webElement.SafeExecute(element => 
            {
                new SelectElement(element).SelectByIndex(index);
                return element;
            });
        }

        public static IWebElement SelectByText(this IWebElement webElement, string text)
        {
            return webElement.SafeExecute(element => 
            {
                new SelectElement(element).SelectByText(text);
                return element;
            });
        }
        public static IWebElement SelectedOption(this IWebElement webElement)
        {
            SelectElement selectElement = new SelectElement(webElement);
            return selectElement.SelectedOption;
        }

        public static void WaitUntilInvisible(this IWebElement webElement, IWait<IWebDriver> elementWait, By locator)
        {
            elementWait.Until(InvisibilityOfElementLocated(locator));
        }

        public static void ClickAndWaitUntilInvisible(this IWebElement webElement, IWait<IWebDriver> elementWait, By locator)
        {
            webElement.Click();
            elementWait.Until(InvisibilityOfElementLocated(locator));
        }

        public static IWebElement WithPicklist(this IWebElement labelElement)
        {
            return labelElement.FindElement(By.XPath("following-sibling::td[position()=1]//select"));
        }

        public static IReadOnlyCollection<IWebElement> RequiredIndicators(this IWebElement labelElement)
        {
            return labelElement.FindElements(By.XPath("../parent::*[@class='requiredInput']"));
        }

        public static IReadOnlyCollection<IWebElement> DependentRequiredIndicators(this IWebElement labelElement)
        {
            return labelElement.FindElements(By.XPath("../../parent::*[@class='requiredInput']"));
        }

        public static bool ElementExists(this IWebElement element, By by)
        {
            try
            {
                element.FindElement(by);
            }
            catch (NoSuchElementException)
            {
                return false;
            }

            return true;
        }

        public static void WaitForElement(this IWebElement element, By by, TimeSpan timeout, bool suppressException = true)
        {
            try
            {
                DefaultWait<IWebElement> defaultWait = new DefaultWait<IWebElement>(element);
                defaultWait.Timeout = timeout;
                defaultWait.IgnoreExceptionTypes(typeof(NoSuchElementException));
                defaultWait.Until((IWebElement ctx) => element.FindElement(by));
            }
            catch (WebDriverTimeoutException ex)
            {
                Console.WriteLine("Element Timeout Exception. " + ex.Message);
                if (!suppressException)
                {
                    throw ex;
                }
            }
            catch (Exception ex2)
            {
                Console.WriteLine("Exception thrown. Please see Exception details " + ex2.Message);
                if (!suppressException)
                {
                    throw ex2;
                }
            }
        }

        public static IWebElement FindElement(this IWebElement element, By by, TimeSpan timeout)
        {
            return new DefaultWait<IWebElement>(element)
            {
                Timeout = timeout
            }.Until((IWebElement ctx) => (!element.FindElement(by).Displayed) ? null : element.FindElement(by));
        }

        public static ReadOnlyCollection<IWebElement> FindElements(this IWebElement element, By by, TimeSpan timeout)
        {
            return new DefaultWait<IWebElement>(element)
            {
                Timeout = timeout
            }.Until((IWebElement ctx) => (element.FindElements(by).Count <= 0) ? null : element.FindElements(by));
        }

        public static bool IsElementVisible(this IWebElement element)
        {
            if (element.Displayed)
            {
                return element.Enabled;
            }

            return false;
        }

        public static void WaitForElementDisplayed(this IWebElement element, By by, TimeSpan timeout, bool supressException = true)
        {
            try
            {
                DefaultWait<IWebElement> defaultWait = new DefaultWait<IWebElement>(element);
                defaultWait.Timeout = timeout;
                defaultWait.IgnoreExceptionTypes(typeof(NoSuchElementException));
                defaultWait.Until((IWebElement ctx) => element.FindElement(by).Displayed);
            }
            catch (WebDriverTimeoutException ex)
            {
                Console.WriteLine("Element Timeout Exception. " + ex.Message);
                if (!supressException)
                {
                    throw;
                }
            }
            catch (Exception ex2)
            {
                Console.WriteLine("Exception thrown. Please see Exception details " + ex2.Message);
                if (!supressException)
                {
                    throw;
                }
            }
        }

        public static void WaitForElementVisible(this IWebElement element, By by, TimeSpan timeout, bool suppressException = true)
        {
            try
            {
                DefaultWait<IWebElement> defaultWait = new DefaultWait<IWebElement>(element);
                defaultWait.Timeout = timeout;
                defaultWait.IgnoreExceptionTypes(typeof(NoSuchElementException));
                defaultWait.Until((IWebElement ctx) => element.FindElement(by).Displayed && element.FindElement(by).Enabled);
            }
            catch (NoSuchElementException ex)
            {
                Console.WriteLine("Element Not Found. " + ex.Message);
                if (!suppressException)
                {
                    throw;
                }
            }
            catch (WebDriverTimeoutException ex2)
            {
                Console.WriteLine("Element Timeout Exception. " + ex2.Message);
                if (!suppressException)
                {
                    throw;
                }
            }
            catch (Exception ex3)
            {
                Console.WriteLine("Exception thrown. Please see Exception details " + ex3.Message);
                if (!suppressException)
                {
                    throw;
                }
            }
        }

        public static void WaitForElementDisplayed(this IWebElement element, TimeSpan timeout, bool suppressException = true)
        {
            try
            {
                DefaultWait<IWebElement> defaultWait = new DefaultWait<IWebElement>(element);
                defaultWait.Timeout = timeout;
                defaultWait.IgnoreExceptionTypes(typeof(NoSuchElementException));
                defaultWait.Until((IWebElement ctx) => element.Displayed);
            }
            catch (WebDriverTimeoutException ex)
            {
                Console.WriteLine("Element Timeout Exception. " + ex.Message);
                if (!suppressException)
                {
                    throw;
                }
            }
            catch (Exception ex2)
            {
                Console.WriteLine("Exception thrown. Please see Exception details " + ex2.Message);
                if (!suppressException)
                {
                    throw;
                }
            }
        }

        public static void WaitForElementVisible(this IWebElement element, TimeSpan timeout, bool suppressException = true)
        {
            try
            {
                DefaultWait<IWebElement> defaultWait = new DefaultWait<IWebElement>(element);
                defaultWait.Timeout = timeout;
                defaultWait.IgnoreExceptionTypes(typeof(NoSuchElementException));
                defaultWait.Until((IWebElement ctx) => element.Displayed && element.Enabled);
            }
            catch (NoSuchElementException ex)
            {
                Console.WriteLine("Element Not Found. " + ex.Message);
                if (!suppressException)
                {
                    throw;
                }
            }
            catch (WebDriverTimeoutException ex2)
            {
                Console.WriteLine("Element Timeout Exception. " + ex2.Message);
                if (!suppressException)
                {
                    throw;
                }
            }
            catch (Exception ex3)
            {
                Console.WriteLine("Exception thrown. Please see Exception details " + ex3.Message);
                if (!suppressException)
                {
                    throw;
                }
            }
        }

        public static void WaitFor(this IWebElement element, string attributeName, ElementPropertyFilter elementPropertyFilter, string searchFor, TimeSpan timeout, bool suppressException = true)
        {
            try
            {
                DefaultWait<IWebElement> defaultWait = new DefaultWait<IWebElement>(element);
                defaultWait.Timeout = timeout;
                defaultWait.IgnoreExceptionTypes(typeof(NoSuchElementException));
                switch (elementPropertyFilter)
                {
                    case ElementPropertyFilter.contains:
                        defaultWait.Until((IWebElement ctx) => element.GetAttribute(attributeName).Contains(searchFor));
                        break;
                    case ElementPropertyFilter.endswith:
                        defaultWait.Until((IWebElement ctx) => element.GetAttribute(attributeName).EndsWith(searchFor));
                        break;
                    case ElementPropertyFilter.equals:
                        defaultWait.Until((IWebElement ctx) => element.GetAttribute(attributeName).Equals(searchFor));
                        break;
                    case ElementPropertyFilter.notequals:
                        defaultWait.Until((IWebElement ctx) => !element.GetAttribute(attributeName).Equals(searchFor));
                        break;
                    case ElementPropertyFilter.startswith:
                        defaultWait.Until((IWebElement ctx) => element.GetAttribute(attributeName).StartsWith(searchFor));
                        break;
                }
            }
            catch (NoSuchElementException ex)
            {
                Console.WriteLine("Element Not Found. " + ex.Message);
                if (!suppressException)
                {
                    throw ex;
                }
            }
            catch (WebDriverTimeoutException ex2)
            {
                Console.WriteLine("Element Timeout Exception. " + ex2.Message);
                if (!suppressException)
                {
                    throw;
                }
            }
            catch (Exception ex3)
            {
                Console.WriteLine("Exception thrown. Please see Exception details " + ex3.Message);
                if (!suppressException)
                {
                    throw;
                }
            }
        }

        public static void WaitFor(this IWebElement element, By by, string attributeName, ElementPropertyFilter elementPropertyFilter, string searchFor, TimeSpan timeout, bool suppressException = true)
        {
            try
            {
                DefaultWait<IWebElement> defaultWait = new DefaultWait<IWebElement>(element);
                defaultWait.Timeout = timeout;
                defaultWait.IgnoreExceptionTypes(typeof(NoSuchElementException));
                switch (elementPropertyFilter)
                {
                    case ElementPropertyFilter.contains:
                        defaultWait.Until((IWebElement ctx) => element.FindElement(by).GetAttribute(attributeName).Contains(searchFor));
                        break;
                    case ElementPropertyFilter.endswith:
                        defaultWait.Until((IWebElement ctx) => element.FindElement(by).GetAttribute(attributeName).EndsWith(searchFor));
                        break;
                    case ElementPropertyFilter.equals:
                        defaultWait.Until((IWebElement ctx) => element.FindElement(by).GetAttribute(attributeName).Equals(searchFor));
                        break;
                    case ElementPropertyFilter.notequals:
                        defaultWait.Until((IWebElement ctx) => !element.FindElement(by).GetAttribute(attributeName).Equals(searchFor));
                        break;
                    case ElementPropertyFilter.startswith:
                        defaultWait.Until((IWebElement ctx) => element.FindElement(by).GetAttribute(attributeName).StartsWith(searchFor));
                        break;
                }
            }
            catch (NoSuchElementException ex)
            {
                Console.WriteLine("Element Not Found. " + ex.Message);
                if (!suppressException)
                {
                    throw ex;
                }
            }
            catch (WebDriverTimeoutException ex2)
            {
                Console.WriteLine("Element Timeout Exception. " + ex2.Message);
                if (!suppressException)
                {
                    throw;
                }
            }
            catch (Exception ex3)
            {
                Console.WriteLine("Exception thrown. Please see Exception details " + ex3.Message);
                if (!suppressException)
                {
                    throw;
                }
            }
        }

        public static void Set(this IWebElement element, string text, bool isWaitRequired = false)
        {
            element.SafeExecute(elem => 
            {
                WaitForElementVisible(elem, TimeoutConfiguration.Element);
                elem.Clear();
                elem.SendKeys(text);
                if (isWaitRequired)
                {
                    Thread.Sleep(1000);
                }
            });
        }

        public static void Set(this IWebElement element, bool state)
        {
            element.SafeExecute(elem => 
            {
                WaitForElementVisible(elem, TimeoutConfiguration.Element);
                if (state)
                {
                    if (!elem.Selected)
                    {
                        elem.Click();
                    }
                }
                else if (elem.Selected)
                {
                    elem.Click();
                }
            });
        }

        public static SelectElement Select(this IWebElement element)
        {
            return element.SafeExecute(elem => new SelectElement(elem));
        }

        /// <summary>
        /// Stale-safe click method
        /// </summary>
        public static void SafeClick(this IWebElement element)
        {
            element.SafeExecute(elem => elem.Click());
        }

        /// <summary>
        /// Stale-safe text retrieval
        /// </summary>
        public static string SafeText(this IWebElement element)
        {
            return element.SafeExecute(elem => elem.Text);
        }

        /// <summary>
        /// Stale-safe attribute retrieval
        /// </summary>
        public static string SafeGetAttribute(this IWebElement element, string attributeName)
        {
            return element.SafeExecute(elem => elem.GetAttribute(attributeName));
        }

        /// <summary>
        /// Stale-safe displayed check
        /// </summary>
        public static bool SafeDisplayed(this IWebElement element)
        {
            return element.SafeExecute(elem => elem.Displayed);
        }

        /// <summary>
        /// Stale-safe enabled check
        /// </summary>
        public static bool SafeEnabled(this IWebElement element)
        {
            return element.SafeExecute(elem => elem.Enabled);
        }

        /// <summary>
        /// Stale-safe selected check
        /// </summary>
        public static bool SafeSelected(this IWebElement element)
        {
            return element.SafeExecute(elem => elem.Selected);
        }
    }
}
