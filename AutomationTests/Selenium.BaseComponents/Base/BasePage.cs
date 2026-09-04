using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using SdetToolbox.Pages;
using SeleniumExtensions.Configurations;
using SeleniumExtensions.Extensions;

namespace Selenium.BaseComponents.Pages
{
    public abstract class BasePage
    {
        public IWebDriver WebDriver { get; } = null;
        public IWait<IWebDriver> ElementWait { get; } = null;
		 public string Url { get; set; }
        public string Name { get; set; }

        public BasePage(IWebDriver webDriver)
        {
            this.WebDriver = webDriver;
            this.ElementWait = new WebDriverWait(webDriver, TimeoutConfiguration.Element);
            
            this.WebDriver.WaitUntilDocumentIsReady(TimeoutConfiguration.Page);
        }

        public BasePage(IWebDriver webDriver, IWait<IWebDriver> elementWait)
        {
            this.WebDriver = webDriver;
            this.ElementWait = elementWait;
        }

        public BasePage(IWebDriver webDriver, Func<IWebDriver, bool> urlCondition)
        {
            this.WebDriver = webDriver;
            ElementWait = new WebDriverWait(webDriver, TimeoutConfiguration.Element);

            try
            {
                WebDriverWait pageWait = new WebDriverWait(webDriver, TimeoutConfiguration.Redirect);
                pageWait.Until(urlCondition);
            }
            catch (WebDriverTimeoutException exception)
            {
                webDriver.SaveScreenshot(this.GetType().Name + " not loaded");

                string message = this.GetType().Name + " not loaded after " + TimeoutConfiguration.Redirect + " seconds. The page url is " + this.WebDriver.Url;
                throw new UnexpectedPageException(message, exception);
            }

            this.WebDriver.WaitUntilDocumentIsReady(TimeoutConfiguration.Page);
        }

        internal class UnexpectedPageException : Exception
        {
            public UnexpectedPageException(string message, Exception innerException) : base(message, innerException)
            {
            }
        }
    }
}
