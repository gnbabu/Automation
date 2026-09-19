using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using SdetToolbox.Pages;
using Selenium.BaseComponents.Pages;
using Selenium.BaseComponents.Utilities;
using SeleniumExtensions.Configurations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BasePageHelper = Selenium.BaseComponents.Utilities.PageHelper;

namespace TC.PriorAuthoriztion.Pages
{
    public class SubmitPriorAuthorizationPage : BasePage
    {
        IWebDriver webDriver;
        public SubmitPriorAuthorizationPage(IWebDriver webDriver) : base(webDriver)
        {
            this.webDriver = webDriver;
            string url = this.webDriver.Url;
        }
        public IWebElement lnkBtnSubmitPriorAuth
        {
            get
            {
                return webDriver.CreateSmartElement(By.XPath("//input[@value='dental']")).Element;
            }
        }
        public void WaitUntilElementIsVisible()
        {
            BasePageHelper.WaitUntilElementIsVisible(webDriver, By.XPath("//*[@id='ctl00_MainContent_uc1SubmitPriorAuthorization_rblClaimType']"), TimeoutConfiguration.Element);
        }
        public IWebElement ddlAuthorization
        {
            get
            {
                // MINIMAL CHANGE: Use smart wrapper to prevent stale element issues
                return webDriver.CreateSmartElement(By.Id("ctl00_MainContent_uc1SubmitPriorAuthorization_ddlAuthorization")).Element;
            }
        }
        public IWebElement ddlSubCapitaPayerID
        {
            get
            {
                // MINIMAL CHANGE: Use smart wrapper to prevent stale element issues
                return webDriver.CreateSmartElement(By.Id("ctl00_MainContent_uc1SubmitPriorAuthorization_ddlSubCapitaPayerIDs")).Element;
            }
        }
        public IWebElement ddlAssignment
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id("ctl00_MainContent_uc1SubmitPriorAuthorization_ddlAssignment")).Element;
            }
        }
        public IWebElement ddlServiceType
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id("ctl00_MainContent_uc1SubmitPriorAuthorization_ddlServiceType")).Element;
            }
        }

        #region Recipient Information

        public IWebElement txtMedicaidBillingNumber
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id("ctl00_MainContent_uc1SubmitPriorAuthorization_txtMedicaidBillingNumber")).Element;
            }
        }
        public IWebElement txtBirthDate
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id("ctl00_MainContent_uc1SubmitPriorAuthorization_txtBirthDate")).Element;
            }
        }
        public IWebElement txtPatientTrckNum
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id("ctl00_MainContent_uc1SubmitPriorAuthorization_txtPatientTrckNum")).Element;
            }
        }

        public IWebElement divpnlRecipientLoader
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id("divpnlRecipientLoader")).Element;
            }
        }
        public void WaitUntilLoaderDisappear()
        {
            BasePageHelper.WaitUntilElementNotAvailable(webDriver, By.Id("divpnlRecipientLoader"), TimeoutConfiguration.Element);
        }

        #endregion

        #region Requestor Contact Information

        public IWebElement txtContactName
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id("ctl00_MainContent_uc1SubmitPriorAuthorization_txtContactName")).Element;
            }
        }
        public IWebElement txtContactLastName
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id("ctl00_MainContent_uc1SubmitPriorAuthorization_txtContactLastName")).Element;
            }
        }
        public IWebElement txtContactNumber
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id("ctl00_MainContent_uc1SubmitPriorAuthorization_txtContactNumber")).Element;
            }
        }
        public IWebElement txtContactExt
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id("ctl00_MainContent_uc1SubmitPriorAuthorization_txtExt")).Element;
            }
        }


        #endregion

        #region  SERVICE INFORMATION

        public IWebElement txtpalceofservice
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id("ctl00_MainContent_uc1SubmitPriorAuthorization_txtpalceofservice")).Element;
            }
        }
        public IWebElement txtAccDtService
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id("ctl00_MainContent_uc1SubmitPriorAuthorization_txtAccDtService")).Element;
            }
        }
        public IWebElement txtDateOfpatientEvent
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id("ctl00_MainContent_uc1SubmitPriorAuthorization_TextBox3")).Element;
            }
        }
        public IWebElement txtProfOnsetIllness
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id("ctl00_MainContent_uc1SubmitPriorAuthorization_txtProfOnsetIllness")).Element;
            }
        }
        public IWebElement txtMenDtInst
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id("ctl00_MainContent_uc1SubmitPriorAuthorization_txtMenDtInst")).Element;
            }
        }
        public IWebElement txtEstDOB
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id("ctl00_MainContent_uc1SubmitPriorAuthorization_txtEstDOB")).Element;
            }
        }
        public IWebElement txtPANumInst
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id("ctl00_MainContent_uc1SubmitPriorAuthorization_txtPANumInst")).Element;
            }
        }
        public IWebElement ddlLvlServiceInst
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id("ctl00_MainContent_uc1SubmitPriorAuthorization_ddlLvlServiceInst")).Element;
            }
        }
        public IWebElement ddlDelayedInst
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id("ctl00_MainContent_uc1SubmitPriorAuthorization_ddlDelayedInst")).Element;
            }
        }
        public IWebElement pnlPlaceofServiceSearchPopup
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id("ctl00_MainContent_uc1SubmitPriorAuthorization_pnlplaceofServiceSearch")).Element;
            }
        }

        public void SetDelayReason(string servicetype)
        {

            if (!string.IsNullOrEmpty(servicetype))
            {
                SelectElement assignedType = new SelectElement(ddlDelayedInst);
                assignedType.SelectByText(servicetype);
            }
        }
        public void SetListOfService(string serviceType)
        {

            if (!string.IsNullOrEmpty(serviceType))
            {
                SelectElement assignedType = new SelectElement(ddlLvlServiceInst);
                assignedType.SelectByText(serviceType);
            }
        }

        #endregion

        #region Service Provider Information
        public IWebElement txtSPNPI
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id("ctl00_MainContent_uc1SubmitPriorAuthorization_txtSPNPI")).Element;
            }
        }

        public void WaitUntilMedicaidIsFetched()
        {
            BasePageHelper.WaitUntilElementIsVisible(webDriver, By.Id("ctl00_MainContent_uc1SubmitPriorAuthorization_txtMedicaidID"), TimeoutConfiguration.Element);
        }

        public IWebElement pnlService
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id("ctl00_MainContent_uc1SubmitPriorAuthorization_pnlService")).Element;
            }
        }

        #endregion

        #region Ordering Provider Information
        public IWebElement lblseporderproviderinfo
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id("ctl00_MainContent_uc1SubmitPriorAuthorization_lblseporderproviderinfo")).Element;
            }
        }

        public IWebElement txtorderingprovidernpi
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id("ctl00_MainContent_uc1SubmitPriorAuthorization_txtorderingprovidernpi")).Element;
            }
        }
        public IWebElement pnlorderproviderinfo
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id("ctl00_MainContent_uc1SubmitPriorAuthorization_pnlorderproviderinfo")).Element;
            }
        }
        public void WaitUntilOrderingMedicaidIsFetched()
        {
            BasePageHelper.WaitUntilElementIsVisible(webDriver, By.Id("ctl00_MainContent_uc1SubmitPriorAuthorization_txtOMID"), TimeoutConfiguration.Element);
        }

        //

        #endregion

        #region Diagnosis Information

        public IWebElement btnDiagnosisAdd
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id("ctl00_MainContent_uc1SubmitPriorAuthorization_btnDiagnosisAdd")).Element;
            }
        }
        public IWebElement btnDiagnosisAddLine
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id("btnDiagnosisAddLine")).Element;
            }
        }
        public IWebElement ddlDiagnosisCodeType
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id("ctl00_MainContent_uc1SubmitPriorAuthorization_ddlDiagnosisCodeType")).Element;
            }
        }
        public IWebElement txtLnDiagnosisCode
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id("ctl00_MainContent_uc1SubmitPriorAuthorization_txtLnDiagnosisCode")).Element;
            }
        }
        public IWebElement txtDiagnosisDate
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id("ctl00_MainContent_uc1SubmitPriorAuthorization_txtDiagnosisDate")).Element;
            }
        }
        public IWebElement btnDiagnosisEditCancel
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id("btnDiagnosisEditCancel")).Element;
            }
        }

        public IWebElement DiagnosisExpanderSpan
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id("ctl00_MainContent_uc1SubmitPriorAuthorization_lblsepDiagnosis")).Element;
            }
        }

        public IWebElement txtDiagnosisCodeDescription
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id("ctl00_MainContent_uc1SubmitPriorAuthorization_txtDiagnosisCodeDescription")).Element;
            }
        }
        #endregion

        #region Service Details
        public IWebElement btnDentalServiceDetailAdd1
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id("ctl00_MainContent_uc1SubmitPriorAuthorization_btnDentalServiceDetailAdd1")).Element;
            }
        }
        public IWebElement txtDentalSDProcCode
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id("ctl00_MainContent_uc1SubmitPriorAuthorization_txtDentalSDProcCode")).Element;
            }
        }
        public IWebElement txtDentalProcCodeDescription
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id("ctl00_MainContent_uc1SubmitPriorAuthorization_txtDentalProcCodeDescription")).Element;
            }
        }
        public IWebElement txtDentalReqUnits
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id("ctl00_MainContent_uc1SubmitPriorAuthorization_txtDentalReqUnits")).Element;
            }
        }
        public IWebElement txtDentalReqDollars
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id("ctl00_MainContent_uc1SubmitPriorAuthorization_txtDentalReqDollars")).Element;
            }
        }
        public IWebElement txtDentalReqFDOS
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id("ctl00_MainContent_uc1SubmitPriorAuthorization_txtDentalReqFDOS")).Element;
            }
        }
        public IWebElement txtDentalReqTDOS
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id("ctl00_MainContent_uc1SubmitPriorAuthorization_txtDentalReqTDOS")).Element;
            }
        }
        public IWebElement txtDentalServTrackingNo
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id("ctl00_MainContent_uc1SubmitPriorAuthorization_txtDentalServTrackingNo")).Element;
            }
        }
        public IWebElement txtDentalProvServnote
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id("ctl00_MainContent_uc1SubmitPriorAuthorization_txtDentalProvServnote")).Element;
            }
        }
        public IWebElement ddDentalToothNumber
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id("ctl00_MainContent_uc1SubmitPriorAuthorization_ddDentalToothNumber")).Element;
            }
        }
        public IWebElement ddDentalOralCavity1
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id("ctl00_MainContent_uc1SubmitPriorAuthorization_ddDentalOralCavity1")).Element;
            }
        }
        public IWebElement ddToothSurface1
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id("ctl00_MainContent_uc1SubmitPriorAuthorization_ddToothSurface1")).Element;
            }
        }
        public IWebElement ddDentalProsthsis
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id("ctl00_MainContent_uc1SubmitPriorAuthorization_ddDentalProsthsis")).Element;
            }
        }
        public IWebElement btnServDentalAddUpdate
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id("btnServDentalUpdate")).Element;
            }
        }
        public void SetDentalToothNumber(string text)
        {
            if (!string.IsNullOrEmpty(text))
            {
                SelectElement assignedType = new SelectElement(ddDentalToothNumber);
                assignedType.SelectByText(text);
            }
        }
        public void SetDentalOralCavity1(string text)
        {
            if (!string.IsNullOrEmpty(text))
            {
                SelectElement assignedType = new SelectElement(ddDentalOralCavity1);
                assignedType.SelectByText(text);
            }
        }
        public void SetDentalToothSurface1(string text)
        {
            if (!string.IsNullOrEmpty(text))
            {
                SelectElement assignedType = new SelectElement(ddToothSurface1);
                assignedType.SelectByText(text);
            }
        }
        public void SetDentalProsthsis(string text)
        {
            if (!string.IsNullOrEmpty(text))
            {
                SelectElement assignedType = new SelectElement(ddDentalProsthsis);
                assignedType.SelectByText(text);
            }
        }
        public void WaituntilProcedureCodeVisible()
        {
            BasePageHelper.WaitUntilElementIsVisible(webDriver, By.XPath("//*[@id='ctl00_MainContent_uc1SubmitPriorAuthorization_txtDentalSDProcCode']"), TimeoutConfiguration.Element);
        }

        #endregion

        #region Provider Notes

        public IWebElement txtProviderNotes
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id("ctl00_MainContent_uc1SubmitPriorAuthorization_txtProviderNotes")).Element;
            }
        }
        public IWebElement btnprovNoteSave
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id("btnprovNoteSave")).Element;
            }
        }
        public IWebElement btnprovNoteEdit
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id("btnprovNoteEdit")).Element;
            }
        }
        public IWebElement btnprovNoteDelete
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id("btnprovNoteDelete")).Element;
            }
        }

        public IWebElement lblsepProvidersNotes
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id("ctl00_MainContent_uc1SubmitPriorAuthorization_lblsepProvidersNotes")).Element;
            }
        }


        #endregion

        #region Attachments

        public IWebElement priorDentalAttachmentUpload
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id("ctl00_MainContent_uc1SubmitPriorAuthorization_priorDentalAttachmentUpload")).Element;
            }
        }
        public IWebElement ddlPriorDentalAuthDocType
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id("ctl00_MainContent_uc1SubmitPriorAuthorization_ddlPriorDentalAuthDocType")).Element;
            }
        }
        public IWebElement txtPriorDentalAttachmentNote
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id("ctl00_MainContent_uc1SubmitPriorAuthorization_txtPriorDentalAttachmentNote")).Element;
            }
        }
        public IWebElement btnAddDentalAttachment
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id("ctl00_MainContent_uc1SubmitPriorAuthorization_btnAddDentalAttachment")).Element;
            }
        }
        public IWebElement btnAddDentalAttachmentsGrid
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id("ctl00_MainContent_uc1SubmitPriorAuthorization_gvDentalAttachment")).Element;
            }
        }

        public IWebElement AttachmentExpanderSpan
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id("ctl00_MainContent_uc1SubmitPriorAuthorization_lblsepDentalAttachment")).Element;
            }
        }

        public IWebElement btnSubmit
        {
            get
            {
                // MINIMAL CHANGE: Just add .Element to use smart wrapper
                // This automatically handles stale elements without changing the property structure
                return webDriver.CreateSmartElement(By.Id("ctl00_MainContent_uc1SubmitPriorAuthorization_btnSubmit")).Element;
            }
        }
        public IWebElement btnSave
        {
            get
            {
                // MINIMAL CHANGE: Just add .Element to use smart wrapper
                return webDriver.CreateSmartElement(By.Id("ctl00_MainContent_uc1SubmitPriorAuthorization_btnSave")).Element;
            }
        }
        public IWebElement btnClearAll
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id("ctl00_MainContent_uc1SubmitPriorAuthorization_btnClearAll")).Element;
            }
        }

        public IWebElement pnmPATXNFailure
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id("ctl00_MainContent_uc1SubmitPriorAuthorization_pnmPATXNFailure")).Element;
            }
        }
        public IWebElement pnmPATXNSuccess
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id("ctl00_MainContent_uc1SubmitPriorAuthorization_pnmPATXNSuccess")).Element;
            }
        }


        //ctl00_MainContent_uc1SubmitPriorAuthorization_pnmPATXNFailure
        //ctl00_MainContent_uc1SubmitPriorAuthorization_pnmPATXNSuccess

        #endregion




        public void SetDocumentType(string documentType)
        {
            if (!string.IsNullOrEmpty(documentType))
            {
                SelectElement assignedType = new SelectElement(ddlPriorDentalAuthDocType);
                assignedType.SelectByText(documentType);
            }
        }
        public void SetAuthorization(string authType)
        {
            if (!string.IsNullOrEmpty(authType))
            {
                SelectElement assignedType = new SelectElement(ddlAuthorization);
                assignedType.SelectByText(authType);
            }
        }
        public void SetAssignment(string assignment)
        {
            string assignmentType = "Dental";

            if (!string.IsNullOrEmpty(assignment))
            {
                SelectElement assignedType = new SelectElement(ddlAssignment);
                assignedType.SelectByText(assignment);
            }
        }
        public void SetServiceType(string serviceType)
        {
            if (!string.IsNullOrEmpty(serviceType))
            {
                SelectElement assignedType = new SelectElement(ddlServiceType);
                assignedType.SelectByText(serviceType);
            }
        }
        public bool VerifyDropdownHasValues(IWebElement drodownElement)
        {
            SelectElement assignedType = new SelectElement(ddlAuthorization);
            IList<IWebElement> options = assignedType.Options;
            return options != null && options.Count > 0 ? true : false;
        }
    }
}
