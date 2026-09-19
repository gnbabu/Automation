using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using SdetToolbox.Pages;
using Selenium.BaseComponents.Pages;
using Selenium.BaseComponents.Utilities;
using SeleniumExtensions.Configurations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using BasePageHelper = Selenium.BaseComponents.Utilities.PageHelper;

namespace TC.ProviderDataEntry.Pages.Registration
{
    public class RegistrationPage : BasePage
    {
        IWebDriver webDriver;
        public string RegID;

        public RegistrationPage(IWebDriver webDriver) : base(webDriver)
        {

            this.webDriver = webDriver;

            string url = this.webDriver.Url;
            var regIDTag = url.Split('?').Last();
            var regID = regIDTag.Split('=').Last();

            RegID = regID;
        }

        #region Provider Information 

        public bool isRequiredMessageAppear
        {
            get
            {
                return BasePageHelper.IsElementExist(webDriver, By.Id("ctl00_MainContent_ucRegistrationNavigation_lblRequired"));
            }
        }

        public IWebElement RequiredMessage
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id("ctl00_MainContent_ucRegistrationNavigation_lblRequired")).Element;
            }
        }

        public void SetResident(string isResident)
        {
            string xpath = isResident switch
            {
                "Yes" => $"//*[@id='ctl00_MainContent_ucOrgInfo_{RegID}_rblIsOhioResident_0']",
                "No" => $"//*[@id='ctl00_MainContent_ucOrgInfo_{RegID}_rblIsOhioResident_1']",
                _ => throw new ArgumentException($"Invalid resident value: {isResident}")
            };
            var element = webDriver.CreateSmartElement(By.XPath(xpath));
            element.Element.Click();
        }

        public IWebElement BtnNext
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id("ctl00_MainContent_ucRegistrationNavigation_btnSaveNext")).Element;
            }
        }
        public IWebElement BtnSubmitForReview
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id("ctl00_rptWorkflowActions_ctl00_btnAction")).Element;
            }
        }

        public void SetPracticeType(string item, WebDriverWait wait)
        {
            wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementExists(By.Id($"ctl00_MainContent_ucOrgInfo_{RegID}_ddlPracticeType")));
            var PracticeTypeElement = webDriver.CreateSmartElement(By.Id($"ctl00_MainContent_ucOrgInfo_{RegID}_ddlPracticeType"));
            SelectElement PracticeTypeElementDropDown = new SelectElement(PracticeTypeElement.Element);
            PracticeTypeElementDropDown.SelectByText(item);
        }

        public void SetOwnershipType(string item, WebDriverWait wait)
        {
            wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementExists(By.Id($"ctl00_MainContent_ucOrgInfo_{RegID}_ddlOwnershiptype")));
            var OwnershipTypeElement = webDriver.CreateSmartElement(By.Id($"ctl00_MainContent_ucOrgInfo_{RegID}_ddlOwnershiptype"));
            SelectElement OwnershipTypeElementDropDown = new SelectElement(OwnershipTypeElement.Element);
            OwnershipTypeElementDropDown.SelectByText(item);
        }

        #endregion


        public void WaitUntilPageLoad()
        {
            BasePageHelper.WaitUntilElementIsVisible(webDriver, By.XPath($"//*[@id='ctl00_MainContent_ucRegistrationNavigation_lblTitlePS']"), TimeoutConfiguration.Element);
        }

        public IWebElement More
        {
            get
            {
                return webDriver.CreateSmartElement(By.XPath($"//a[contains(@class,'btn btn-primary btn-md') and text()='More ...']")).Element;

            }
        }

        public IWebElement BtnApprove
        {
            get
            {
                return webDriver.CreateSmartElement(By.XPath($"//*[@id='ctl00_MainContent_ucRegistrationNavigation_btnTakeActionApprove']")).Element;

            }
        }

        public IWebElement TakeAction
        {
            get
            {
                return webDriver.CreateSmartElement(By.XPath($"//*[@id='ctl00_rptWorkflowActions_ctl00_btnAction']")).Element;
            }
        }

        public string GetLoggedInUser()
        {
            var element = webDriver.CreateSmartElement(By.Id("ctl00_LoginView2_LoginName1"));
            return element.Element.Text;
        }

        public IWebElement AssignedTo
        {
            get
            {
                return webDriver.CreateSmartElement(By.Id("ctl00_ddlAssignedTo")).Element;
            }
        }

        public void SetAssignmentUser()
        {
            string loggedInUser = GetLoggedInUser();

            if (!string.IsNullOrEmpty(loggedInUser))
            {
                SelectElement assignedType = new SelectElement(AssignedTo);
                assignedType.SelectByText(loggedInUser);
            }
        }

        public IWebElement SidebarMenu
        {
            get
            {
                return webDriver.CreateSmartElement(By.XPath("//button[contains(@class,'hamburger is-closed')]")).Element;
            }
        }

        public void NavigateToMyQueue()
        {
            if (MyQueueLink != null)
            {
                MyQueueLink.Click();
            }
        }

        public IWebElement MyQueueLink
        {
            get
            {
                return webDriver.CreateSmartElement(By.XPath($"//a[@title='My Queue']")).Element;

            }
        }
    }
}