using TC.ProviderDataEntry.Models;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using Selenium.BaseComponents.Pages;
using SeleniumExtensions.Extensions;
using Selenium.BaseComponents.Utilities;
using SeleniumExtensions.Configurations;

namespace TC.ProviderDataEntry.Pages.Registration
{
    public class Specialties:BasePage
    {
        IWebDriver webDriver;
        string RegID;
        public Specialties(IWebDriver webDriver, string RegID) : base(webDriver)
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

        #region Specalities

        public IWebElement? RegistrationTitle
        {
            get
            {
                return Selenium.BaseComponents.Utilities.PageHelper.IsElementExist(webDriver, By.Id("ctl00_MainContent_ucRegistrationNavigation_lblTitle")) ?
                    webDriver.CreateSmartElement(By.Id("ctl00_MainContent_ucRegistrationNavigation_lblTitle")).Element : null;
            }
        }
        public IWebElement AddSpecalities
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id($"ctl00_MainContent_ucSpecialties_{RegID}_btnAddSpecialties")).Element;
            }
        }

        public IWebElement DesignatedPrimarySpecalities
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id($"ctl00_MainContent_ucSpecialties_{RegID}_chkIsPrimary")).Element;
            }
        }

        public IWebElement Specality
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id($"ctl00_MainContent_ucSpecialties_{RegID}_ddlSpecialty")).Element;
            }
        }

        public IWebElement StartDate
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id($"ctl00_MainContent_ucSpecialties_{RegID}_txtSpecStart")).Element;
            }
        }
        public IWebElement EndDate
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id($"ctl00_MainContent_ucSpecialties_{RegID}_txtSpecEnd")).Element;
            }
        }

        #endregion
        #region Functions

        public void SelectSpecality(string item)
        {
            SelectElement SpecalityElementDropDown = new SelectElement(Specality);
            //SpecalityElementDropDown.SelectByText(item);

            SpecalityElementDropDown.SelectByIndex(1);
        }

        public void SetSpecalityInformation(SpecialitiesDTO info)
        {
            PageHelper.WaitUntilDocumentIsReady(webDriver,TimeoutConfiguration.Page);
           
            if (!Convert.ToBoolean(info.DesignatedPrimarySpecality))
                DesignatedPrimarySpecalities.Click();

            //if (!string.IsNullOrEmpty(info.StartDate))
            //    StartDate.Set(info.StartDate, true);

            if (!string.IsNullOrEmpty(info.Enddate))
                EndDate.Set(info.Enddate, true);

            if (!string.IsNullOrEmpty(info.Specality))
                SelectSpecality(info.Specality);

        }
        #endregion
    }
}
