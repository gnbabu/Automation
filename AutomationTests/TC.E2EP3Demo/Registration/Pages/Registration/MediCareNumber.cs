using OpenQA.Selenium.Support.UI;
using Selenium.BaseComponents.Pages;
using SeleniumExtensions.Extensions;
using Selenium.BaseComponents.Utilities;
using OpenQA.Selenium;

namespace TC.ProviderDataEntry.Pages.Registration
{
    public class MediCareNumber : BasePage
    {
        IWebDriver webDriver;
        string RegID;
        public MediCareNumber(IWebDriver webDriver, string RegID) : base(webDriver)
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
        #region MediCare Number
        public IWebElement? RegistrationTitle
        {
            get
            {
                return Selenium.BaseComponents.Utilities.PageHelper.IsElementExist(webDriver, By.Id("ctl00_MainContent_ucRegistrationNavigation_lblTitle")) ?
                    webDriver.CreateSmartElement(By.Id("ctl00_MainContent_ucRegistrationNavigation_lblTitle")).Element : null;
            }
        }
        public IWebElement AddMediCareNumber
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id($"ctl00_MainContent_ucMiscellaneous_{RegID}_ucMedicare_btnAddMedicare")).Element;
            }
        }
        public IWebElement CCNMediCareType
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id($"ctl00_MainContent_ucMiscellaneous_{RegID}_ucMedicare_rblMedicareNumberType_0")).Element;
            }
        }
        public IWebElement PTANMediCareType
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id($"ctl00_MainContent_ucMiscellaneous_{RegID}_rblMedicareNumberType_1")).Element;
            }
        }
        public IWebElement MediCareNum
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id($"ctl00_MainContent_ucMiscellaneous_{RegID}_ucMedicare_txtMedicareNumber")).Element;
            }
        }
        public IWebElement MediCareState
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id($"ctl00_MainContent_ucMiscellaneous_{RegID}_ucMedicare_ddlProvState")).Element;
            }
        }
        public IWebElement MediCareFileUpload
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id($"ctl00_MainContent_ucMiscellaneous_{RegID}_ucMedicare_1187_RadAsyncUpload1file0")).Element;
            }
        }
        #endregion
    }
}
