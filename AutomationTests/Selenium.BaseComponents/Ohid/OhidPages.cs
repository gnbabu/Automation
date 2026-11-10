using OpenQA.Selenium;

namespace Selenium.BaseComponents.Ohid
{
    public static class OhidPages
    {
        public static readonly By PnmLoginWithOhid = By.XPath(
            "//*[self::a or self::button or self::input][contains(normalize-space(.),'Log in with OH') or contains(@value,'Log in with OH')]");

        public static readonly By PnmUserId = By.Id("ctl00_MainContent_Login1_UserName");
        public static readonly By PnmNext = By.Id("ctl00_MainContent_Login1_btnNext");
        public static readonly By PnmGoToIop = By.Id("ctl00_MainContent_Login1_btnGotoIOP");
        public static readonly By PnmPassword = By.Id("ctl00_MainContent_Login1_Password");

        public static readonly By OhidUsername = By.XPath(
            "//label[contains(normalize-space(.),'OHID username')]/following::input[not(@type='hidden')][1]" +
            " | //input[@name='username' or @id='username']");

        public static readonly By OhidPassword = By.XPath("//input[@type='password']");

        public static readonly By OhidLogIn = By.XPath(
            "//button[normalize-space(.)='Log in'] | //input[(@type='submit' or @type='button') and @value='Log in']");

        public static readonly By TwoStepHeading = By.XPath(
            "//p[contains(@class,'bx--type-delta')] | //*[normalize-space(.)='Two-step verification' or normalize-space(.)='Choose a method']");

        public static readonly By TotpOption = By.XPath(
            "(//*[normalize-space(text())='TOTP AuthApp' or normalize-space(text())='TOTP Authenticator']" +
            "/following::*[self::a or self::button][normalize-space(.)='Enter code'])[1]");

        public static readonly By CodeInput = By.XPath(
            "//input[@autocomplete='one-time-code']" +
            " | //input[not(@type) or @type='text' or @type='tel' or @type='number'][not(@readonly)]");

        public static readonly By VerifyButton = By.XPath(
            "//*[self::button or self::a][normalize-space(.)='Verify' or normalize-space(.)='Submit']" +
            " | //input[(@type='submit' or @type='button') and (@value='Verify' or @value='Submit')]");

        public static readonly By ErrorMessage = By.XPath(
            "//*[@role='alert' or contains(@class,'error') or contains(@class,'alert-danger') or contains(@class,'bx--inline-notification--error')][normalize-space(.)!='']");

        public static readonly By PnmTermsAgreement = By.XPath(
            "//input[@id='ctl00_MainContent_chkTerms']" +
            " | //input[@type='checkbox'][following-sibling::*[contains(normalize-space(.),'I have read the agreement')] or ../*[contains(normalize-space(.),'I have read the agreement')]]");

        public static readonly By PnmSignedIn = By.XPath(
            "//*[@id='ctl00_LoginView2_lnkLogout'] | //a[normalize-space(.)='Logout' or normalize-space(.)='Log out']");
    }
}
