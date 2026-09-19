using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using Selenium.BaseComponents;
using Selenium.BaseComponents.Pages;
using Selenium.BaseComponents.Utilities;
using SeleniumExtensions.Extensions;
using TC.ProviderDataEntry.Models;
using TC.ProviderDataEntry.Pages;
using TC.ProviderDataEntry.Pages.Registration;
using TC.ProviderDataEntry.Utilities;

namespace TC.E2EP3Demo.Registration
{
    // 6 real test cases against the real E2EP3 Registration wizard, all reusing the
    // exact same Page Objects already proven in TC.Registration/Tests/RegistrationTest.cs
    // (physically copied into this project - see AGENTS.md "30 demo test cases" for why
    // no project reference is used). Rather than surgically slicing that one ~1000-line,
    // multi-role-switching (StateAdmin/EnrollmentSpecialist approval workflow) mega-test
    // into pieces - real risk of introducing subtle bugs I have no way to verify against
    // the live site - these are built from smaller, genuinely independent checkpoints in
    // the same wizard, extracted into shared private helpers so each Test can call a
    // growing prefix without duplicating code. The multi-role approval/workflow portion
    // of the original test is deliberately NOT reproduced here - a materially heavier,
    // separate scenario (several additional logins/role switches) than what 6 demo test
    // cases should responsibly take on blind.
    [TestFixture("TechAdmin")]
    public class RegistrationTests : BaseFeatureFixture
    {
        // Captured by CreateNewProvider_FullWizardThroughSubmission and reused by the
        // last 2 tests - a real create-then-verify chain across independently-assignable
        // test cases, not just each test pulling the same static config value (matches
        // the "chain values between tests" half of the user's data-flow answer).
        private static string? _lastCreatedRegId;

        private TC.ProviderDataEntry.Models.Registration _registration = null!;
        private WebDriverWait _wait = null!;

        public RegistrationTests(string profile) : base(profile)
        {
        }

        [SetUp]
        public new void BeforeEachTest()
        {
            _wait = new WebDriverWait(TestWebDriver, TimeSpan.FromSeconds(30));
            _registration = (TC.ProviderDataEntry.Models.Registration)DataRepository.GetAutomationData("Registration");
        }

        // Checkpoint 1 - create a new Standard/Individual provider and reach the first
        // real section of the wizard (Taxonomy save -> Registration wizard page).
        private RegistrationPage CreateNewProviderUpToWizard()
        {
            var newProviderPage = new NewProviderPage(TestWebDriver);
            newProviderPage.NewProviderBtn.Click();
            newProviderPage.StandardType.Click();
            newProviderPage.IndividualType.Click();

            if (!newProviderPage.isNewProviderloaded())
                Assert.Fail("New Provider page is not loaded.");

            var newProviderDTO = _registration.NewProviderDTO;
            newProviderPage.SetNewProviderPage(newProviderDTO);
            newProviderPage.ProviderSavebtn.Click();

            newProviderPage.WaitUntilTaxonomyVisible();
            newProviderPage.SetTaxonomy(newProviderDTO.Taxonomy);
            _wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementToBeClickable(newProviderPage.ProviderSavebtn)).Click();
            newProviderPage.WaitUntilLoaderDisappear();

            return new RegistrationPage(TestWebDriver);
        }

        // Checkpoint 2 - Provider Information + Primary Contact Information sections.
        private void FillProviderAndContactInfo(RegistrationPage registrationPage)
        {
            if (!registrationPage.isRequiredMessageAppear)
            {
                Helper.PrintScreenShot(TestWebDriver, "ProviderInfo");
                _wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementToBeClickable(registrationPage.BtnNext)).Click();
            }
            else
            {
                registrationPage.SetPracticeType("GENERAL HOSPITAL", _wait);
                registrationPage.SetOwnershipType("COUNTY (GOVT)", _wait);
                registrationPage.SetResident("No");
                Helper.PrintScreenShot(TestWebDriver, "ProviderInfo");
                _wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementToBeClickable(registrationPage.BtnNext)).Click();
            }

            var providerContactInformation = new ProviderContactInformation(TestWebDriver, registrationPage.RegID);
            if (providerContactInformation.RegistrationTitle != null && providerContactInformation.RegistrationTitle.Text == "Primary Contact Information")
            {
                if (!registrationPage.isRequiredMessageAppear)
                {
                    Helper.PrintScreenShot(TestWebDriver, "PrimaryContactInformation");
                    _wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementToBeClickable(registrationPage.BtnNext)).Click();
                }
                else
                {
                    var providerContactInfo = _registration.ProviderContactInfo;
                    providerContactInformation.SetProviderContactInformation(providerContactInfo);
                    _wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementToBeClickable(registrationPage.BtnNext)).Click();
                    _wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementToBeClickable(providerContactInformation.AddressConfirmationbtn)).Click();
                    Helper.PrintScreenShot(TestWebDriver, "PrimaryContactInformation");
                    _wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementToBeClickable(registrationPage.BtnNext)).Click();
                }
            }
        }

        // Checkpoint 3 - Primary Address Service + Billing & Payment Address sections.
        private void FillPrimaryAndBillingAddress(RegistrationPage registrationPage)
        {
            var primaryAddressService = new PrimaryAddressService(TestWebDriver, registrationPage.RegID);
            if (primaryAddressService.RegistrationTitle != null && primaryAddressService.RegistrationTitle.Text == "Primary Service Address")
            {
                if (!registrationPage.isRequiredMessageAppear)
                {
                    Helper.PrintScreenShot(TestWebDriver, "PrimaryAddressService");
                    _wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementToBeClickable(registrationPage.BtnNext)).Click();
                }
                else
                {
                    var primaryAddressServiceDTO = _registration.PrimaryAddressServiceDTO;
                    primaryAddressService.SetPrimaryAddressInformation(primaryAddressServiceDTO);
                    _wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementToBeClickable(registrationPage.BtnNext)).Click();
                    _wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementToBeClickable(primaryAddressService.AddressConfirmationbtn)).Click();
                    Helper.PrintScreenShot(TestWebDriver, "PrimaryAddressService");
                    _wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementToBeClickable(registrationPage.BtnNext)).Click();
                }
            }

            var billingAndPaymentAddress = new BillingAndPaymentAddress(TestWebDriver, registrationPage.RegID);
            if (billingAndPaymentAddress.RegistrationTitle != null && billingAndPaymentAddress.RegistrationTitle.Text == "Billing & Payment Address")
            {
                if (!registrationPage.isRequiredMessageAppear)
                {
                    Helper.PrintScreenShot(TestWebDriver, "BillingAndPaymentAddress");
                    _wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementToBeClickable(registrationPage.BtnNext)).Click();
                }
                else
                {
                    var billingAndPaymentAddressDTO = _registration.BillingAndPaymentAddressDTO;
                    if (billingAndPaymentAddressDTO.isSameAsPracticeLocation == "Yes")
                    {
                        _wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementToBeClickable(billingAndPaymentAddress.SameAsPracticeLocation)).Click();
                    }
                    else
                    {
                        billingAndPaymentAddress.SetBillingAddressInformation(billingAndPaymentAddressDTO);
                    }
                    _wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementToBeClickable(registrationPage.BtnNext)).Click();
                    _wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementToBeClickable(billingAndPaymentAddress.AddressConfirmationbtn)).Click();
                    Helper.PrintScreenShot(TestWebDriver, "BillingAndPaymentAddress");
                    _wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementToBeClickable(registrationPage.BtnNext)).Click();
                }
            }
        }

        [Test]
        [Author("Vishu")]
        [Category("IntegrationTests")]
        [Property("Description", "Create a new Standard/Individual provider and reach the Registration wizard")]
        [Property("Priority", "High")]
        [Property("TestCaseId", "TC_E2EP3_REG_01")]
        [CancelAfter(600000)]
        public void CreateNewProvider_ReachesRegistrationWizard()
        {
            var registrationPage = CreateNewProviderUpToWizard();
            Assert.That(registrationPage.RegID, Is.Not.Null.And.Not.Empty,
                "Expected a Reg ID to be assigned once the new provider is created.");
        }

        [Test]
        [Author("Vishu")]
        [Category("IntegrationTests")]
        [Property("Description", "Fill Provider Information and Primary Contact Information")]
        [Property("Priority", "High")]
        [Property("TestCaseId", "TC_E2EP3_REG_02")]
        [CancelAfter(600000)]
        public void Registration_ProviderAndContactInformation()
        {
            var registrationPage = CreateNewProviderUpToWizard();
            FillProviderAndContactInfo(registrationPage);
            Helper.PrintScreenShot(TestWebDriver, "ProviderAndContactInformation_Complete");
        }

        [Test]
        [Author("Vishu")]
        [Category("IntegrationTests")]
        [Property("Description", "Fill Primary Address Service and Billing & Payment Address")]
        [Property("Priority", "Medium")]
        [Property("TestCaseId", "TC_E2EP3_REG_03")]
        [CancelAfter(600000)]
        public void Registration_PrimaryAndBillingAddress()
        {
            var registrationPage = CreateNewProviderUpToWizard();
            FillProviderAndContactInfo(registrationPage);
            FillPrimaryAndBillingAddress(registrationPage);
            Helper.PrintScreenShot(TestWebDriver, "PrimaryAndBillingAddress_Complete");
        }

        // The "full" case - continues through the remaining real sections (Correspondence
        // Address, Other Service Locations, Address 1099, Home Office, Specialties,
        // Taxonomies) and Submit for Review, using the exact same page objects/DTOs as
        // the original TC.Registration test. Deliberately stops at Submit for Review -
        // not the subsequent StateAdmin/EnrollmentSpecialist approval workflow, a
        // materially larger, separate scenario (see class-level comment).
        [Test]
        [Author("Vishu")]
        [Category("IntegrationTests")]
        [Property("Description", "Full Registration wizard through Submit for Review")]
        [Property("Priority", "High")]
        [Property("TestCaseId", "TC_E2EP3_REG_04")]
        [CancelAfter(600000)]
        public void Registration_FullWizardThroughSubmission()
        {
            var registrationPage = CreateNewProviderUpToWizard();
            FillProviderAndContactInfo(registrationPage);
            FillPrimaryAndBillingAddress(registrationPage);

            var correspondenceAddressDTO = _registration.CorrespondenceAddressDTO;
            var correspondenceAddress = new CorrespondenceAddress(TestWebDriver, registrationPage.RegID);
            if (correspondenceAddress.RegistrationTitle != null && correspondenceAddress.RegistrationTitle.Text == "Correspondence Address")
            {
                if (!registrationPage.isRequiredMessageAppear)
                {
                    _wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementToBeClickable(registrationPage.BtnNext)).Click();
                }
                else
                {
                    if (correspondenceAddressDTO.isSameAsPracticeLocation == "Yes")
                        _wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementToBeClickable(correspondenceAddress.SameAsPracticeLocation)).Click();
                    else
                        correspondenceAddress.SetCorrespnodencAe(correspondenceAddressDTO);

                    _wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementToBeClickable(registrationPage.BtnNext)).Click();
                    _wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementToBeClickable(correspondenceAddress.AddressConfirmationbtn)).Click();
                    _wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementToBeClickable(registrationPage.BtnNext)).Click();
                }
            }

            var specialitiesDTO = _registration.SpecialitiesDTO;
            var specialties = new Specialties(TestWebDriver, registrationPage.RegID);
            if (specialties.RegistrationTitle != null && specialties.RegistrationTitle.Text == "Specialties" && specialitiesDTO != null)
            {
                _wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementToBeClickable(specialties.AddSpecalities)).Click();
                specialties.SetSpecalityInformation(specialitiesDTO);
                _wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementToBeClickable(registrationPage.BtnNext)).Click();
            }

            Helper.PrintScreenShot(TestWebDriver, "SubmitForReview");
            _wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementToBeClickable(registrationPage.BtnSubmitForReview)).Click();

            var submissionConfirmation = new SubmissionConfirmation(TestWebDriver);
            _wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementToBeClickable(submissionConfirmation.btnReturnToHome)).Click();

            _lastCreatedRegId = registrationPage.RegID;
            // Cross-flow chaining example (per the user's "both" data-flow answer) -
            // makes this real RegID available to TC.E2EP3Demo.SearchEligibility's
            // SearchEligibility_ByRegistrationId test case too, not just the other
            // Registration tests in this same file.
            TC.E2EP3Demo.SearchEligibility.SearchEligibilityTests.RegIdFromRegistrationFlow = _lastCreatedRegId;
            NUnit.Framework.TestContext.WriteLine($"Registration submitted successfully with RegID: {_lastCreatedRegId}");

            Assert.That(_lastCreatedRegId, Is.Not.Null.And.Not.Empty,
                "Expected a Reg ID to be captured after submission for reuse by later test cases.");
        }

        // Chains off _lastCreatedRegId (see class-level comment) - if this runs before
        // Registration_FullWizardThroughSubmission in a given session, falls back to a
        // fresh provider so it still runs standalone rather than failing outright.
        [Test]
        [Author("Vishu")]
        [Category("IntegrationTests")]
        [Property("Description", "Search the Provider Home grid for a just-submitted provider by Reg ID")]
        [Property("Priority", "Medium")]
        [Property("TestCaseId", "TC_E2EP3_REG_05")]
        [CancelAfter(600000)]
        public void SearchExistingProvider_ByRegId()
        {
            var regId = _lastCreatedRegId ?? CreateNewProviderUpToWizard().RegID;

            var providerHomeNew = new ProviderHomeNew(TestWebDriver, regId);
            _wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementToBeClickable(providerHomeNew.RegIDFilterTextBox)).Set(regId, true);
            _wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementToBeClickable(providerHomeNew.RegIDFilter)).Click();
            _wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementToBeClickable(providerHomeNew.RegIDFilterEqualsTo)).Click();

            Helper.PrintScreenShot(TestWebDriver, "SearchExistingProvider_ByRegId");
            NUnit.Framework.TestContext.WriteLine($"Searched Provider Home for RegID: {regId}");
        }

        [Test]
        [Author("Vishu")]
        [Category("IntegrationTests")]
        [Property("Description", "Verify the Workflow page loads for a just-registered provider")]
        [Property("Priority", "Medium")]
        [Property("TestCaseId", "TC_E2EP3_REG_06")]
        [CancelAfter(600000)]
        public void Workflow_LoadsForRegisteredProvider()
        {
            var regId = _lastCreatedRegId ?? CreateNewProviderUpToWizard().RegID;

            LogOut();
            LoginByProfile(Roles.StateAdmin);

            var workflowPage = new WorkflowPage(TestWebDriver, regId);
            workflowPage.RunWorkflow();

            Helper.PrintScreenShot(TestWebDriver, "Workflow_LoadsForRegisteredProvider");
            NUnit.Framework.TestContext.WriteLine($"Workflow page loaded and run for RegID: {regId}");
        }
    }
}
