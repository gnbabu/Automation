using OpenQA.Selenium;
using Selenium.BaseComponents.Pages;
using Selenium.BaseComponents.Utilities;

namespace Selenium.BaseComponents.CommonPages.Login
{
    /// <summary>
    /// Page object for the login page
    /// Encapsulates all login page elements and interactions
    /// </summary>
    public class LoginPage : BasePage
    {
        private IWebDriver _webDriver;

        public LoginPage(IWebDriver webDriver) : base(webDriver)
        {
            _webDriver = webDriver;
        }

        public IWebElement UserName
        {
            get
            {
                return _webDriver.CreateSmartElement(By.XPath("//input[@id='ctl00_MainContent_Login1_UserName']")).Element;
            }
        }

        public IWebElement Password
        {
            get
            {
                return _webDriver.CreateSmartElement(By.XPath("//input[@id='ctl00_MainContent_Login1_Password']")).Element;
            }
        }

        public IWebElement ChkLoginTerms
        {
            get
            {
                return _webDriver.CreateSmartElement(By.XPath("//input[@id='ctl00_MainContent_chkTerms']")).Element;
            }
        }

        public IWebElement NextButton
        {
            get
            {
                return _webDriver.CreateSmartElement(By.XPath("//input[@id='ctl00_MainContent_Login1_btnNext']")).Element;
            }
        }

        public IWebElement LoginButton
        {
            get
            {
                return _webDriver.CreateSmartElement(By.XPath("//input[@id='ctl00_MainContent_Login1_LoginButton']")).Element;
            }
        }

        public IWebElement LogOutButton
        {
            get
            {
                return _webDriver.CreateSmartElement(By.Id("ctl00_LoginView2_lnkLogout")).Element;
            }
        }

        #region Cancel Change Password
        private IWebElement _buttonCancel;
        public IWebElement Button_Cancel
        {
            get
            {
                if (_buttonCancel == null)
                    _buttonCancel = _webDriver.CreateSmartElement(By.Id("cancel-button")).Element;
                return _buttonCancel;
            }
        }

        public void ClickCancelButton()
        {
            if (Button_Cancel != null)
            {
                Button_Cancel.Click();
            }
        }
        #endregion
    }
}
