using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace Selenium.BaseComponents.Ohid
{
    public class OhidLoginService
    {
        private static readonly TimeSpan PageTimeout = TimeSpan.FromSeconds(60);
        private static readonly TimeSpan CodeTimeout = TimeSpan.FromSeconds(45);
        private static readonly string[] LockoutWords = { "locked", "too many", "suspended", "disabled" };

        private readonly IWebDriver _driver;
        private readonly Action<string, string> _log;

        public OhidLoginService(IWebDriver driver, Action<string, string> log)
        {
            _driver = driver;
            _log = log;
        }

        public void Login(string pnmUrl, string userName, string password, IOtpProvider otp, string? pnmUserId = null)
        {
            try
            {
                OpenOhid(pnmUrl, pnmUserId ?? userName);
                SignInToOhidIfAsked(userName, password);
                if (OnOhid())
                {
                    ChooseAuthenticator();
                    EnterCodeAndVerify(otp);
                }
                else
                {
                    _log("Two-step verification skipped", "OH|ID returned to PNM without asking for a one-time code.");
                }
                ConfirmSignedIn(pnmUrl);
            }
            catch
            {
                ClearSensitiveInputs();
                throw;
            }
        }

        private void OpenOhid(string pnmUrl, string pnmUserId)
        {
            _driver.Navigate().GoToUrl(pnmUrl);
            WaitFor("the PNM login page", () => IsVisible(OhidPages.PnmLoginWithOhid) || IsVisible(OhidPages.PnmUserId) || OnOhid());
            var windows = _driver.WindowHandles.Count;

            if (IsVisible(OhidPages.PnmLoginWithOhid))
            {
                Click(OhidPages.PnmLoginWithOhid);
            }
            else if (IsVisible(OhidPages.PnmUserId))
            {
                Type(OhidPages.PnmUserId, pnmUserId);
                Click(OhidPages.PnmNext);
                WaitFor("the 'Go to IOP' (OH|ID) button after entering the user ID",
                    () => IsVisible(OhidPages.PnmGoToIop) || IsVisible(OhidPages.PnmPassword));
                if (!IsVisible(OhidPages.PnmGoToIop))
                    throw new InvalidOperationException(
                        "This environment asked for a PNM password instead of offering OH|ID ('Go to IOP'). Use the regular login for this login user or environment.");
                Click(OhidPages.PnmGoToIop);
            }

            SwitchToNewWindowIfOpened(windows);
            WaitFor("the OH|ID pages", () => OnOhid() || IsVisible(OhidPages.TwoStepHeading));
            _log("Opened OH|ID login", $"Redirected to OH|ID ({Host()}).");
        }

        private void SignInToOhidIfAsked(string userName, string password)
        {
            WaitFor("the OH|ID login or two-step verification page",
                () => !OnOhid() || IsVisible(OhidPages.OhidUsername) || IsVisible(OhidPages.TwoStepHeading) || IsVisible(OhidPages.TotpOption));
            if (!IsVisible(OhidPages.OhidUsername)) return;
            if (string.IsNullOrEmpty(password))
                throw new InvalidOperationException("OH|ID asked for a username and password, but this login user has no password saved in Credential Configuration.");

            Type(OhidPages.OhidUsername, userName);
            Type(OhidPages.OhidPassword, password);
            Click(OhidPages.OhidLogIn);
            _log("Entered OH|ID credentials", "Entered the OH|ID username and password and clicked Log in.");

            WaitFor("two-step verification (or the return to PNM) after logging in to OH|ID",
                () => !OnOhid() || IsVisible(OhidPages.TwoStepHeading) || IsVisible(OhidPages.TotpOption) || ErrorText() != null);
            ThrowOnOhidError("OH|ID login");
        }

        private void ChooseAuthenticator()
        {
            WaitFor("the TOTP authenticator option on two-step verification", () => IsVisible(OhidPages.TotpOption) || ErrorText() != null);
            ThrowOnOhidError("Two-step verification");
            if (!IsVisible(OhidPages.TotpOption))
                throw new InvalidOperationException("No authenticator (TOTP) is registered on this OH|ID account.");

            Click(OhidPages.TotpOption);
            _log("Chose TOTP authenticator", "Chose the authenticator app on two-step verification.");
            WaitFor("the one-time code entry page", () => IsVisible(OhidPages.CodeInput) || ErrorText() != null);
            ThrowOnOhidError("Two-step verification");
        }

        private void EnterCodeAndVerify(IOtpProvider otp)
        {
            for (var attempt = 1; ; attempt++)
            {
                var code = otp.WaitForOtpAsync(CodeTimeout).GetAwaiter().GetResult()
                    ?? throw new InvalidOperationException("Could not generate a one-time code from the TOTP key.");

                Type(OhidPages.CodeInput, code);
                Click(OhidPages.VerifyButton);
                _log("Entered one-time code", "Entered the authenticator code and clicked Verify.");

                WaitFor("OH|ID to accept the one-time code", () => !OnOhid() || ErrorText() != null);
                if (!OnOhid()) return;

                var error = ErrorText();
                if (IsLockout(error))
                    throw new InvalidOperationException($"The OH|ID account appears to be locked ({error}). Not retrying.");
                if (attempt == 2)
                    throw new InvalidOperationException(
                        $"OH|ID rejected the one-time code twice ({error}). Check the TOTP key in {OhidSettings.LocalFileName} and the machine's clock.");

                _log("One-time code rejected", "OH|ID rejected the code; retrying once with the next code.");
            }
        }

        private void ConfirmSignedIn(string pnmUrl)
        {
            var pnmHost = new Uri(pnmUrl).Host;
            bool OnPnm() => string.Equals(Host(), pnmHost, StringComparison.OrdinalIgnoreCase);

            WaitFor("the redirect back to PNM after OH|ID verification",
                () => OnPnm() && (IsVisible(OhidPages.PnmSignedIn) || IsVisible(OhidPages.PnmTermsAgreement)));

            if (!IsVisible(OhidPages.PnmSignedIn) && IsVisible(OhidPages.PnmTermsAgreement))
            {
                Click(OhidPages.PnmTermsAgreement);
                _log("Accepted terms", "Checked \"Yes, I have read the agreement\" on the PNM Terms dialog.");
                WaitFor("PNM to continue after accepting the terms", () => OnPnm() && IsVisible(OhidPages.PnmSignedIn));
            }

            _log("Signed in to PNM", $"Signed in through OH|ID and back on {pnmHost}.");
        }

        private void ThrowOnOhidError(string step)
        {
            var error = ErrorText();
            if (error == null) return;
            throw new InvalidOperationException(IsLockout(error)
                ? $"{step}: the OH|ID account appears to be locked ({error}). Not retrying."
                : $"{step}: OH|ID reported \"{error}\".");
        }

        private static bool IsLockout(string? error) =>
            error != null && LockoutWords.Any(w => error.Contains(w, StringComparison.OrdinalIgnoreCase));

        private string? ErrorText()
        {
            try
            {
                return _driver.FindElements(OhidPages.ErrorMessage).FirstOrDefault(e => e.Displayed)?.Text?.Trim() is { Length: > 0 } text ? text : null;
            }
            catch (WebDriverException)
            {
                return null;
            }
        }

        private bool OnOhid() => Host().Contains("ohid", StringComparison.OrdinalIgnoreCase);

        private string Host()
        {
            try { return new Uri(_driver.Url).Host; }
            catch (Exception) { return ""; }
        }

        private void SwitchToNewWindowIfOpened(int windowsBefore)
        {
            try
            {
                new WebDriverWait(_driver, TimeSpan.FromSeconds(3)).Until(d => d.WindowHandles.Count > windowsBefore);
                _driver.SwitchTo().Window(_driver.WindowHandles.Last());
            }
            catch (WebDriverTimeoutException)
            {
            }
        }

        private bool IsVisible(By by)
        {
            try { return _driver.FindElements(by).Any(e => e.Displayed); }
            catch (WebDriverException) { return false; }
        }

        private void WaitFor(string what, Func<bool> condition)
        {
            var wait = new WebDriverWait(_driver, PageTimeout) { PollingInterval = TimeSpan.FromMilliseconds(500) };
            wait.IgnoreExceptionTypes(typeof(StaleElementReferenceException), typeof(NoSuchElementException));
            try
            {
                wait.Until(_ => condition());
            }
            catch (WebDriverTimeoutException)
            {
                throw new WebDriverTimeoutException($"Timed out after {PageTimeout.TotalSeconds:0} s waiting for {what} (at {Host()}).");
            }
        }

        private void Click(By by)
        {
            WaitFor($"a clickable {by}", () => IsVisible(by));
            for (var attempt = 1; ; attempt++)
            {
                try
                {
                    _driver.FindElements(by).First(e => e.Displayed).Click();
                    return;
                }
                catch (StaleElementReferenceException) when (attempt < 3)
                {
                }
            }
        }

        private void Type(By by, string value)
        {
            WaitFor($"an input {by}", () => IsVisible(by));
            for (var attempt = 1; ; attempt++)
            {
                try
                {
                    var element = _driver.FindElements(by).First(e => e.Displayed);
                    element.Clear();
                    element.SendKeys(value);
                    return;
                }
                catch (StaleElementReferenceException) when (attempt < 3)
                {
                }
            }
        }

        private void ClearSensitiveInputs()
        {
            foreach (var by in new[] { OhidPages.OhidPassword, OhidPages.CodeInput })
            {
                try
                {
                    foreach (var element in _driver.FindElements(by).Where(e => e.Displayed))
                        element.Clear();
                }
                catch (WebDriverException)
                {
                }
            }
        }
    }
}
