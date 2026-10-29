using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using Selenium.BaseComponents.Utilities;
using SeleniumExtensions.Configurations;
using SeleniumHelper = Selenium.BaseComponents.Utilities.PageHelper;

namespace TC.PriorAuthoriztion.Pages
{
    public class MemberEligibilityPage
    {
        private readonly IWebDriver _driver;

        public MemberEligibilityPage(IWebDriver driver)
        {
            _driver = driver;
        }

        public By SearchEligibilityIcon => By.XPath("//img[@alt='Search Eligibility']");
        public By MedicaidBillingNumber => By.Id("txtMBN");
        public By Ssn => By.Id("txtSSN");
        public By DateOfBirth => By.Id("txtDOB");
        public By FromDateOfService => By.Id("txtFDOS");
        public By ToDateOfService => By.XPath("//input[@id='txtTDOS' or contains(@id,'TDOS') or contains(@id,'ToDOS') or contains(@id,'ToDos')]");
        public By SearchButton => By.Id("btnSearch");
        public By RecipientInformationHeader => By.XPath("//h1[normalize-space()='RECIPIENT INFORMATION']");
        public By BenefitPlansHeader => By.XPath("//h1[normalize-space()='BENEFIT/ASSIGNMENT PLAN(S)']");
        public By ErrorMessage => By.XPath("//span[contains(normalize-space(),'Error Code') or contains(normalize-space(),'Error Description')]");

        public void Open()
        {
            Click(SearchEligibilityIcon);
            SeleniumHelper.WaitUntilElementIsVisibleAndReturn(_driver, MedicaidBillingNumber, TimeoutConfiguration.Element);
        }

        public void Click(By by)
        {
            SeleniumHelper.WaitUntilElementIsVisibleAndReturn(_driver, by, TimeoutConfiguration.Element);
            _driver.CreateSmartElement(by).Click();
            SeleniumHelper.WaitForPostback(_driver, TimeSpan.FromSeconds(30));
        }

        public void Type(By by, string? value)
        {
            SeleniumHelper.WaitUntilElementIsVisibleAndReturn(_driver, by, TimeoutConfiguration.Element);
            var element = _driver.CreateSmartElement(by);
            element.Set(value ?? "");
            element.SendKeys(Keys.Tab);
            SeleniumHelper.WaitForPostback(_driver, TimeSpan.FromSeconds(30));
        }

        public bool IsDisplayed(By by)
        {
            try
            {
                return _driver.FindElements(by).Any(e => e.Displayed);
            }
            catch (StaleElementReferenceException)
            {
                return false;
            }
        }

        public string? VisibleText(By by)
        {
            try
            {
                return _driver.FindElements(by).FirstOrDefault(e => e.Displayed)?.Text?.Trim();
            }
            catch (StaleElementReferenceException)
            {
                return null;
            }
        }

        public string? SectionText(By header)
        {
            try
            {
                var heading = _driver.FindElements(header).FirstOrDefault(e => e.Displayed);
                var container = heading?.FindElements(By.XPath("./ancestor::div[.//*[self::table or self::input or self::span]][1]")).FirstOrDefault();
                return container?.Text?.Trim();
            }
            catch (StaleElementReferenceException)
            {
                return null;
            }
        }

        public WebDriverWait Wait(TimeSpan timeout)
        {
            var wait = new WebDriverWait(_driver, timeout);
            wait.IgnoreExceptionTypes(typeof(StaleElementReferenceException), typeof(NoSuchElementException));
            return wait;
        }
    }
}
