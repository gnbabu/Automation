using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using SdetToolbox.Pages;
using Selenium.BaseComponents.Pages;
using SeleniumExtensions.Configurations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using SeleniumExtensions.Extensions;

namespace TC.ProviderDataEntry.Pages.Registration
{
    public class WorkflowPage:BasePage
    {
        IWebDriver TestWebDriver;
        string RegID;

        public WorkflowPage(IWebDriver TestWebDriver, string RegID) : base(TestWebDriver)
        {

            this.TestWebDriver = TestWebDriver;

            string url = this.TestWebDriver.Url;

            if (!string.IsNullOrEmpty(RegID))
            {
                this.RegID = RegID;
            }
            else
            {
                var regIDTag = url.Split('?').Last();
                var regID = regIDTag.Split('=').Last();
                this.RegID = regID;
            }
        }

        public bool RunWorkflow()
        {

            bool status = false;

            TestWebDriver.FindElement(By.Id("menu")).Click();

            PageHelper.WaitUntilElementIsVisible(TestWebDriver, By.XPath($"//a[@title='Workflow Engine']"), TimeoutConfiguration.Element);

            IWebElement workflowSearch = TestWebDriver.FindElement(By.XPath($"//a[@title='Workflow Engine']"));

            if (workflowSearch != null)
            {
                workflowSearch.Click();
            }

            PageHelper.WaitUntilElementIsVisible(TestWebDriver, By.XPath($"//*[@id='btnRefresh']"), TimeoutConfiguration.Element);

            WebDriverWait wait = new WebDriverWait(TestWebDriver, TimeoutConfiguration.Element);
            wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementToBeClickable(By.XPath("//*[@id='gvAwaitingAction_ctl00_ctl02_ctl02_FilterTextBox_REGISTRATION_ID']"))).Set(RegID, true);
            wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementToBeClickable(By.XPath("//*[@id='gvAwaitingAction_ctl00_ctl02_ctl02_Filter_REGISTRATION_ID']"))).Click();
            wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementToBeClickable(By.XPath($"//*[@id='gvAwaitingAction_rfltMenu_detached']/ul/li/a[span[text()='EqualTo']]"))).Click();

            bool isRecordProcessed = false;
            bool isNoRecordsFound = false;

            while (!isRecordProcessed)
            {
                try
                {
                    IWebElement resultGridMain = TestWebDriver.FindElement(By.XPath("//*[@id='gvAwaitingAction_ctl00']//tr[contains(@class,'rgNoRecords')]"));
                    if (resultGridMain != null || resultGridMain.IsElementVisible())
                    {
                        isNoRecordsFound = true;
                        isRecordProcessed = true;
                    }
                }
                catch
                {

                }
                if (!isNoRecordsFound)
                {
                    IWebElement resultGrid = TestWebDriver.FindElement(By.XPath("//*[@id='gvAwaitingAction_ctl00']//a[text()='Process']"));
                    if (resultGrid.IsElementVisible())
                        resultGrid.Click();
                    else
                        isRecordProcessed = true;
                }
            }
            wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementToBeClickable(By.XPath($"//*[@id='btnLogout']"))).Click();
            return status;
        }
    }
}
