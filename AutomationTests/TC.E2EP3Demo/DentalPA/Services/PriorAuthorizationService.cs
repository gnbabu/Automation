using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using SdetToolbox.Pages;
using Selenium.BaseComponents.Utilities;
using SeleniumExtensions.Configurations;
using SeleniumExtensions.Extensions;
using System;
using System.Threading;
using TC.PriorAuthoriztion.Models;
using TC.PriorAuthoriztion.Pages;
using NUnit.Framework;

namespace TC.PriorAuthoriztion.Services
{
    /// <summary>
    /// Service layer for Prior Authorization business logic
    /// Separates business logic from test classes for better maintainability
    /// </summary>
    public class PriorAuthorizationService
    {
        private readonly IWebDriver _webDriver;

        public PriorAuthorizationService(IWebDriver webDriver)
        {
            _webDriver = webDriver;
        }

        /// <summary>
        /// Fills dental PA fields with provided data
        /// </summary>
        public void FillDentalPAFields(DentalPA dentalPA)
        {
            // Extract business logic from test class
            // This method will contain all the field filling logic
            // Implementation to be added based on existing test logic
        }

        /// <summary>
        /// Handles diagnosis popup search
        /// </summary>
        public void DiagnosisPopupSearch(string diagnosisCode)
        {
            IWebElement searchPopup = _webDriver.CreateSmartElement(By.Id("ctl00_MainContent_uc1SubmitPriorAuthorization_pnlDiagnosisSearch1")).Element;

            if (searchPopup.Displayed)
            {
                _webDriver.CreateSmartElement(By.Id("ctl00_MainContent_uc1SubmitPriorAuthorization_txtDiagnosisCodeSearch1")).Element.Set(diagnosisCode);
                _webDriver.CreateSmartElement(By.Id("ctl00_MainContent_uc1SubmitPriorAuthorization_btndiagnosiscodeSearch")).Element.Click();

                Thread.Sleep(5000);

                IWebElement diagnosisOutput = _webDriver.CreateSmartElement(By.Id("DiagnosisOutput")).Element;

                if (diagnosisOutput.Displayed)
                {
                    if (!diagnosisOutput.Text.Equals("No Diagnosis Found."))
                        _webDriver.CreateSmartElement(By.XPath("//*[@id='DiagnosisOutput']/table/tbody/tr[2]/td[1]/a")).Element.Click();
                    else
                        Assert.Fail("no diagnosis data found with the given code.");
                }
            }
        }

        /// <summary>
        /// Handles procedure code popup search
        /// </summary>
        public void ProcedureCodePopupSearch(string procedureCode)
        {
            _webDriver.CreateSmartElement(By.Id("ctl00_MainContent_uc1SubmitPriorAuthorization_txtCode")).Element.Set(procedureCode);
            _webDriver.CreateSmartElement(By.Id("ctl00_MainContent_uc1SubmitPriorAuthorization_Button9")).Element.Click();

            Thread.Sleep(5000);

            IWebElement procedureCodeOutput = _webDriver.CreateSmartElement(By.XPath("//*[@id='procedureCodeOutput']/div")).Element;

            if (procedureCodeOutput.Displayed)
            {
                if (!procedureCodeOutput.Text.Equals("No data found."))
                    _webDriver.CreateSmartElement(By.XPath("//*[@id='tableData']/tbody/tr[2]/td[1]/a")).Element.Click();
                else
                    throw new Exception("no procedure code data found with the given code.");
            }
        }

        /// <summary>
        /// Handles warning acknowledgment dialog
        /// </summary>
        public void HandleWarningAcknowledgment()
        {
            IWebElement messageWarning = _webDriver.CreateSmartElement(By.Id("ctl00_MainContent_uc1SubmitPriorAuthorization_pnlWarningAcknowledgment")).Element;

            if (messageWarning.Displayed)
            {
                _webDriver.CreateSmartElement(By.Id("ctl00_MainContent_uc1SubmitPriorAuthorization_btnWarningAcknowledgmentYes")).Element.Click();
            }
        }

        /// <summary>
        /// Clicks submit button with JavaScript execution
        /// </summary>
        public void ClickSubmitButton(IWebElement submitButton)
        {
            IJavaScriptExecutor jsExecutor = (IJavaScriptExecutor)_webDriver;
            jsExecutor.ExecuteScript("arguments[0].click();", submitButton);
        }

        /// <summary>
        /// Expands section if collapsed
        /// </summary>
        public void ExpandSectionIfCollapsed(IWebElement expanderSpan, string panelId)
        {
            string expanderText = expanderSpan.Text;
            if (expanderText.Equals("+"))
            {
                _webDriver.CreateSmartElement(By.Id(panelId)).Element.Click();
                Thread.Sleep(2000);
            }
        }
    }
}
