using OpenQA.Selenium.Support.UI;
using Selenium.BaseComponents.Pages;
using SeleniumExtensions.Extensions;
using Selenium.BaseComponents.Utilities;
using OpenQA.Selenium;

namespace TC.ProviderDataEntry.Pages.Registration
{
    public class GroupFacilityHospitalAffiliations : BasePage
    {
        IWebDriver webDriver;
        string RegID;
        public GroupFacilityHospitalAffiliations(IWebDriver webDriver, string RegID) : base(webDriver)
        {

            this.webDriver = webDriver;

            string url = this.webDriver.Url;

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
        #region Group Facility & Hospality Affiliations
        public IWebElement? RegistrationTitle
        {
            get
            {
                return Selenium.BaseComponents.Utilities.PageHelper.IsElementExist(webDriver, By.Id("ctl00_MainContent_ucRegistrationNavigation_lblTitle")) ?
                    webDriver.CreateSmartElement(By.Id("ctl00_MainContent_ucRegistrationNavigation_lblTitle")).Element : null;
            }
        }
        public IWebElement AddGroupAffiliations
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id($"ctl00_MainContent_ucGroupAndFacility_{RegID}_btnAdd")).Element;
            }
        }
        public IWebElement GroupAffiliations_MedicaidID
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id($"ctl00_MainContent_ucGroupAndFacility_{RegID}_ucPendingGroupAffiliations_txtMedicaidID")).Element;
            }
        }
        public IWebElement GroupAffiliations_NPI
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id($"ctl00_MainContent_ucGroupAndFacility_{RegID}_ucPendingGroupAffiliations_txtNPI")).Element;
            }
        }
        public IWebElement GroupAffiliations_Save
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id($"ctl00_MainContent_ucGroupAndFacility_{RegID}_ucPendingGroupAffiliations_btnSave")).Element;
            }
        }
        public IWebElement GroupAffiliations_Cancel
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id($"ctl00_MainContent_ucGroupAndFacility_{RegID}_ucPendingGroupAffiliations_btnCancel")).Element;
            }
        }
        public IWebElement AddHospitalAffiliations
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id($"ctl00_MainContent_ucGroupAndFacility_{RegID}_ImageButton1")).Element;
            }
        }
        public IWebElement PatientSetting_No
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id($"ctl00_MainContent_ucGroupAndFacility_{RegID}_ucHealthCareAffiliations_rblInpatientSetting_1")).Element;
            }
        }
        public IWebElement PatientSetting_Yes
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id($"ctl00_MainContent_ucGroupAndFacility_{RegID}_ucHealthCareAffiliations_rblInpatientSetting_0")).Element;
            }
        }
        public IWebElement HospitalPrivileges_No
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id($"ctl00_MainContent_ucGroupAndFacility_{RegID}_ucHealthCareAffiliations_rblHospitalPrivileges_1")).Element;
            }
        }
        public IWebElement HospitalPrivileges_Yes
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id($"ctl00_MainContent_ucGroupAndFacility_{RegID}_ucHealthCareAffiliations_rblHospitalPrivileges_0")).Element;
            }
        }
        public IWebElement HospitalPrivileges_Reason
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id($"ctl00_MainContent_ucGroupAndFacility_{RegID}_ucHealthCareAffiliations_txtHospitalPrivilegesReason")).Element;
            }
        }
        public IWebElement PrimaryFacility
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id($"ctl00_MainContent_ucGroupAndFacility_{RegID}_ucHealthCareAffiliations_chkIsPrimaryFacility")).Element;
            }
        }
        public IWebElement OhioMedicaidID
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id($"ctl00_MainContent_ucGroupAndFacility_{RegID}_ucHealthCareAffiliations_txtMedicaidIDSearch")).Element;
            }
        }
        public IWebElement FacilityName
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id($"ctl00_MainContent_ucGroupAndFacility_{RegID}_ucHealthCareAffiliations_txtFacilityName")).Element;
            }
        }
        public IWebElement Staffcategory
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id($"ctl00_MainContent_ucGroupAndFacility_{RegID}_ucHealthCareAffiliations_ddlStaffCategoryID")).Element;
            }
        }
        public IWebElement StatusOfPrivileges
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id($"ctl00_MainContent_ucGroupAndFacility_{RegID}_ucHealthCareAffiliations_ddlAffiliationPrivilegesStatus")).Element;
            }
        }
        public IWebElement StartDate
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id($"ctl00_MainContent_ucGroupAndFacility_{RegID}_ucHealthCareAffiliations_txtStart_Date")).Element;
            }
        }
        public IWebElement RestrictedPrivileges_No
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id($"ctl00_MainContent_ucGroupAndFacility_{RegID}_ucHealthCareAffiliations_rblRestrictedPrivileges_1")).Element;
            }
        }
        public IWebElement RestrictedPrivileges_Yes
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id($"ctl00_MainContent_ucGroupAndFacility_{RegID}_ucHealthCareAffiliations_rblRestrictedPrivileges_0")).Element;
            }
        }
        public IWebElement RestrictedPrivileges_Reason
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id($"ctl00_MainContent_ucGroupAndFacility_{RegID}_ucHealthCareAffiliations_txtResponseComment")).Element;
            }
        }
        
        public IWebElement HospitalAffiliation_Save
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id($"ctl00_MainContent_ucGroupAndFacility_{RegID}_ucHealthCareAffiliations_btnSave")).Element;
            }
        }
        public IWebElement HospitalAffiliation_Cancel
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id($"ctl00_MainContent_ucGroupAndFacility_{RegID}_ucHealthCareAffiliations_btnCancel")).Element;
            }
        }
        #endregion
    }
}
