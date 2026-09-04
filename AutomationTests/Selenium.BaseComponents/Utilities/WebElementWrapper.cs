using OpenQA.Selenium;
using Selenium.BaseComponents.Pages;
using System.Collections.ObjectModel;

namespace Selenium.BaseComponents.Utilities
{
    public class WebElementWrapper : BasePage
    {
        public IWebDriver webDriver;


        public WebElementWrapper(IWebDriver webDriver) : base(webDriver)
        {
            this.webDriver = webDriver;
        }

        public IWebElement webElement = null;
        public IWebElement getElement(String locatorType, String locatorValue)
        {
            By by = null;
            IJavaScriptExecutor js = (IJavaScriptExecutor)webDriver;
            switch (locatorType.ToLower())
            {
                case "id":
                    by = By.Id(locatorValue);
                    break;
                case "name":
                    by = By.Name(locatorValue);
                    break;
                case "xpath":
                    by = By.XPath(locatorValue);
                    break;
                case "classname":
                    by = By.ClassName(locatorValue);
                    break;
                case "cssselector":
                    by = By.CssSelector(locatorValue);
                    break;
                case "linktext":
                    by = By.LinkText(locatorValue);
                    break;
            }

            int time = 0;
            while (time < 80)
            {
                try
                {

                    webElement = webDriver.FindElement(by);
                    js.ExecuteScript("arguments[0].style.border='2px solid red';", webElement);
                    Thread.Sleep(200);
                    js.ExecuteScript("arguments[0].style.border='2px solid blue';", webElement);
                    Thread.Sleep(200);
                    js.ExecuteScript("arguments[0].style.border='2px solid black';", webElement);
                    Thread.Sleep(200);
                    js.ExecuteScript("arguments[0].style.border='2px solid green';", webElement);
                    return webElement;
                }
                catch (Exception)
                {
                    System.Console.WriteLine("Wating for the element to visible " + locatorValue);
                    Thread.Sleep(1000);
                    time++;

                }
            }
            return null;
        }



        // Visit Ops

        public IWebElement FindElement(String locatorType, String locatorValue)
        {
            By by = null;
            IJavaScriptExecutor js = (IJavaScriptExecutor)webDriver;
            switch (locatorType.ToLower())
            {
                case "id":
                    by = By.Id(locatorValue);
                    break;
                case "name":
                    by = By.Name(locatorValue);
                    break;
                case "xpath":
                    by = By.XPath(locatorValue);
                    break;
                case "classname":
                    by = By.ClassName(locatorValue);
                    break;
                case "cssselector":
                    by = By.CssSelector(locatorValue);
                    break;
                case "linktext":
                    by = By.LinkText(locatorValue);
                    break;
            }

            int time = 0;
            while (time < 20)
            {
                try
                {
                    if (webDriver.FindElements(By.XPath("//span[text()='Processing...']")).Count > 0)
                    {
                        webElement = webDriver.FindElement(By.XPath("//span[contains(@id,'progressIndicator.start') and contains(@style,'none')]"));
                    }
                    webElement = webDriver.FindElement(by);
                    js.ExecuteScript("arguments[0].style.border='2px solid red';", webElement);
                    Thread.Sleep(200);
                    js.ExecuteScript("arguments[0].style.border='2px solid blue';", webElement);
                    Thread.Sleep(200);
                    js.ExecuteScript("arguments[0].style.border='2px solid black';", webElement);
                    Thread.Sleep(200);
                    js.ExecuteScript("arguments[0].style.border='2px solid green';", webElement);
                    return webElement;
                }
                catch (Exception)
                {
                    System.Console.WriteLine("Wating for the element to visible " + locatorValue);
                    Thread.Sleep(1000);
                    time++;

                }
            }
            return null;
        }


        public void ClickByJS()
        {

            IJavaScriptExecutor js = (IJavaScriptExecutor)webDriver;
            js.ExecuteScript("arguments[0].click()", webElement);
        }
    }

    public class RadioButtons
    {
        public RadioButtons(IWebDriver driver, ReadOnlyCollection<IWebElement> webElements)
        {
            Driver = driver;
            WebElements = webElements;
        }

        protected IWebDriver Driver { get; }

        protected ReadOnlyCollection<IWebElement> WebElements { get; }

        public void SelectValue(String value)
        {
            WebElements.Single(we => we.GetAttribute("text") == value).Click();
        }
    }

    /// <summary>
    /// Smart element wrapper that automatically handles stale elements by refreshing them
    /// This allows minimal changes to existing page objects while fixing caching issues
    /// </summary>
    public class SmartElement
    {
        private readonly IWebDriver _driver;
        private readonly By _locator;
        private IWebElement _cachedElement;
        private readonly Func<By> _locatorFunc;

        public SmartElement(IWebDriver driver, By locator)
        {
            _driver = driver;
            _locator = locator;
        }

        public SmartElement(IWebDriver driver, Func<By> locatorFunc)
        {
            _driver = driver;
            _locatorFunc = locatorFunc;
        }

        /// <summary>
        /// Gets the element, automatically refreshing if stale
        /// </summary>
        public IWebElement Element
        {
            get
            {
                try
                {
                    // Try to use cached element first
                    if (_cachedElement != null)
                    {
                        // Quick check if element is still valid
                        var displayed = _cachedElement.Displayed;
                        return _cachedElement;
                    }
                }
                catch (StaleElementReferenceException)
                {
                    // Element is stale, will refresh below
                }
                catch (NoSuchElementException)
                {
                    // Element not found, will refresh below
                }

                // Refresh the element
                return RefreshElement();
            }
        }

        /// <summary>
        /// Forces a refresh of the element
        /// </summary>
        public IWebElement RefreshElement()
        {
            var locator = _locatorFunc != null ? _locatorFunc() : _locator;
            _cachedElement = _driver.FindElement(locator);
            return _cachedElement;
        }

        // Note: Implicit conversion to IWebElement is not allowed in C# for interfaces
        // Use the .Element property explicitly instead

        // Common element operations that automatically handle staleness
        public void Click() => Element.Click();
        public void Clear() => Element.Clear();
        public void SendKeys(string text) => Element.SendKeys(text);
        public string Text => Element.Text;
        public bool Displayed => Element.Displayed;
        public bool Enabled => Element.Enabled;
        public bool Selected => Element.Selected;
        public string GetAttribute(string name) => Element.GetAttribute(name);
    }

    /// <summary>
    /// Extension methods to create smart wrappers with minimal code changes
    /// </summary>
    public static class SmartElementExtensions
    {
        /// <summary>
        /// Creates a smart wrapper from a locator function
        /// Usage: var smartElement = driver.CreateSmartElement(() => By.Id("myElement"));
        /// </summary>
        public static SmartElement CreateSmartElement(this IWebDriver driver, Func<By> locatorFunc)
        {
            return new SmartElement(driver, locatorFunc);
        }

        /// <summary>
        /// Creates a smart wrapper from a static locator
        /// Usage: var smartElement = driver.CreateSmartElement(By.Id("myElement"));
        /// </summary>
        public static SmartElement CreateSmartElement(this IWebDriver driver, By locator)
        {
            return new SmartElement(driver, locator);
        }

        /// <summary>
        /// Wraps an existing IWebElement property with smart refresh logic
        /// Usage: var smartElement = existingElementProperty.AsSmartElement(driver, () => By.Id("myElement"));
        /// </summary>
        public static SmartElement AsSmartElement(this IWebElement element, IWebDriver driver, Func<By> locatorFunc)
        {
            return new SmartElement(driver, locatorFunc);
        }
    }
}
