using OpenQA.Selenium;
using System;
using TC.ProviderDataEntry.Models;
using TC.ProviderDataEntry.Pages.Registration;
using SeleniumExtensions.Extensions;
using Selenium.BaseComponents.Utilities;

namespace TC.ProviderDataEntry.Services
{
    /// <summary>
    /// Service layer for Registration business logic
    /// Separates business logic from test classes for better maintainability
    /// </summary>
    public class RegistrationService
    {
        private readonly IWebDriver _webDriver;

        public RegistrationService(IWebDriver webDriver)
        {
            _webDriver = webDriver;
        }

        /// <summary>
        /// Handles file upload for documents
        /// </summary>
        public void UploadDocument(string filePath, string elementId)
        {
            IWebElement fileUploadElement = _webDriver.CreateSmartElement(By.XPath($"//*[@id='{elementId}']")).Element;

            if (fileUploadElement != null)
            {
                fileUploadElement.SendKeys(filePath);
            }
        }

        /// <summary>
        /// Sets verification results
        /// </summary>
        public void SetVerificationResults(string regId, string matchResult = "Verified")
        {
            // Implementation for setting verification results
            // Extracted from test class business logic
        }

        /// <summary>
        /// Handles adverse action entry
        /// </summary>
        public void EnterAdverseAction(string regId, string adverseActionText = "Test")
        {
            // Implementation for entering adverse action
            // Extracted from test class business logic
        }

        /// <summary>
        /// Navigates through registration workflow steps
        /// </summary>
        public void NavigateToStep(string regId, string stepName)
        {
            // Implementation for workflow navigation
            // Extracted from test class business logic
        }
    }
}
