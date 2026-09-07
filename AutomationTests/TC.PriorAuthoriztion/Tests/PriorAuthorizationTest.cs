using NUnit.Framework;
using OpenQA.Selenium;
using SdetToolbox.Pages;
using Selenium.BaseComponents;
using Selenium.BaseComponents.Pages;
using Selenium.BaseComponents.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SeleniumExtensions.Extensions;
using OpenQA.Selenium.Support.UI;
using SeleniumExtensions.Configurations;
using static System.Net.Mime.MediaTypeNames;
using System.Xml.Linq;
using System.Diagnostics;
using TC.PriorAuthoriztion.Utilities;
using TC.PriorAuthoriztion.Models;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using TC.PriorAuthoriztion.Pages;
using TC.PriorAuthoriztion.Services;
using BasePageHelper = SdetToolbox.Pages.PageHelper;

namespace TC.PriorAuthoriztion.Tests
{
    [TestFixture("TechAdmin")]
    public class PriorAuthorizationTest : BaseFeatureFixture
    {
        private PriorAuthorizationService _priorAuthService;
        private TestCaseExecutionLog _testCaseExecutionLog;
        private Screeshots? _screenshots;

        public PriorAuthorizationTest(string profile) : base(profile)
        {
        }

        [SetUp]
        public new void BeforeEachTest()
        {
            _priorAuthService = new PriorAuthorizationService(TestWebDriver);
        }

        // Example of using injected APIGateway from base class
        protected APIGatway GetAPIGateway()
        {
            return APIGateway;
        }

        // Shared by both [Test] methods and FillDentalPAFields - logs+screenshots one
        // step, matching the pattern already proven in TC.PriorAuthSearch/TC.SearchRA/
        // TC.SearchEligibility, but factored into one helper since this project has two
        // separate test methods (DentalPA_Submit/DentalPA_Save) plus a large shared
        // multi-section form-filling helper, all needing to log against the same
        // _testCaseExecutionLog/_screenshots instance.
        private void LogStep(string stepName, string message)
        {
            _testCaseExecutionLog.StepName = stepName;
            _testCaseExecutionLog.LogMessage = message;
            SaveLog(_testCaseExecutionLog);
            _screenshots?.screenShot.Add(Common.PrintScreenShot(TestWebDriver, stepName));
        }

        [Test]
        [Author("Vishnuvardhan Reddy")]
        [Category("IntegrationTests")]
        [System.ComponentModel.Description("Verify new provider creation flow")]
        [Property("Description", "Submit Dental Prior Authorization")]
        [Property("Priority", "High")]
        [Property("TestCaseId", "TCDentalPASubmit")]
        //[CustomRetry(5)]
        [CancelAfter(600000)]
        public void DentalPA_Submit()
        {
            _testCaseExecutionLog = new TestCaseExecutionLog
            {
                AssignmentId = AssignmentId,
                AssignmentTestCaseId = AssignmentTestCaseId,
                LogLevel = TestCaseLogLevel.Info,
                ExecutionStatus = ExecutionStatus.Running,
                TestCaseId = "TCDentalPASubmit",
                TestCaseDescription = "Submit Dental Prior Authorization"
            };
            _screenshots = new Screeshots { AssignmentTestCaseId = AssignmentTestCaseId, screenShot = new List<byte[]>() };

            LogStep("Login to PNM", "Login to PNM succesfully...!");

            DentalPA dentalPA = (DentalPA)DataRepository.GetAutomationData("DentalPA");

            SidebarMenu.Click();
            NavigateToSelfService();

            LogStep("Self Service", "Navigating to self service...!");

            FinancialProviderInformationPage financialProviderInformationPage = new FinancialProviderInformationPage(TestWebDriver);
            financialProviderInformationPage.WaitUntilElementIsVisible();
            financialProviderInformationPage.TxtMedicaid.Set(dentalPA.DentalInformation.RegID);
            financialProviderInformationPage.lnkBtnPriorAuth.Click();

            LogStep("Medicaid Search", $"Entered {dentalPA.DentalInformation.RegID} for search");

            SearchPriorAuthorizationPage searchPriorAuthorizationPage = new SearchPriorAuthorizationPage(TestWebDriver);
            searchPriorAuthorizationPage.WaitUntilElementIsVisible();
            searchPriorAuthorizationPage.lnkBtnSubmitPriorAuth.Click();

            LogStep("Submit Prior Auth", "Navigated to Submit Prior Authorization page");

            IJavaScriptExecutor jsExecutor;

            FillDentalPAFields(dentalPA);

            SubmitPriorAuthorizationPage submitPriorAuthorizationPage = new SubmitPriorAuthorizationPage(TestWebDriver);
            jsExecutor = (IJavaScriptExecutor)TestWebDriver;
            jsExecutor.ExecuteScript("arguments[0].click();", submitPriorAuthorizationPage.btnSubmit);

            BasePageHelper.WaitUntilDocumentIsReady(TestWebDriver, TimeoutConfiguration.Element);
            Thread.Sleep(3000);

            //submitPriorAuthorizationPage.btnSubmit.Click();

            BasePageHelper.WaitUntilDocumentIsReady(TestWebDriver, TimeoutConfiguration.Element);

            IWebElement messagrWarning = TestWebDriver.CreateSmartElement(By.Id("ctl00_MainContent_uc1SubmitPriorAuthorization_pnlWarningAcknowledgment")).Element;

            if (messagrWarning.Displayed)
            {
                TestWebDriver.CreateSmartElement(By.Id("ctl00_MainContent_uc1SubmitPriorAuthorization_btnWarningAcknowledgmentYes")).Element.Click();
            }

            BasePageHelper.WaitUntilElementNotVisible(TestWebDriver, By.Id("ctl00_MainContent_uc1SubmitPriorAuthorization_pnlWarningAcknowledgment"), TimeoutConfiguration.Element);

            jsExecutor = (IJavaScriptExecutor)TestWebDriver;
            jsExecutor.ExecuteScript("arguments[0].click();", submitPriorAuthorizationPage.btnSubmit);

            messagrWarning = TestWebDriver.CreateSmartElement(By.Id("ctl00_MainContent_uc1SubmitPriorAuthorization_pnlWarningAcknowledgment")).Element;
            if (messagrWarning.Displayed)
            {
                jsExecutor = (IJavaScriptExecutor)TestWebDriver;
                jsExecutor.ExecuteScript("arguments[0].click();", TestWebDriver.CreateSmartElement(By.Id("ctl00_MainContent_uc1SubmitPriorAuthorization_btnWarningAcknowledgmentYes")).Element);
            }
            Thread.Sleep(1000);
            OpenQA.Selenium.IWebElement elemtToClick = TestWebDriver.FindElement(By.XPath("//*[@id='ctl00_MainContent_uc1SubmitPriorAuthorization_btnSubmit']"));
            Thread.Sleep(1000);
            var actions = new OpenQA.Selenium.Interactions.Actions(TestWebDriver);
            Thread.Sleep(1000);
            actions = actions.MoveToElement(elemtToClick);
            Thread.Sleep(1000);
            actions = actions.Click();
            Thread.Sleep(1000);
            actions.Perform();

            bool trnxSuccess = submitPriorAuthorizationPage.pnmPATXNSuccess.Displayed;
            bool tXNFailure = submitPriorAuthorizationPage.pnmPATXNFailure.Displayed;

            if (trnxSuccess)
            {
                LogStep("Test case executed successfully...!", "Success");
            }
            else if (tXNFailure)
            {
                string failureMessage = TestWebDriver.CreateSmartElement(By.XPath("//*[@id='ctl00_MainContent_uc1SubmitPriorAuthorization_grdTXNResponse']/tbody/tr/td[2]")).Element.Text;
                TestWebDriver.CreateSmartElement(By.Id("ctl00_MainContent_uc1SubmitPriorAuthorization_btnCloseFailure")).Element.Click();
                LogStep("Submission failed", failureMessage);
               NUnit.Framework.Assert.Fail(failureMessage);
            }
        }

        [Test]
        [Author("Vishnuvardhan Reddy")]
        [Category("IntegrationTests")]
        [System.ComponentModel.Description("Verify new provider creation flow")]
        [Property("Description", "Save Dental Prior Authorization")]
        [Property("Priority", "High")]
        [Property("TestCaseId", "TCDentalPASave")]
        //[CustomRetry(5)]
        [CancelAfter(600000)]
        public void DentalPA_Save()
        {
            _testCaseExecutionLog = new TestCaseExecutionLog
            {
                AssignmentId = AssignmentId,
                AssignmentTestCaseId = AssignmentTestCaseId,
                LogLevel = TestCaseLogLevel.Info,
                ExecutionStatus = ExecutionStatus.Running,
                TestCaseId = "TCDentalPASave",
                TestCaseDescription = "Save Dental Prior Authorization"
            };
            _screenshots = new Screeshots { AssignmentTestCaseId = AssignmentTestCaseId, screenShot = new List<byte[]>() };

            LogStep("Login to PNM", "Login to PNM succesfully...!");

             DentalPA dentalPA = (DentalPA)DataRepository.GetAutomationData("DentalPA");

            SidebarMenu.Click();
            NavigateToSelfService();

            LogStep("Self Service", "Navigating to self service...!");

            FinancialProviderInformationPage financialProviderInformationPage = new FinancialProviderInformationPage(TestWebDriver);
            financialProviderInformationPage.WaitUntilElementIsVisible();
            financialProviderInformationPage.TxtMedicaid.Set(dentalPA.DentalInformation.RegID);
            financialProviderInformationPage.lnkBtnPriorAuth.Click();

            SearchPriorAuthorizationPage searchPriorAuthorizationPage = new SearchPriorAuthorizationPage(TestWebDriver);
            searchPriorAuthorizationPage.WaitUntilElementIsVisible();
            searchPriorAuthorizationPage.lnkBtnSubmitPriorAuth.Click();

            LogStep("Submit Prior Auth", "Navigated to Submit Prior Authorization page");

            IJavaScriptExecutor jsExecutor;

            FillDentalPAFields(dentalPA);

            SubmitPriorAuthorizationPage submitPriorAuthorizationPage = new SubmitPriorAuthorizationPage(TestWebDriver);
            jsExecutor = (IJavaScriptExecutor)TestWebDriver;
            jsExecutor.ExecuteScript("arguments[0].click();", submitPriorAuthorizationPage.btnSave);


            Thread.Sleep(3000);

            IAlert alert = TestWebDriver.SwitchTo().Alert();

            string textMessage = alert.Text;

            if (!textMessage.Equals("Your unsubmitted prior authorization will be saved in the system for 72 hours."))
            {
                LogStep("Save failed", "Something went wrong while saving the Dental PA");
                NUnit.Framework.Assert.Fail("Something went wrong while saving the Dental PA");
            }

            alert.Accept();

            BasePageHelper.WaitUntilDocumentIsReady(TestWebDriver, TimeoutConfiguration.Element);
            BasePageHelper.WaitUntilElementIsVisible(TestWebDriver, By.Id("ctl00_MainContent_uc1SubmitPriorAuthorization_lblpriorautherror"), TimeoutConfiguration.Element);

            IWebElement messagrWarning = TestWebDriver.CreateSmartElement(By.Id("ctl00_MainContent_uc1SubmitPriorAuthorization_lblpriorautherror")).Element;

            if (messagrWarning.Displayed && messagrWarning.Text.Equals("PA request has been saved."))
            {
                NUnit.Framework.TestContext.WriteLine($"PA request has been saved with Patient Tracking Number::{dentalPA.DentalRecipientInformation.PatientTrackingNumber}");
                LogStep("Test case executed successfully...!", $"PA request has been saved with Patient Tracking Number::{dentalPA.DentalRecipientInformation.PatientTrackingNumber}");
            }
        }

        private void FillDentalPAFields(DentalPA dentalPA)
        {
            IJavaScriptExecutor jsExecutor;
            SubmitPriorAuthorizationPage submitPriorAuthorizationPage;
            submitPriorAuthorizationPage = new SubmitPriorAuthorizationPage(TestWebDriver);
            submitPriorAuthorizationPage.WaitUntilElementIsVisible();
            submitPriorAuthorizationPage.lnkBtnSubmitPriorAuth.Click();

            submitPriorAuthorizationPage.SetAuthorization(dentalPA.DentalInformation.DestinationPayerName);

            while (!submitPriorAuthorizationPage.VerifyDropdownHasValues(submitPriorAuthorizationPage.ddlSubCapitaPayerID))
            {
                Thread.Sleep(2000);
            }

            submitPriorAuthorizationPage.SetAssignment(dentalPA.DentalInformation.Assignment);
            submitPriorAuthorizationPage.SetServiceType(dentalPA.DentalInformation.ServiceType);

            #region Recipient Information

            submitPriorAuthorizationPage.txtMedicaidBillingNumber.Set(dentalPA.DentalRecipientInformation.MedicaidBillingNumber);
            submitPriorAuthorizationPage.txtBirthDate.Set(dentalPA.DentalRecipientInformation.DateOfBirth);
            submitPriorAuthorizationPage.txtPatientTrckNum.Click();

            IWebElement firstnameElement = TestWebDriver.CreateSmartElement(By.Id("ctl00_MainContent_uc1SubmitPriorAuthorization_txtfrstmi2")).Element;

            string valuee = firstnameElement.GetAttribute("value");

            bool isLoaded = !string.IsNullOrEmpty(valuee) ? true : false;

            while (!isLoaded)
            {
                Thread.Sleep(2000);
                firstnameElement = TestWebDriver.CreateSmartElement(By.Id("ctl00_MainContent_uc1SubmitPriorAuthorization_txtfrstmi2")).Element;
                valuee = firstnameElement.GetAttribute("value");
                isLoaded = !string.IsNullOrEmpty(valuee) ? true : false;
            }

            //ctl00_MainContent_uc1SubmitPriorAuthorization_txtfrstmi2

            submitPriorAuthorizationPage.txtPatientTrckNum.Set(dentalPA.DentalRecipientInformation.PatientTrackingNumber);

            #endregion

            LogStep("Recipient Information", "Recipient Information filled");

            #region Requestor Contact Information

            submitPriorAuthorizationPage.txtContactName.Set(dentalPA.DentalContactInformation.ContactFirstName);
            submitPriorAuthorizationPage.txtContactLastName.Set(dentalPA.DentalContactInformation.ContactLastName);
            submitPriorAuthorizationPage.txtContactNumber.Click();

            Thread.Sleep(2000);

            jsExecutor = (IJavaScriptExecutor)TestWebDriver;
            jsExecutor.ExecuteScript($"arguments[0].value = '{dentalPA.DentalContactInformation.ContactNumber}'", submitPriorAuthorizationPage.txtContactNumber);


            submitPriorAuthorizationPage.txtContactExt.Set(dentalPA.DentalContactInformation.ContactExtension);


            #endregion

            LogStep("Requestor Contact Information", "Requestor Contact Information filled");

            #region  SERVICE INFORMATION

            submitPriorAuthorizationPage.txtpalceofservice.Set(dentalPA.DentalServiceInformation.PlaceOfService);
            submitPriorAuthorizationPage.SetDelayReason(dentalPA.DentalServiceInformation.DelayReason);
            submitPriorAuthorizationPage.SetListOfService(dentalPA.DentalServiceInformation.LevelOfService);
            submitPriorAuthorizationPage.txtAccDtService.Set(dentalPA.DentalServiceInformation.AccidentDate);


            #endregion

            LogStep("Service Information", "Service Information filled");

            #region Service Provider Information

            submitPriorAuthorizationPage.txtSPNPI.Set(dentalPA.DentalServiceProviderInformation.ServiceProviderNPI);
            submitPriorAuthorizationPage.pnlService.Click();
            submitPriorAuthorizationPage.WaitUntilMedicaidIsFetched();

            #endregion

            LogStep("Service Provider Information", "Service Provider Information filled");

            #region Ordering Provider Information

            string orderingProvExp = submitPriorAuthorizationPage.lblseporderproviderinfo.Text;
            if (orderingProvExp.Equals("+"))
                TestWebDriver.CreateSmartElement(By.Id("ctl00_MainContent_uc1SubmitPriorAuthorization_pnlseporderproviderinfo")).Element.Click();

            submitPriorAuthorizationPage.txtorderingprovidernpi.Set(dentalPA.DentalOrderingProviderInformation.OrderingProviderNPI);

            IWebElement medicaidElement = TestWebDriver.FindElement(By.XPath("//div[@id='ctl00_MainContent_uc1SubmitPriorAuthorization_pnlorderproviderinfo']//td[text()='Medicaid ID']"));

            if (medicaidElement != null)
                medicaidElement.Click();

            submitPriorAuthorizationPage.WaitUntilOrderingMedicaidIsFetched();


            #endregion

            LogStep("Ordering Provider Information", "Ordering Provider Information filled");

            #region Diagnosis Information
            try
            {
                string diagnosisExp = submitPriorAuthorizationPage.DiagnosisExpanderSpan.Text;
                if (diagnosisExp.Equals("+"))
                    TestWebDriver.CreateSmartElement(By.Id("ctl00_MainContent_uc1SubmitPriorAuthorization_pnlsepDiagnosis")).Element.Click();

                Thread.Sleep(2000);

                submitPriorAuthorizationPage.btnDiagnosisAdd.Click();
            }
            catch (Exception ex)
            {
                submitPriorAuthorizationPage.btnDiagnosisAdd.Click();
            }

            BasePageHelper.WaitUntilElementIsVisible(TestWebDriver, By.Id("ctl00_MainContent_uc1SubmitPriorAuthorization_txtLnDiagnosisCode"), TimeoutConfiguration.Element);
            submitPriorAuthorizationPage.txtLnDiagnosisCode.Set(dentalPA.DentalDiagnosisInformation.DiagnosisCode);
            TestWebDriver.CreateSmartElement(By.Id("ctl00_MainContent_uc1SubmitPriorAuthorization_txtDiagnosisCodeDescription")).Element.Click();

            Thread.Sleep(3000);

            DiagnosisPopupSearch(dentalPA.DentalDiagnosisInformation.DiagnosisCode);

            submitPriorAuthorizationPage.txtDiagnosisDate.Set(dentalPA.DentalDiagnosisInformation.DiagnosisDate);
            submitPriorAuthorizationPage.txtDiagnosisCodeDescription.Click();
            Thread.Sleep(2000);
            submitPriorAuthorizationPage.btnDiagnosisAddLine.Click();
            Thread.Sleep(5000);

            #endregion

            LogStep("Diagnosis Information", "Diagnosis Information filled");

            #region Service Details

            submitPriorAuthorizationPage.btnDentalServiceDetailAdd1.Click();
            submitPriorAuthorizationPage.txtDentalSDProcCode.Set(dentalPA.DentalServiceDetails.ProcedureCode);

            submitPriorAuthorizationPage.txtDentalProcCodeDescription.Click();

            Thread.Sleep(3000);

            IWebElement lblDentalProcMessage = TestWebDriver.CreateSmartElement(By.Id("ctl00_MainContent_uc1SubmitPriorAuthorization_txtDiagnosisCodeDescription")).Element;

            if (lblDentalProcMessage.Displayed && lblDentalProcMessage.Text.Equals("Procedure code is invalid"))
            {
                TestWebDriver.CreateSmartElement(By.Id("ctl00_MainContent_uc1SubmitPriorAuthorization_lnkDentalSDProcCodeSearchLink")).Element.Click();
                BasePageHelper.WaitUntilElementIsVisible(TestWebDriver, By.Id("ctl00_MainContent_uc1SubmitPriorAuthorization_pnlSubmitPriorAuthSearchProcPop"), TimeoutConfiguration.Element);

                TestWebDriver.CreateSmartElement(By.Id("ctl00_MainContent_uc1SubmitPriorAuthorization_txtCode")).Element.Set(dentalPA.DentalServiceDetails.ProcedureCode);
                TestWebDriver.CreateSmartElement(By.Id("ctl00_MainContent_uc1SubmitPriorAuthorization_Button9")).Element.Click();

                Thread.Sleep(5000);

                IWebElement procedureCodeOutput = TestWebDriver.CreateSmartElement(By.XPath("//*[@id='procedureCodeOutput']/div")).Element;

                if (procedureCodeOutput.Displayed)
                {
                    if (!procedureCodeOutput.Text.Equals("No data found."))
                        TestWebDriver.CreateSmartElement(By.XPath("//*[@id='tableData']/tbody/tr[2]/td[1]/a")).Element.Click();
                    else
                        NUnit.Framework.Assert.Fail("no procedure code data found with the given code.");
                }
            }

            submitPriorAuthorizationPage.txtDentalProcCodeDescription.Set(dentalPA.DentalServiceDetails.ProcedureCodeDescription);



            submitPriorAuthorizationPage.SetDentalToothNumber(dentalPA.DentalServiceDetails.ToothNumber);
            submitPriorAuthorizationPage.SetDentalOralCavity1(dentalPA.DentalServiceDetails.OralCavity);
            submitPriorAuthorizationPage.SetDentalToothSurface1(dentalPA.DentalServiceDetails.ToothSurface);
            submitPriorAuthorizationPage.txtDentalProvServnote.Set(dentalPA.DentalServiceDetails.ProviderServiceNote);
            submitPriorAuthorizationPage.txtDentalReqUnits.Set(dentalPA.DentalServiceDetails.RequestedUnits);

            submitPriorAuthorizationPage.txtDentalReqDollars.Set(dentalPA.DentalServiceDetails.RequestedDollars);
            submitPriorAuthorizationPage.txtDentalReqFDOS.Set(dentalPA.DentalServiceDetails.RequestedFDOS);
            submitPriorAuthorizationPage.txtDentalReqTDOS.Set(dentalPA.DentalServiceDetails.RequestedTDOS);
            Random random = new Random();
            int randomNumber = random.Next(1, 9999);
            submitPriorAuthorizationPage.txtDentalServTrackingNo.Set($"AUTH{randomNumber}");
            submitPriorAuthorizationPage.btnServDentalAddUpdate.Click();

            Thread.Sleep(5000);

            #endregion

            LogStep("Service Details", "Service Details filled");

            #region Provider Notes

            string providerNotesExp = submitPriorAuthorizationPage.lblsepProvidersNotes.Text;
            if (providerNotesExp.Equals("+"))
                submitPriorAuthorizationPage.lblsepProvidersNotes.Click();

            submitPriorAuthorizationPage.txtProviderNotes.Set(dentalPA.DentalProviderNotes.ProviderNotes);

            TestWebDriver.FindElement(By.Id("ctl00_MainContent_uc1SubmitPriorAuthorization_Div17")).Click();

            BasePageHelper.WaitUntilElementIsVisible(TestWebDriver, By.Id("btnprovNoteSave"), TimeoutConfiguration.Element);

            submitPriorAuthorizationPage.btnprovNoteSave.Click();

            BasePageHelper.WaitUntilElementIsVisible(TestWebDriver, By.Id("btnprovNoteEdit"), TimeoutConfiguration.Element);

            #endregion

            LogStep("Provider Notes", "Provider Notes filled");

            #region Attachments


            string attachmentExp = submitPriorAuthorizationPage.AttachmentExpanderSpan.Text;
            if (attachmentExp.Equals("+"))
                TestWebDriver.CreateSmartElement(By.Id("ctl00_MainContent_uc1SubmitPriorAuthorization_pnlSepDentalAttachment")).Element.Click();

            Thread.Sleep(2000);


            string filePath = dentalPA.DentalAttachments.FileName;

            submitPriorAuthorizationPage.priorDentalAttachmentUpload.SendKeys(filePath);

            Thread.Sleep(3000);

            submitPriorAuthorizationPage.SetDocumentType(dentalPA.DentalAttachments.DocumentType);

            submitPriorAuthorizationPage.txtPriorDentalAttachmentNote.Set(dentalPA.DentalAttachments.AttachmentNotes);

            submitPriorAuthorizationPage.btnAddDentalAttachment.Click();

            BasePageHelper.WaitUntilDocumentIsReady(TestWebDriver, TimeoutConfiguration.Element);

            BasePageHelper.WaitUntilElementIsVisible(TestWebDriver, By.Id("ctl00_MainContent_uc1SubmitPriorAuthorization_gvDentalAttachment"), TimeoutConfiguration.Element);

            BasePageHelper.WaitUntilDocumentIsReady(TestWebDriver, TimeoutConfiguration.Element);

            #endregion

            LogStep("Attachments", "Attachments filled");
        }

        [TearDown]
        public void AfterTest()
        {
            if (_screenshots?.screenShot == null || !_screenshots.screenShot.Any())
                return;

            if (_screenshots.AssignmentTestCaseId == 0)
                return;

            var screenshots = _screenshots.screenShot
                .Select((screen, index) => new TestScreenshot
                {
                    ID = index + 1,
                    AssignmentTestCaseId = Convert.ToInt32(_screenshots.AssignmentTestCaseId),
                    Caption = $"Screenshot_{index + 1}",
                    Screenshot = $"data:image/png;base64,{Convert.ToBase64String(screen)}",
                    TakenAt = DateTime.Now,
                })
                .ToList();

            APIGateway.SaveMethodScreenShots(screenshots);
        }

        public void SaveLog(TestCaseExecutionLog testCaseExecutionLog)
        {
            APIGateway.SaveTestCaseLog(testCaseExecutionLog);
        }

        private void DiagnosisPopupSearch(string diagnosisCode)
        {
            _priorAuthService.DiagnosisPopupSearch(diagnosisCode);
        }

        public IWebElement SidebarMenu
        {
            get
            {
                return TestWebDriver.CreateSmartElement(By.XPath("//button[contains(@class,'hamburger is-closed')]")).Element;
            }
        }
        public void NavigateToSelfService()
        {
            if (SelfService != null)
            {
                SelfService.Click();
            }
        }
        public IWebElement SelfService
        {
            get
            {
                // Was `//a[@title='Self Service']` - confirmed by direct testing (real
                // NoSuchElementException) this doesn't match the real page; matches
                // TC.PriorAuthSearch's already-working selector for the same element.
                return TestWebDriver.CreateSmartElement(By.XPath($"//a[normalize-space()='Self Service']")).Element;

            }
        }
    }
}

