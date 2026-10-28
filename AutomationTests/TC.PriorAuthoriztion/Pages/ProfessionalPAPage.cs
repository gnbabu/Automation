using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using SeleniumExtensions.Configurations;
using SeleniumHelper = Selenium.BaseComponents.Utilities.PageHelper;

namespace TC.PriorAuthoriztion.Pages
{
    public class ProfessionalPAPage
    {
        private const string Prefix = "ctl00_MainContent_uc1SubmitPriorAuthorization_";
        private readonly IWebDriver _driver;

        public ProfessionalPAPage(IWebDriver driver)
        {
            _driver = driver;
        }

        public static By Id(string suffix) => By.Id(Prefix + suffix);

        public By ProfessionalClaimType => Id("rblClaimType_1");
        public By DestinationPayer => Id("ddlAuthorization");
        public By Assignment => Id("ddlAssignment");
        public By ServiceType => Id("ddlServiceType");
        public By DestinationPayerId => Id("ddlSubCapitaPayerIDs");

        public By MedicaidBillingNumber => Id("txtMedicaidBillingNumber");
        public By BirthDate => Id("txtBirthDate");
        public By PatientTrackingNumber => Id("txtPatientTrckNum");
        public By RecipientFirstName => Id("txtfrstmi2");

        public By ContactFirstName => Id("txtContactName");
        public By ContactLastName => Id("txtContactLastName");
        public By ContactNumber => Id("txtContactNumber");
        public By ContactExtension => Id("txtExt");

        public By PlaceOfService => Id("txtpalceofservice");
        public By PlaceOfServiceSearch => Id("lnkplaceofservicesrch");
        public By PlaceOfServiceSearchCode => Id("txtPlaceOfServiceCode");
        public By PlaceOfServicePopup => Id("pnlplaceofServiceSearch");
        public By PlaceOfServiceSearchButton =>
            By.XPath($"//input[@id='{Prefix}txtPlaceOfServiceCode']/following::button[contains(concat(' ', normalize-space(@class), ' '), ' buttonBoxFocus ')][1]");
        public By LevelOfService => Id("ddlLvlServiceInst");
        public By DelayReason => Id("ddlDelayedInst");
        public By AccidentDate => Id("txtAccDtService");
        public By DateOfPatientEvent => Id("TextBox3");
        public By DateOfOnsetOfIllness => Id("txtProfOnsetIllness");
        public By LastMenstrualPeriodDate => Id("txtMenDtInst");
        public By EstimatedBirthDate => Id("txtEstDOB");
        public By AssociatePaNumber => Id("txtPANumInst");

        public By ServiceProviderNpi => Id("txtSPNPI");
        public By ServiceProviderSearch => By.Id("btnSearch6");
        public By ServiceProviderSearchNpi => Id("txtNPI");
        public By ServiceProviderSearchButton => Id("btnSearch");
        public By ServiceProviderMedicaidId => Id("txtMedicaidID");
        public By ServiceProviderName => Id("lblSvcProviderLName");

        public By OrderingProviderHeader => By.XPath("//span[normalize-space()='ORDERING PROVIDER INFORMATION']");
        public By OrderingProviderNpi => Id("txtorderingprovidernpi");
        public By OrderingProviderSearch => By.Id("btnSearch1");
        public By OrderingProviderSearchNpi => Id("txtNPIORD");
        public By OrderingProviderSearchButton => Id("btnSearchORD");

        public By DiagnosisHeader => By.XPath("//span[normalize-space()='DIAGNOSIS INFORMATION']");
        public By DiagnosisAdd => Id("btnDiagnosisAdd");
        public By DiagnosisCode => Id("txtLnDiagnosisCode");
        public By DiagnosisSearch => Id("lnkDiagnosisSearch");
        public By DiagnosisSearchCode => Id("txtDiagnosisCodeSearch1");
        public By DiagnosisSearchButton => Id("btndiagnosiscodeSearch");
        public By DiagnosisDate => Id("txtDiagnosisDate");
        public By DiagnosisCodeType => Id("ddlDiagnosisCodeType");
        public By DiagnosisAddLine => By.Id("btnDiagnosisAddLine");

        public By ServiceDetailsHeader => By.XPath("//span[normalize-space()='SERVICE DETAILS']");
        public By ServiceDetailAdd => Id("btnProfessionalServiceDetailAdd");
        public By ProcedureCode => Id("txtProfessionalSDProcCode");
        public By ProcedureCodeSearch => Id("lnkProfessionalSDProcCodeSearchLink");
        public By ProcedureSearchCode => Id("txtCode");
        public By ProcedureSearchButton => Id("Button9");
        public By Modifier(int index) => Id($"txtModifier{index}");
        public By RequestedUnits => Id("txtProfessionalReqUnits");
        public By RequestedDollars => Id("txtProfessionalReqDollars");
        public By RequestedFromDate => Id("txtProfessionalReqFDOS");
        public By RequestedToDate => Id("txtProfessionalReqTDOS");
        public By ServiceDetailSave => By.Id("btnServProfessionalUpdate");

        public By AttachmentHeader => By.XPath("//span[normalize-space()='ATTACHMENT']");
        public By AttachmentUpload => Id("PriorAttachmentUpload");
        public By AttachmentDocumentType => Id("ddlPriorAuthDocType");
        public By AttachmentNote => Id("txtPriorAttachmentNote");
        public By AttachmentAdd => By.Id("btnClientAttachments");

        public By ProviderNotes => Id("txtProviderNotes");
        public By ProviderNotesHeader => Id("lblsepProvidersNotes");
        public By SaveButton => Id("btnSave");
        public By SaveMessage => Id("lblpriorautherror");
        public By SubmitButton => Id("btnSubmit");
        public static By HeaderField(string label) =>
            By.XPath($"//*[normalize-space(text())={XPathLiteral(label + ":")}]/following::input[1]");
        public By PaStatusField => HeaderField("PA Status");
        public By PaNumberField => HeaderField("PA Number");
        public By PaSubmissionDateField => HeaderField("PA Submission Date");
        public By WarningPanel => Id("pnlWarningAcknowledgment");
        public By WarningYes => Id("btnWarningAcknowledgmentYes");
        public By SubmitSuccessPanel => Id("pnmPATXNSuccess");
        public By SubmitFailurePanel => Id("pnmPATXNFailure");
        public By SubmitFailureMessage => By.XPath($"//*[@id='{Prefix}grdTXNResponse']/tbody/tr/td[2]");
        public By ValidationMessages => By.XPath(
            "//*[(contains(@id,'Validator') or contains(@id,'lblError') or contains(@id,'lblerror') or contains(@class,'validation-summary') or contains(@class,'field-validation-error')) and normalize-space()]");

        public static By LinkWithText(string text) =>
            By.XPath($"//a[normalize-space()={XPathLiteral(text)}]");

        public IWebElement Visible(By by) =>
            SeleniumHelper.WaitUntilElementIsVisibleAndReturn(_driver, by, TimeoutConfiguration.Element);

        private void Stable(Action action)
        {
            for (var attempt = 1; ; attempt++)
            {
                SeleniumHelper.WaitForPostback(_driver, TimeSpan.FromSeconds(30));
                try
                {
                    action();
                    SeleniumHelper.WaitForPostback(_driver, TimeSpan.FromSeconds(30));
                    return;
                }
                catch (StaleElementReferenceException) when (attempt < 3)
                {
                    Thread.Sleep(300 * attempt);
                }
            }
        }

        public void Click(By by) => Stable(() => ClickOnce(by));

        private void ClickOnce(By by)
        {
            var element = SeleniumHelper.WaitUntilElementIsClickableAndReturn(_driver, by, TimeoutConfiguration.Element);
            try
            {
                element.Click();
            }
            catch (WebDriverException)
            {
                ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].click();", element);
            }
        }

        public void ClickVisibleLink(string text) => Stable(() => ClickVisibleLinkOnce(text));

        private void ClickVisibleLinkOnce(string text)
        {
            var wait = NewWait();
            var link = wait.Until(d => d.FindElements(LinkWithText(text)).FirstOrDefault(e => e.Displayed));
            ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].scrollIntoView({block:'center'});", link);
            try
            {
                link.Click();
            }
            catch (WebDriverException)
            {
                ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].click();", link);
            }
        }

        public void ClickLinkInPopup(By popup, string text) => Stable(() => ClickLinkInPopupOnce(popup, text));

        private void ClickLinkInPopupOnce(By popup, string text)
        {
            var wait = NewWait();
            var link = wait.Until(d =>
            {
                var panel = d.FindElements(popup).FirstOrDefault(p => p.Displayed);
                return panel?.FindElements(By.XPath($".//a[normalize-space()={XPathLiteral(text)}]")).FirstOrDefault(e => e.Displayed);
            });
            ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].scrollIntoView({block:'center'});", link);
            try
            {
                link.Click();
            }
            catch (WebDriverException)
            {
                ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].click();", link);
            }
        }

        public void ClickButtonInPopup(By popup, string label) => Stable(() => ClickButtonInPopupOnce(popup, label));

        private void ClickButtonInPopupOnce(By popup, string label)
        {
            var literal = XPathLiteral(label);
            var wait = NewWait();
            var button = wait.Until(d =>
            {
                var panel = d.FindElements(popup).FirstOrDefault(p => p.Displayed);
                return panel?.FindElements(By.XPath(
                        $".//input[(@type='submit' or @type='button') and normalize-space(@value)={literal}]" +
                        $" | .//button[normalize-space(.)={literal}]" +
                        $" | .//a[normalize-space(.)={literal}]"))
                    .FirstOrDefault(e => e.Displayed && e.Enabled);
            });
            try
            {
                button.Click();
            }
            catch (WebDriverException)
            {
                ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].click();", button);
            }
        }

        public bool IsDisplayed(By by)
        {
            try
            {
                return _driver.FindElements(by).Any(e => e.Displayed);
            }
            catch (StaleElementReferenceException)
            {
                return false;
            }
        }

        public bool WaitUntilHidden(By by, TimeSpan timeout)
        {
            try
            {
                new WebDriverWait(_driver, timeout).Until(d => !d.FindElements(by).Any(e => e.Displayed));
                return true;
            }
            catch (WebDriverTimeoutException)
            {
                return false;
            }
        }

        public void ClosePopup(By popup)
        {
            if (WaitUntilHidden(popup, TimeSpan.FromSeconds(5))) return;

            var panel = _driver.FindElements(popup).FirstOrDefault(p => p.Displayed);
            var close = panel?.FindElements(By.XPath(
                    ".//input[(@type='button' or @type='submit' or @type='image') and (contains(translate(@value,'CLOSE','close'),'close') or contains(translate(@id,'CLOSE','close'),'close') or normalize-space(@value)='X')]" +
                    " | .//button[contains(translate(normalize-space(.),'CLOSE','close'),'close') or normalize-space(.)='X' or normalize-space(.)='×' or contains(translate(@class,'CLOSE','close'),'close') or contains(translate(@id,'CLOSE','close'),'close')]" +
                    " | .//a[contains(translate(normalize-space(.),'CLOSE','close'),'close') or normalize-space(.)='X' or normalize-space(.)='×' or contains(translate(@id,'CLOSE','close'),'close') or contains(translate(@class,'CLOSE','close'),'close')]"))
                .FirstOrDefault(e => e.Displayed);

            if (close != null)
            {
                try { close.Click(); }
                catch (WebDriverException) { ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].click();", close); }
            }
            else
            {
                new OpenQA.Selenium.Interactions.Actions(_driver).SendKeys(Keys.Escape).Perform();
            }

            if (!WaitUntilHidden(popup, TimeSpan.FromSeconds(10)))
                throw new WebDriverException($"The search popup ({popup}) did not close after selecting a value.");
        }

        public void Type(By by, string? value, bool pressEnter = false)
        {
            if (string.IsNullOrWhiteSpace(value)) return;
            Stable(() =>
            {
                var element = Visible(by);
                element.Clear();
                element.SendKeys(value);
            });
            if (pressEnter) Stable(() => _driver.FindElement(by).SendKeys(Keys.Enter));
        }

        public void Select(By by, string? text)
        {
            if (string.IsNullOrWhiteSpace(text)) return;
            Stable(() =>
            {
                NewWait().Until(d =>
                {
                    var select = new SelectElement(d.FindElement(by));
                    return select.Options.Any(o => o.Text.Trim() == text.Trim());
                });
                new SelectElement(_driver.FindElement(by)).SelectByText(text.Trim());
            });
        }

        private WebDriverWait NewWait(TimeSpan? timeout = null)
        {
            var wait = new WebDriverWait(_driver, timeout ?? TimeoutConfiguration.Element);
            wait.IgnoreExceptionTypes(typeof(StaleElementReferenceException), typeof(NoSuchElementException));
            return wait;
        }

        public void ExpandSection(By header, By contentInside)
        {
            if (_driver.FindElements(contentInside).Any(e => e.Displayed)) return;
            Click(header);
            Visible(contentInside);
        }

        public void WaitForValue(By by, TimeSpan timeout) =>
            NewWait(timeout).Until(d => !string.IsNullOrEmpty(d.FindElement(by).GetAttribute("value")));

        public void WaitForText(By by, TimeSpan timeout) =>
            NewWait(timeout).Until(d => !string.IsNullOrWhiteSpace(d.FindElement(by).Text));

        private static string XPathLiteral(string value) =>
            !value.Contains('\'') ? $"'{value}'"
            : !value.Contains('"') ? $"\"{value}\""
            : "concat('" + value.Replace("'", "',\"'\",'") + "')";
    }
}
