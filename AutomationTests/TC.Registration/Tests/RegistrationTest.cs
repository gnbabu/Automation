using NUnit.Framework;
using TC.ProviderDataEntry.Models;
using TC.ProviderDataEntry.Utilities;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using Selenium.BaseComponents;
using Selenium.BaseComponents.Pages;
using Selenium.BaseComponents.Utilities;
using SeleniumExtensions.Configurations;
using SeleniumExtensions.Extensions;
using System.Data;
using TC.ProviderDataEntry.Pages;
using TC.ProviderDataEntry.Pages.Registration;

namespace TC.ProviderDataEntry.Tests
{
    [TestFixture("TechAdmin")] // ProviderAdmin

    public class RegistrationTest : BaseFeatureFixture
    {


        public RegistrationTest(string profile) : base(profile)
        {

        }


        [Test]
        [Author("Vishnuvardhan Reddy")]
        [Category("IntegrationTests")]
        [System.ComponentModel.Description("Verify new provider creation flow")]
        [CustomRetry(5)]
        [CancelAfter(600000)]
        public void CreateNewProvider()
        {
            string? filePath = (default(string));

            var param = NUnit.Framework.TestContext.Parameters;

            string testName = NUnit.Framework.TestContext.CurrentContext.Test.Name;

            #region Commented Code

            TC.ProviderDataEntry.Models.Registration registration = (TC.ProviderDataEntry.Models.Registration)DataRepository.GetAutomationData("Registration"); // DataRepository.GetRegistrationData();

            WebDriverWait wait = new WebDriverWait(TestWebDriver, TimeSpan.FromSeconds(30));
            try
            {
                NewProviderPage newProviderPage = new NewProviderPage(TestWebDriver);
                newProviderPage.NewProviderBtn.Click();
                newProviderPage.StandardType.Click();
                newProviderPage.IndividualType.Click();

                if (!newProviderPage.isNewProviderloaded())
                {
                    NUnit.Framework.Assert.Fail("New Provider page is not loaded.");
                }

                NewProviderDTO newProviderDTO = registration.NewProviderDTO; //RegistrationData.GetNewProviderDTO();
                newProviderPage.SetNewProviderPage(newProviderDTO);

                newProviderPage.ProviderSavebtn.Click();

                newProviderPage.WaitUntilTaxonomyVisible();

                newProviderPage.SetTaxonomy(newProviderDTO.Taxonomy);

                wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementToBeClickable(newProviderPage.ProviderSavebtn)).Click();
                newProviderPage.WaitUntilLoaderDisappear();

                RegistrationPage registrationPage = new RegistrationPage(TestWebDriver);

                #region Provider Information

                if (!registrationPage.isRequiredMessageAppear)
                {
                    Helper.PrintScreenShot(TestWebDriver, "ProviderInfo");
                    wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementToBeClickable(registrationPage.BtnNext)).Click();
                }
                else
                {
                    registrationPage.SetPracticeType("GENERAL HOSPITAL", wait);
                    registrationPage.SetOwnershipType("COUNTY (GOVT)", wait);
                    registrationPage.SetResident("No");
                    Helper.PrintScreenShot(TestWebDriver, "ProviderInfo");
                    wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementToBeClickable(registrationPage.BtnNext)).Click();
                }



                #endregion

                #region Primary Contact Information

                ProviderContactInformation providerContactInformation = new ProviderContactInformation(TestWebDriver, registrationPage.RegID);

                if (providerContactInformation.RegistrationTitle != null && providerContactInformation.RegistrationTitle.Text == "Primary Contact Information")
                {
                    if (!registrationPage.isRequiredMessageAppear)
                    {
                        Helper.PrintScreenShot(TestWebDriver, "PrimaryContactInformation");
                        wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementToBeClickable(registrationPage.BtnNext)).Click();
                    }
                    else
                    {
                        ProviderContactInfo providerContactInfo = registration.ProviderContactInfo; //RegistrationData.GetproviderContactInfo();
                        providerContactInformation.SetProviderContactInformation(providerContactInfo);
                        wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementToBeClickable(registrationPage.BtnNext)).Click();
                        wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementToBeClickable(providerContactInformation.AddressConfirmationbtn)).Click();
                        Helper.PrintScreenShot(TestWebDriver, "PrimaryContactInformation");
                        wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementToBeClickable(registrationPage.BtnNext)).Click();
                    }
                }

                #endregion

                #region Credentialing Contact

                CredentailContactDTO credentailContactDTO = null;

                if (providerContactInformation.RegistrationTitle != null && providerContactInformation.RegistrationTitle.Text == "Credentialing Contact")
                {
                    if (registrationPage.RequiredMessage != null &&
                        registrationPage.RequiredMessage.Text == "This is not a required section. To skip this section click on Next button." && credentailContactDTO == null)
                    {
                        Helper.PrintScreenShot(TestWebDriver, "CredentialingContact");
                        registrationPage.BtnNext.Click();
                    }
                    else
                    {
                        // Fill Credential Contact information.
                        Helper.PrintScreenShot(TestWebDriver, "CredentialingContact");
                        registrationPage.BtnNext.Click();
                    }
                }

                #endregion

                #region Primary Address Service
                PrimaryAddressService primaryAddressService = new PrimaryAddressService(TestWebDriver, registrationPage.RegID);
                if (primaryAddressService.RegistrationTitle != null && primaryAddressService.RegistrationTitle.Text == "Primary Service Address")
                {
                    if (!registrationPage.isRequiredMessageAppear)
                    {
                        Helper.PrintScreenShot(TestWebDriver, "PrimaryAddressService");
                        wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementToBeClickable(registrationPage.BtnNext)).Click();
                    }
                    else
                    {
                        PrimaryAddressServiceDTO primaryAddressServiceDTO = registration.PrimaryAddressServiceDTO; //RegistrationData.GetPrimaryAddressServiceDTO();
                        primaryAddressService.SetPrimaryAddressInformation(primaryAddressServiceDTO);
                        wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementToBeClickable(registrationPage.BtnNext)).Click();
                        wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementToBeClickable(primaryAddressService.AddressConfirmationbtn)).Click();
                        Helper.PrintScreenShot(TestWebDriver, "PrimaryAddressService");
                        wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementToBeClickable(registrationPage.BtnNext)).Click();
                    }
                }
                #endregion

                #region Billing & Payment Address
                BillingAndPaymentAddress billingAndPaymentAddress = new BillingAndPaymentAddress(TestWebDriver, registrationPage.RegID);
                if (billingAndPaymentAddress.RegistrationTitle != null && billingAndPaymentAddress.RegistrationTitle.Text == "Billing & Payment Address")
                {
                    if (!registrationPage.isRequiredMessageAppear)
                    {
                        Helper.PrintScreenShot(TestWebDriver, "BillingAndPaymentAddress");
                        wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementToBeClickable(registrationPage.BtnNext)).Click();
                    }
                    else
                    {
                        BillingAndPaymentAddressDTO billingAndPaymentAddressDTO = registration.BillingAndPaymentAddressDTO; // RegistrationData.GetBillingAndPaymentAddressDTO();

                        if (billingAndPaymentAddressDTO.isSameAsPracticeLocation == "Yes")
                        {
                            wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementToBeClickable(billingAndPaymentAddress.SameAsPracticeLocation)).Click();
                        }
                        else
                        {
                            billingAndPaymentAddress.SetBillingAddressInformation(billingAndPaymentAddressDTO);
                        }
                        wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementToBeClickable(registrationPage.BtnNext)).Click();
                        wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementToBeClickable(billingAndPaymentAddress.AddressConfirmationbtn)).Click();
                        Helper.PrintScreenShot(TestWebDriver, "BillingAndPaymentAddress");
                        wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementToBeClickable(registrationPage.BtnNext)).Click();
                    }
                }
                #endregion

                #region Correspondence Address
                CorrespondenceAddressDTO correspondenceAddressDTO = registration.CorrespondenceAddressDTO; //RegistrationData.GetCorrespondenceAddressDTO();
                CorrespondenceAddress correspondenceAddress = new CorrespondenceAddress(TestWebDriver, registrationPage.RegID);
                if (correspondenceAddress.RegistrationTitle != null && correspondenceAddress.RegistrationTitle.Text == "Correspondence Address")
                {
                    if (!registrationPage.isRequiredMessageAppear)
                    {
                        Helper.PrintScreenShot(TestWebDriver, "CorrespondenceAddress");
                        wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementToBeClickable(registrationPage.BtnNext)).Click();
                    }
                    else
                    {
                        if (correspondenceAddressDTO.isSameAsPracticeLocation == "Yes")
                        {
                            wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementToBeClickable(correspondenceAddress.SameAsPracticeLocation)).Click();
                        }
                        else
                        {
                            correspondenceAddress.SetCorrespnodencAe(correspondenceAddressDTO);
                        }
                        wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementToBeClickable(registrationPage.BtnNext)).Click();
                        wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementToBeClickable(correspondenceAddress.AddressConfirmationbtn)).Click();
                        Helper.PrintScreenShot(TestWebDriver, "CorrespondenceAddress");
                        wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementToBeClickable(registrationPage.BtnNext)).Click();
                    }
                }

                #endregion

                #region Other Service Locations

                OtherServiceLocationsDTO OtherServiceLocationsDTO = registration.OtherServiceLocationsDTO; // RegistrationData.GetOtherServiceLocationsDTO();
                OtherServiceLocations otherServiceLocations = new OtherServiceLocations(TestWebDriver, registrationPage.RegID);
                if (otherServiceLocations.RegistrationTitle != null && otherServiceLocations.RegistrationTitle.Text == "Other Service Locations")
                {
                    if (!registrationPage.isRequiredMessageAppear && registrationPage.RequiredMessage != null && registrationPage.RequiredMessage.Text == "This is not a required section. To skip this section click on Next button." && OtherServiceLocationsDTO == null)
                    {
                        Helper.PrintScreenShot(TestWebDriver, "OtherServiceLocations");
                        wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementToBeClickable(registrationPage.BtnNext)).Click();
                    }
                    else
                    {
                        wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementToBeClickable(otherServiceLocations.AddOtherLocationBtn)).Click();
                        otherServiceLocations.SetOtherServiceLocations(OtherServiceLocationsDTO);
                        wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementToBeClickable(registrationPage.BtnNext)).Click();
                        wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementToBeClickable(otherServiceLocations.AddressConfirmationbtn)).Click();
                        Helper.PrintScreenShot(TestWebDriver, "OtherServiceLocations");
                        wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementToBeClickable(registrationPage.BtnNext)).Click();
                    }
                }

                #endregion

                #region Address 1099

                try
                {
                    Adress1099 address1099Data = registration.Adress1099; // RegistrationData.GetAddress1099Information();
                    Address1099 address1099 = new Address1099(TestWebDriver, registrationPage.RegID);
                    if (address1099.RegistrationTitle != null && address1099.RegistrationTitle.Text == "1099 Address")
                    {
                        if (!registrationPage.isRequiredMessageAppear && registrationPage.RequiredMessage != null && registrationPage.RequiredMessage.Text == "This is not a required section. To skip this section click on Next button." && address1099 == null)
                        {
                            Helper.PrintScreenShot(TestWebDriver, "Address1099");
                            wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementToBeClickable(registrationPage.BtnNext)).Click();
                        }
                        else
                        {
                            if (address1099Data.SameBillingLocation == "Yes")
                            {
                                wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementToBeClickable(address1099.SameBillingLocation)).Click();
                            }
                            else if (address1099Data.SamePracticeLocation == "Yes")
                            {
                                wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementToBeClickable(address1099.SamePracticeLocation)).Click();
                            }
                            else
                            {
                                address1099.SetAddress1099Information(address1099Data);
                                wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementToBeClickable(registrationPage.BtnNext)).Click();
                                wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementToBeClickable(address1099.AddressConfirmationbtn)).Click();
                                Helper.PrintScreenShot(TestWebDriver, "Address1099");
                                wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementToBeClickable(registrationPage.BtnNext)).Click();
                            }

                        }
                    }
                }
                catch (Exception ex)
                {
                    TestContext.Error.WriteLine($"Address 1099 section failed: {ex.Message}");
                    throw;
                }

                #endregion

                #region Home Office Address

                try
                {
                    HomeOfficeAddressDTO homeOfficeAddressDTO = registration.HomeOfficeAddressDTO;  //RegistrationData.GetHomeOfficeAddressInformation();
                    HomeOfficeAddress homeOfficeAddress = new HomeOfficeAddress(TestWebDriver, registrationPage.RegID);
                    if (homeOfficeAddress.RegistrationTitle != null && homeOfficeAddress.RegistrationTitle.Text == "Home Office Address")
                    {
                        if (!registrationPage.isRequiredMessageAppear && registrationPage.RequiredMessage != null && registrationPage.RequiredMessage.Text == "This is not a required section. To skip this section click on Next button." && homeOfficeAddressDTO == null)
                        {
                            Helper.PrintScreenShot(TestWebDriver, "HomeOfficeAddress");
                            wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementToBeClickable(registrationPage.BtnNext)).Click();
                        }
                        else
                        {
                            if (homeOfficeAddressDTO.SamePracticeLocation == "Yes")
                            {
                                wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementToBeClickable(homeOfficeAddress.SamePracticeLocation)).Click();
                            }
                            else
                            {
                                homeOfficeAddress.SetHomeOfficeInformation(homeOfficeAddressDTO);
                                wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementToBeClickable(registrationPage.BtnNext)).Click();
                                wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementToBeClickable(homeOfficeAddress.AddressConfirmationbtn)).Click();
                                Helper.PrintScreenShot(TestWebDriver, "HomeOfficeAddress");
                                wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementToBeClickable(registrationPage.BtnNext)).Click();
                            }

                        }
                    }
                }
                catch (Exception ex)
                {
                    TestContext.Error.WriteLine($"Home Office Address section failed: {ex.Message}");
                    throw;
                }

                #endregion

                #region Specalities

                try
                {
                    SpecialitiesDTO specialitiesDTO = registration.SpecialitiesDTO; //  RegistrationData.GetSpecialitiesInformation();
                    Specialties specialties = new Specialties(TestWebDriver, registrationPage.RegID);
                    if (specialties.RegistrationTitle != null && specialties.RegistrationTitle.Text == "Specialties")
                    {
                        if (!registrationPage.isRequiredMessageAppear && registrationPage.RequiredMessage != null && registrationPage.RequiredMessage.Text == "This is not a required section. To skip this section click on Next button." && specialitiesDTO == null)
                        {
                            Helper.PrintScreenShot(TestWebDriver, "Specalities");
                            wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementToBeClickable(registrationPage.BtnNext)).Click();
                        }
                        else
                        {
                            wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementToBeClickable(specialties.AddSpecalities)).Click();
                            specialties.SetSpecalityInformation(specialitiesDTO);
                            Helper.PrintScreenShot(TestWebDriver, "Specalities");
                            wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementToBeClickable(registrationPage.BtnNext)).Click();

                        }
                    }
                }
                catch (Exception ex)
                {
                    TestContext.Error.WriteLine($"Specialties section failed: {ex.Message}");
                    throw;
                }

                #endregion

                #region Taxonomy Information

                try
                {
                    TaxonomiesDTO taxonomiesDTO = registration.TaxonomiesDTO; // RegistrationData.GetTaxonomiesInformation();
                    Taxonomies taxonomies = new Taxonomies(TestWebDriver, registrationPage.RegID);
                    if (taxonomies.RegistrationTitle != null && taxonomies.RegistrationTitle.Text == "Taxonomies")
                    {
                        if (!registrationPage.isRequiredMessageAppear && registrationPage.RequiredMessage != null && registrationPage.RequiredMessage.Text == "This is not a required section. To skip this section click on Next button." && taxonomies == null)
                        {
                            Helper.PrintScreenShot(TestWebDriver, "TaxonomyInformation");
                            wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementToBeClickable(registrationPage.BtnNext)).Click();
                        }
                        else
                        {
                            if (taxonomies.TaxonomyTable != null && Convert.ToBoolean(taxonomiesDTO.AddNewtaxonomy) == false)
                            {
                                wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementToBeClickable(registrationPage.BtnNext)).Click();
                            }
                            else
                            {
                                wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementToBeClickable(taxonomies.AddTaxonomyBtn)).Click();
                                taxonomies.SetTaxonomyInformation(taxonomiesDTO);
                                Helper.PrintScreenShot(TestWebDriver, "TaxonomyInformation");
                                wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementToBeClickable(registrationPage.BtnNext)).Click();
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    TestContext.Error.WriteLine($"Taxonomy Information section failed: {ex.Message}");
                    throw;
                }

                #endregion

                #region Professional License

                try
                {
                    ProfessionalLicenses professionalLicenseDTO = registration.ProfessionalLicenses; // RegistrationData.GetProfessionalLicenses();
                    ProfessionalLicense professionalLicense = new ProfessionalLicense(TestWebDriver, registrationPage.RegID);
                    if (professionalLicense.RegistrationTitle != null && professionalLicense.RegistrationTitle.Text == "Professional Licenses")
                    {
                        if (!registrationPage.isRequiredMessageAppear && registrationPage.RequiredMessage != null && registrationPage.RequiredMessage.Text == "This is not a required section. To skip this section click on Next button." && professionalLicense == null)
                        {
                            Helper.PrintScreenShot(TestWebDriver, "ProfessionalLicense");
                            wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementToBeClickable(registrationPage.BtnNext)).Click();
                        }
                        else
                        {
                            wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementToBeClickable(professionalLicense.AddLicenseBtn)).Click();
                            professionalLicense.SetProfessionalLicense(professionalLicenseDTO);
                            Helper.PrintScreenShot(TestWebDriver, "ProfessionalLicense");
                            wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementToBeClickable(registrationPage.BtnNext)).Click();
                        }
                    }
                }
                catch (Exception ex)
                {
                    TestContext.Error.WriteLine($"Professional License section failed: {ex.Message}");
                    throw;
                }
                #endregion

                #region Board Certification

                try
                {
                    BoardCertificateDTO boardCertificateDTO = registration.BoardCertificateDTO; // RegistrationData.GetBoardCertificateDTO();
                    BoardCertificate boardCertificate = new BoardCertificate(TestWebDriver, registrationPage.RegID);
                    if (boardCertificate.RegistrationTitle != null && boardCertificate.RegistrationTitle.Text == "Board Certification")
                    {
                        if (registrationPage.RequiredMessage != null && registrationPage.RequiredMessage.Text == "This is not a required section. To skip this section click on Next button." && Convert.ToBoolean(boardCertificateDTO.AddNewCertificate) == false)
                        {
                            Helper.PrintScreenShot(TestWebDriver, "BoardCertification");
                            wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementToBeClickable(registrationPage.BtnNext)).Click();
                        }
                        else
                        {
                            wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementToBeClickable(boardCertificate.AddBoardCertificate)).Click();
                            Helper.PrintScreenShot(TestWebDriver, "BoardCertification");
                            wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementToBeClickable(registrationPage.BtnNext)).Click();
                        }
                    }
                }
                catch (Exception ex)
                {
                    TestContext.Error.WriteLine($"Board Certification section failed: {ex.Message}");
                    throw;
                }

                #endregion

                #region CLIA Certifications
                try
                {
                    CLIACertificationsDTO cLIACertificationsDTO = registration.CLIACertificationsDTO; //RegistrationData.GetCLIACertificationsDTO();
                    CLIACertifications cLIACertifications = new CLIACertifications(TestWebDriver, registrationPage.RegID);
                    if (cLIACertifications.RegistrationTitle != null && cLIACertifications.RegistrationTitle.Text == "CLIA Certifications")
                    {
                        if (registrationPage.RequiredMessage != null && registrationPage.RequiredMessage.Text == "This is not a required section. To skip this section click on Next button." && Convert.ToBoolean(cLIACertificationsDTO.AddNewCLIACertificate) == false)
                        {
                            Helper.PrintScreenShot(TestWebDriver, "CLIACertifications");
                            wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementToBeClickable(registrationPage.BtnNext)).Click();
                        }
                        else
                        {
                            wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementToBeClickable(cLIACertifications.AddCLIACertification)).Click();
                            Helper.PrintScreenShot(TestWebDriver, "CLIACertifications");
                            wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementToBeClickable(registrationPage.BtnNext)).Click();
                        }
                    }
                }
                catch (Exception ex)
                {
                    TestContext.Error.WriteLine($"CLIA Certifications section failed: {ex.Message}");
                    throw;
                }
                #endregion

                #region MediCare Number
                try
                {
                    MediCareNumberDTO mediCareNumberDTO = registration.MediCareNumberDTO; // RegistrationData.GetMediCareNumberDTO();
                    MediCareNumber mediCareNumber = new MediCareNumber(TestWebDriver, registrationPage.RegID);
                    if (mediCareNumber.RegistrationTitle != null && mediCareNumber.RegistrationTitle.Text == "Medicare Number")
                    {
                        if (registrationPage.RequiredMessage != null && registrationPage.RequiredMessage.Text == "This is not a required section. To skip this section click on Next button." && Convert.ToBoolean(mediCareNumberDTO.AddNewMediCareNumber) == false)
                        {
                            Helper.PrintScreenShot(TestWebDriver, "MediCareNumber");
                            wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementToBeClickable(registrationPage.BtnNext)).Click();
                        }
                        else
                        {
                            wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementToBeClickable(mediCareNumber.AddMediCareNumber)).Click();
                            Helper.PrintScreenShot(TestWebDriver, "MediCareNumber");
                            wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementToBeClickable(registrationPage.BtnNext)).Click();
                        }
                    }
                }
                catch (Exception ex)
                {
                    TestContext.Error.WriteLine($"Medicare Number section failed: {ex.Message}");
                    throw;
                }
                #endregion

                #region Group Facility & Hospital Affiliations
                try
                {
                    GroupFacilityHospital groupFacilityDTO = registration.GroupFacilityHospital; // RegistrationData.GetGroupFacilityHospitalAffiliationsDTO();
                    GroupFacilityHospitalAffiliations groupFacility = new GroupFacilityHospitalAffiliations(TestWebDriver, registrationPage.RegID);
                    if (groupFacility.RegistrationTitle != null && groupFacility.RegistrationTitle.Text == "Group, Facility & Hospital Affiliations (Individual)")
                    {
                        if (registrationPage.RequiredMessage != null && registrationPage.RequiredMessage.Text == "This is not a required section. To skip this section click on Next button." && Convert.ToBoolean(groupFacilityDTO.AddFacility) == false)
                        {
                            Helper.PrintScreenShot(TestWebDriver, "GroupFacilityANDHospitalAffiliations");
                            wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementToBeClickable(registrationPage.BtnNext)).Click();
                        }
                        else
                        {
                            wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementToBeClickable(groupFacility.AddGroupAffiliations)).Click();
                            Helper.PrintScreenShot(TestWebDriver, "GroupFacilityANDHospitalAffiliations");
                            wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementToBeClickable(registrationPage.BtnNext)).Click();
                        }
                    }
                }
                catch (Exception ex)
                {
                    TestContext.Error.WriteLine($"Group Facility & Hospital Affiliations section failed: {ex.Message}");
                    throw;
                }
                #endregion

                #region MCP Affiliation
                try
                {
                    MCPAffiliationDTO mcpAffiliationDTO = registration.MCPAffiliationDTO; //RegistrationData.GetMCPAffiliationDTO();
                    MCPAffiliation mcpAffiliation = new MCPAffiliation(TestWebDriver, registrationPage.RegID);
                    if (mcpAffiliation.RegistrationTitle != null && mcpAffiliation.RegistrationTitle.Text == "MCP Affiliation")
                    {
                        if (registrationPage.RequiredMessage != null && registrationPage.RequiredMessage.Text == "This is not a required section. To skip this section click on Next button." && Convert.ToBoolean(mcpAffiliationDTO.AddMCPAffiliation) == false)
                        {
                            Helper.PrintScreenShot(TestWebDriver, "MCPAffiliation");
                            wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementToBeClickable(registrationPage.BtnNext)).Click();
                        }
                        else
                        {
                            wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementToBeClickable(mcpAffiliation.ManagedCarePlans_No)).Click();
                            Helper.PrintScreenShot(TestWebDriver, "MCPAffiliation");
                            wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementToBeClickable(registrationPage.BtnNext)).Click();
                        }
                    }
                }
                catch (Exception ex)
                {
                    TestContext.Error.WriteLine($"MCP Affiliation section failed: {ex.Message}");
                    throw;
                }
                #endregion

                #region State CDS Number
                try
                {
                    StateCDSNumberDTO stateCDSNumberDTO = registration.StateCDSNumberDTO; //RegistrationData.GetStateCDSNumberDTO();
                    StateCDSNumber stateCDSNumber = new StateCDSNumber(TestWebDriver, registrationPage.RegID);
                    if (stateCDSNumber.RegistrationTitle != null && stateCDSNumber.RegistrationTitle.Text == "State CDS Number")
                    {
                        if (registrationPage.RequiredMessage != null && registrationPage.RequiredMessage.Text == "This is not a required section. To skip this section click on Next button." && Convert.ToBoolean(stateCDSNumberDTO.AddStateCDSNumber) == false)
                        {
                            Helper.PrintScreenShot(TestWebDriver, "StateCDSNumber");
                            wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementToBeClickable(registrationPage.BtnNext)).Click();
                        }
                        else
                        {
                            wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementToBeClickable(stateCDSNumber.AddStateCDSNumber)).Click();
                            Helper.PrintScreenShot(TestWebDriver, "StateCDSNumber");
                            wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementToBeClickable(registrationPage.BtnNext)).Click();
                        }
                    }
                }
                catch (Exception ex)
                {
                    TestContext.Error.WriteLine($"State CDS Number section failed: {ex.Message}");
                    throw;
                }
                #endregion

                #region Federal DEA Registration
                try
                {
                    FederalDEARegistrationDTO federalDEARegistrationDTO = registration.FederalDEARegistrationDTO; // RegistrationData.GetFederalDEARegistrationDTO();
                    FederalDEARegistration federalDEARegistration = new FederalDEARegistration(TestWebDriver, registrationPage.RegID);
                    if (federalDEARegistration.RegistrationTitle != null && federalDEARegistration.RegistrationTitle.Text == "Federal DEA Registration")
                    {
                        if (registrationPage.RequiredMessage != null && registrationPage.RequiredMessage.Text == "This is not a required section. To skip this section click on Next button.")
                        {
                            Helper.PrintScreenShot(TestWebDriver, "FederalDEARegistration");
                            wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementToBeClickable(registrationPage.BtnNext)).Click();
                        }
                        else
                        {
                            federalDEARegistration.SetFederalDEARegistrationInformation(federalDEARegistrationDTO);
                            Helper.PrintScreenShot(TestWebDriver, "FederalDEARegistration");
                            wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementToBeClickable(registrationPage.BtnNext)).Click();
                        }
                    }
                }
                catch (Exception ex)
                {
                    TestContext.Error.WriteLine($"Federal DEA Registration section failed: {ex.Message}");
                    throw;
                }
                #endregion

                #region Professional Liability Insurance
                try
                {
                    ProfessionalLiabilityInsurance professionalLiabilityInsuranceDTO = registration.ProfessionalLiabilityInsurance; //  RegistrationData.GetProfessionalLiabilityInsuranceDTO();
                    ProfessionalLiabilityInsurancePage professionalLiabilityInsurance = new ProfessionalLiabilityInsurancePage(TestWebDriver, registrationPage.RegID);
                    if (professionalLiabilityInsurance.RegistrationTitle != null && professionalLiabilityInsurance.RegistrationTitle.Text == "Professional Liability Insurance")
                    {
                        if (registrationPage.RequiredMessage != null && registrationPage.RequiredMessage.Text == "This is not a required section. To skip this section click on Next button.")
                        {
                            Helper.PrintScreenShot(TestWebDriver, "ProfessionalLiabilityInsurance");
                            wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementToBeClickable(registrationPage.BtnNext)).Click();
                        }
                        else
                        {
                            wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementToBeClickable(professionalLiabilityInsurance.AddNewInsurance)).Click();
                            professionalLiabilityInsurance.SetProfessionalLiabilityInsurance(professionalLiabilityInsuranceDTO);
                            Helper.PrintScreenShot(TestWebDriver, "ProfessionalLiabilityInsurance");
                            wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementToBeClickable(registrationPage.BtnNext)).Click();
                        }
                    }
                }
                catch (Exception ex)
                {
                    TestContext.Error.WriteLine($"Professional Liability Insurance section failed: {ex.Message}");
                    throw;
                }
                #endregion

                #region Education
                try
                {
                    EducationDTO educationDTO = registration.EducationDTO;// RegistrationData.GetEducationDTO();
                    Education education = new Education(TestWebDriver, registrationPage.RegID);
                    if (education.RegistrationTitle != null && education.RegistrationTitle.Text == "Education")
                    {
                        if (registrationPage.RequiredMessage != null && registrationPage.RequiredMessage.Text == "This is not a required section. To skip this section click on Next button.")
                        {
                            Helper.PrintScreenShot(TestWebDriver, "Education");
                            wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementToBeClickable(registrationPage.BtnNext)).Click();
                        }
                        else
                        {
                            wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementToBeClickable(education.AddEducation)).Click();
                            education.SetEducationDetails(educationDTO);
                            Helper.PrintScreenShot(TestWebDriver, "Education");
                            wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementToBeClickable(registrationPage.BtnNext)).Click();
                        }
                    }
                }
                catch (Exception ex)
                {
                    TestContext.Error.WriteLine($"Education section failed: {ex.Message}");
                    throw;
                }
                #endregion

                #region MalPractice Claims History
                try
                {
                    MalPracticeClaimsHistoryDTO malPracticeClaimsHistoryDTO = registration.MalPracticeClaimsHistoryDTO; // RegistrationData.GetMalPracticeClaimsHistoryDTO();
                    MalPracticeClaimsHistory malPracticeClaimsHistory = new MalPracticeClaimsHistory(TestWebDriver, registrationPage.RegID);
                    if (malPracticeClaimsHistory.RegistrationTitle != null && malPracticeClaimsHistory.RegistrationTitle.Text == "Malpractice Claims History")
                    {
                        if (registrationPage.RequiredMessage != null && registrationPage.RequiredMessage.Text == "This is not a required section. To skip this section click on Next button.")
                        {
                            Helper.PrintScreenShot(TestWebDriver, "MalPracticeClaimsHistory");
                            wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementToBeClickable(registrationPage.BtnNext)).Click();
                        }
                        else
                        {
                            wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementToBeClickable(malPracticeClaimsHistory.AddNewClaims)).Click();
                            malPracticeClaimsHistory.SetMalPracticeClaimsHistory(malPracticeClaimsHistoryDTO);
                            Helper.PrintScreenShot(TestWebDriver, "MalPracticeClaimsHistory");
                            wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementToBeClickable(registrationPage.BtnNext)).Click();
                        }
                    }
                }
                catch (Exception ex)
                {
                    TestContext.Error.WriteLine($"MalPractice Claims History section failed: {ex.Message}");
                    throw;
                }
                #endregion

                #region Work History
                try
                {
                    WorkHistoryDTO workHistoryDTO = registration.WorkHistoryDTO; // RegistrationData.GetWorkHistoryDTO();
                    WorkHistory workHistory = new WorkHistory(TestWebDriver, registrationPage.RegID);
                    if (workHistory.RegistrationTitle != null && workHistory.RegistrationTitle.Text == "Work History")
                    {
                        if (registrationPage.RequiredMessage != null && registrationPage.RequiredMessage.Text == "This is not a required section. To skip this section click on Next button.")
                        {
                            Helper.PrintScreenShot(TestWebDriver, "WorkHistory");
                            wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementToBeClickable(registrationPage.BtnNext)).Click();
                        }
                        else
                        {
                            wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementToBeClickable(workHistory.AddWorkHistory)).Click();
                            workHistory.SetWorkHistory(workHistoryDTO);
                            wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementToBeClickable(workHistory.AddGapHistory)).Click();
                            workHistory.SetGapHistoryDetails(workHistoryDTO);
                            Helper.PrintScreenShot(TestWebDriver, "WorkHistory");
                            wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementToBeClickable(registrationPage.BtnNext)).Click();
                        }
                    }
                }
                catch (Exception ex)
                {
                    TestContext.Error.WriteLine($"Work History section failed: {ex.Message}");
                    throw;
                }
                #endregion

                #region W9 Form

                try
                {
                    W9FormDTO w9FormDTO = registration.W9FormDTO; // RegistrationData.GetW9FormDTO();

                    filePath = w9FormDTO.FilePath;

                    W9Form w9Form = new W9Form(TestWebDriver, registrationPage.RegID);
                    if (w9Form.RegistrationTitle != null && w9Form.RegistrationTitle.Text == "W9 Form")
                    {
                        if (registrationPage.RequiredMessage != null && registrationPage.RequiredMessage.Text == "This is not a required section. To skip this section click on Next button.")
                        {
                            Helper.PrintScreenShot(TestWebDriver, "W9Form");
                            wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementToBeClickable(registrationPage.BtnNext)).Click();
                        }
                        else
                        {
                            try
                            {
                                w9Form.SetW9FormDetails(w9FormDTO);
                                wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementToBeClickable(registrationPage.BtnNext)).Click();
                            }
                            catch (Exception ex)
                            {
                                TestContext.Error.WriteLine($"W9 Form failed (retry): {ex.Message}");
                                w9Form.SetW9FormDetails(w9FormDTO);
                                wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementToBeClickable(registrationPage.BtnNext)).Click();
                            }

                            // w9Form.WaitUntilImgProgressDisappear();

                            //wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementToBeClickable(w9Form.FileDownload));
                            //Console.WriteLine("W9 Form is Executed Succesfully");
                            //wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementToBeClickable(w9Form.ETFBankingImage)).Click();
                            //Console.WriteLine("EFT Banking button is Clicked Succesfully");
                            //wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementToBeClickable(w9Form.PopUp_Confirmation)).Click();
                            //Console.WriteLine("Pop up Button is clicked");
                            Helper.PrintScreenShot(TestWebDriver, "W9Form");
                        }
                    }
                }
                catch (Exception ex)
                {
                    TestContext.Error.WriteLine($"W9 Form section failed: {ex.Message}");
                    throw;
                }
                #endregion

                #region EFT Banking Information
                try
                {
                    EFTBankingDTO eFTBankingDTO = registration.EFTBankingDTO; // RegistrationData.GetEFTBankingDTO();
                    EFTBanking eFTBanking = new EFTBanking(TestWebDriver, registrationPage.RegID);
                    if (eFTBanking.RegistrationTitle != null && eFTBanking.RegistrationTitle.Text == "EFT Banking Information")
                    {
                        if (registrationPage.RequiredMessage != null && registrationPage.RequiredMessage.Text == "This is not a required section. To skip this section click on Next button." && Convert.ToBoolean(eFTBankingDTO.AddDateToPage) == false)
                        {
                            Helper.PrintScreenShot(TestWebDriver, "EFTBankingInformation");
                            wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementToBeClickable(registrationPage.BtnNext)).Click();
                        }
                        else
                        {
                            eFTBanking.SelectEFTBankingOptions(eFTBankingDTO);
                            if (eFTBankingDTO.SupplementalPoolPayments == "Yes")
                            {
                                wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementToBeClickable(eFTBanking.AddBankingInformation)).Click();
                                eFTBanking.SetBankingInformation(eFTBankingDTO);

                                wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementToBeClickable(eFTBanking.AddEFTContact)).Click();
                                eFTBanking.SetEFTInformation(eFTBankingDTO);
                                eFTBanking.ConfirmInformation.Click();
                                Helper.PrintScreenShot(TestWebDriver, "EFTBankingInformation");
                                wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementToBeClickable(registrationPage.BtnNext)).Click();
                            }
                            else
                            {
                                Helper.PrintScreenShot(TestWebDriver, "EFTBankingInformation");
                                wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementToBeClickable(registrationPage.BtnNext)).Click();
                            }

                        }
                    }
                }
                catch (Exception ex)
                {
                    TestContext.Error.WriteLine($"EFT Banking Information section failed: {ex.Message}");
                    throw;
                }
                #endregion

                #region Required Documents
                try
                {
                    RequiredDocumentsDTO requiredDocumentsDTO = registration.RequiredDocumentsDTO; // RegistrationData.GetRequiredDocumentsDTO();
                    RequiredDocuments requiredDocuments = new RequiredDocuments(TestWebDriver, registrationPage.RegID);
                    if (requiredDocuments.RegistrationTitle != null && requiredDocuments.RegistrationTitle.Text == "Required Documents")
                    {
                        if (registrationPage.RequiredMessage != null && registrationPage.RequiredMessage.Text == "This is not a required section. To skip this section click on Next button." && Convert.ToBoolean(requiredDocumentsDTO.AddDetailsOnPage) == false)
                        {
                            Helper.PrintScreenShot(TestWebDriver, "RequiredDocuments");
                            wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementToBeClickable(registrationPage.BtnNext)).Click();
                        }
                        else
                        {
                            // requiredDocuments.SetRequiredDocument(requiredDocumentsDTO);
                            Helper.PrintScreenShot(TestWebDriver, "RequiredDocuments");
                            wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementToBeClickable(registrationPage.BtnNext)).Click();
                        }
                    }
                }
                catch (Exception ex)
                {
                    TestContext.Error.WriteLine($"Required Documents section failed: {ex.Message}");
                    throw;
                }
                #endregion

                #region Agreements
                try
                {
                    AgreementsDTO agreementsDTO = registration.AgreementsDTO; // RegistrationData.GetAgreementsDTO();
                    Agreements agreements = new Agreements(TestWebDriver, registrationPage.RegID);
                    if (agreements.RegistrationTitle != null && agreements.RegistrationTitle.Text == "Agreements")
                    {
                        if (registrationPage.RequiredMessage != null && registrationPage.RequiredMessage.Text == "This is not a required section. To skip this section click on Next button.")
                        {
                            Helper.PrintScreenShot(TestWebDriver, "Agreements");
                            wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementToBeClickable(registrationPage.BtnNext)).Click();
                        }
                        else
                        {
                            agreements.SetAgreemntsInformationForAnestesia(agreementsDTO);
                            wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementToBeClickable(registrationPage.BtnNext)).Click();
                            wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementToBeClickable(agreements.ReviewModelPopup));
                            wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementToBeClickable(agreements.ReviewModelPopupOK)).Click();
                        }
                    }
                }
                catch (Exception ex)
                {
                    TestContext.Error.WriteLine($"Agreements section failed: {ex.Message}");
                    throw;
                }
                Helper.PrintScreenShot(TestWebDriver, "SubmitForReview");
                wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementToBeClickable(registrationPage.BtnSubmitForReview)).Click();

                SubmissionConfirmation submissionConfirmation = new SubmissionConfirmation(TestWebDriver);
                wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementToBeClickable(submissionConfirmation.btnReturnToHome)).Click();

                ProviderHomeNew providerHomeNew = new ProviderHomeNew(TestWebDriver, registrationPage.RegID);
                wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementToBeClickable(providerHomeNew.RegIDFilterTextBox)).Set(registrationPage.RegID, true);
                wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementToBeClickable(providerHomeNew.RegIDFilter)).Click();

                wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementToBeClickable(providerHomeNew.RegIDFilterEqualsTo)).Click();
                Helper.PrintScreenShot(TestWebDriver, "ProviderHome");


                #endregion
                string regId = registrationPage.RegID;



                LogOut();
                LoginByProfile(Roles.StateAdmin);

                WorkflowPage workflowPage = new WorkflowPage(TestWebDriver, regId);
                workflowPage.RunWorkflow();

                LoginByProfile(Roles.EnrollementSpecialist);

                MyQueue MyQueue = new MyQueue(TestWebDriver, registrationPage.RegID);

                MyQueue.WaitUntilElementIsVisible();

                MyQueue.SidebarMenu.Click();


                if (MyQueue.ProviderSearch != null)
                {
                    MyQueue.ProviderSearch.Click();
                }

                GroupReview GroupReview = new GroupReview(TestWebDriver, registrationPage.RegID);
                GroupReview.txtRegID.Set(regId);
                GroupReview.Search.Click();
                GroupReview.WaitUntilRowCountIsVisible();
                GroupReview.Review.Click();


                RegistrationPage Registration = new RegistrationPage(TestWebDriver);

                Registration.WaitUntilPageLoad();
                Registration.More.Click();
                Registration.SetAssignmentUser();

                Registration.SidebarMenu.Click();
                Registration.NavigateToMyQueue();


                PageHelper.WaitUntilElementIsVisible(TestWebDriver, By.XPath("//h2[text()='Work Items Summary']"), TimeoutConfiguration.Element);

                bool screeningClicked = false;


                GetQueueLinkByTaskName(regId, Tasks.ProviderScreening, ref screeningClicked);

                if (screeningClicked)
                    ApproveProviderScreening(regId, filePath);


                bool providerReviewClicked = false;

                GetQueueLinkByTaskName(regId, Tasks.ProviderReview, ref providerReviewClicked);

                bool isApplicationSubmitted = false;

                if (providerReviewClicked)
                {
                    Registration = new RegistrationPage(TestWebDriver);

                    Registration.WaitUntilPageLoad();

                    while (!isApplicationSubmitted)
                    {

                        if (GetRegistrationTitle() != null && GetRegistrationTitle().Text == TC.ProviderDataEntry.Utilities.Pages.NPIAndMedID)
                        {
                            NPIAndMedID nPIAndMedID = new NPIAndMedID(TestWebDriver, regId);
                            nPIAndMedID.UpdateEffectiveDate();
                        }
                        else if (GetRegistrationTitle() != null && GetRegistrationTitle().Text == TC.ProviderDataEntry.Utilities.Pages.ApplicationDisposition)
                        {
                            Registration.TakeAction.Click();
                            isApplicationSubmitted = true;
                            break;
                        }

                        Registration.BtnApprove.Click();
                    }
                }

                LoginByProfile(Roles.StateAdmin);

                workflowPage = new WorkflowPage(TestWebDriver, regId);
                workflowPage.RunWorkflow();

                LoginByProfile(Roles.StateAdmin);

                GroupReview = new GroupReview(TestWebDriver, regId);
                GroupReview.SidebarMenu.Click();
                GroupReview.ProviderSearch.Click();

                GroupReview.txtRegID.Set(regId);
                GroupReview.Search.Click();

                NUnit.Framework.TestContext.WriteLine($"{testName} is successfully executed and newly Created RegId is::{regId}");

            }
            catch (Exception ex)
            {
                NUnit.Framework.TestContext.WriteLine($"{testName}- failed:{ex.Message}");
                NUnit.Framework.Assert.Fail($"{testName}- failed:{ex.Message}");

                TestWebDriver.Close();
                TestWebDriver.Quit();
                TestWebDriver.Dispose();
                TestWebDriver = null;
            }
            #endregion
        }

        private void GetQueueLinkByTaskName(string regId, string taskName, ref bool screeningClicked)
        {
            bool isBroken = false;

            var gridView = TestWebDriver.FindElement(By.Id("ctl00_MainContent_gvAssignedDetail"));
            if (gridView != null)
            {
                var rows = gridView.FindElements(By.TagName("tr"));

                if (rows != null && rows.Count() > 0)
                {
                    foreach (var row in rows)
                    {
                        var columns = row.FindElements(By.TagName("td"));

                        foreach (var column in columns)
                        {
                            if (column.Text == regId)
                            {
                                if (taskName == "Provider Screening")
                                {
                                    IWebElement screening = columns.Where(c => c.Text == taskName).SingleOrDefault();

                                    if (screening != null)
                                    {
                                        screening.Click();
                                        screeningClicked = true;
                                        isBroken = true;
                                        break;
                                    }
                                }
                                else if (taskName == "Provider Review")
                                {
                                    IWebElement providerReview = columns.Where(c => c.Text == taskName).SingleOrDefault();

                                    if (providerReview != null)
                                    {
                                        providerReview.Click();
                                        screeningClicked = true;
                                        isBroken = true;
                                        break;
                                    }
                                }
                            }
                        }
                        if (isBroken)
                            break;
                    }
                }
            }
        }

        private void ApproveProviderScreening(string regId, string filePath)
        {
            IWebElement provideScreener = TestWebDriver.FindElement(By.Id("ctl00_MainContent_ucRegistrationNavigation_lblTitlePS"));

            if (provideScreener == null || provideScreener.Text != "Provider Screening")
            {

            }

            IWebElement elem = TestWebDriver.FindElement(By.XPath($"//*[@id='ctl00_MainContent_ucScreeningSteps_{regId}_ucScreeningActivities_grdScreeningDetails']"));

            ////*[@id="ctl00_MainContent_ucScreeningSteps_597969_ucScreeningActivities_grdScreeningDetails"]

            if (PageHelper.IsElementIsVisible(TestWebDriver, By.XPath($"//*[@id='ctl00_MainContent_ucScreeningSteps_{regId}_ucScreeningActivities_grdScreeningDetails']"), TimeoutConfiguration.Element))
            {
                var screeningDetails = TestWebDriver.FindElement(By.XPath($"//*[@id='ctl00_MainContent_ucScreeningSteps_{regId}_ucScreeningActivities_grdScreeningDetails']"));
                VerifyLicenseStatus(screeningDetails, regId, filePath);
            }

            if (PageHelper.IsElementIsVisible(TestWebDriver, By.XPath($"//*[@id='ctl00_MainContent_ucRegistrationNavigation_btnTakeAction']"), TimeoutConfiguration.Element))
            {
                TestWebDriver.FindElement(By.XPath("//*[@id='ctl00_MainContent_ucRegistrationNavigation_btnTakeAction']")).Click();

                PageHelper.WaitUntilElementIsVisible(TestWebDriver, By.XPath("//*[@id='ctl00_MainContent_ucRegistrationNavigation_lblMpeTitle']"), TimeoutConfiguration.Element);

                TestWebDriver.FindElement(By.XPath("//*[@id='ctl00_MainContent_ucRegistrationNavigation_chkScreeningComplete']")).Click();

                TestWebDriver.FindElement(By.XPath("//*[@id='ctl00_MainContent_ucRegistrationNavigation_btnSavempe']")).Click();
            }

            if (PageHelper.IsElementIsVisible(TestWebDriver, By.XPath("//*[@id='ctl00_rptWorkflowActions_ctl00_btnAction']"), TimeoutConfiguration.Element))
            {
                TestWebDriver.FindElement(By.XPath("//*[@id='ctl00_rptWorkflowActions_ctl00_btnAction']")).Click();
            }
        }

        private void VerifyLicenseStatus(IWebElement screeningDetails, string regId, string filePath)
        {
            if (screeningDetails != null)
            {
                var screening = screeningDetails.FindElements(By.TagName("tr"));

                if (screening != null && screening.Count() > 0)
                {
                    bool isBroken = false;
                    foreach (var row in screening)
                    {
                        if (isBroken)
                            break;

                        var columns = row.FindElements(By.TagName("td"));

                        foreach (var column in columns)
                        {

                            if (column.Text == "License Verification")
                            {
                                var columnss = row.FindElements(By.TagName("td"));

                                IWebElement? licensePending = columnss.Where(c => c.Text == "Pending").FirstOrDefault();

                                if (licensePending != null)
                                {
                                    IWebElement acnhorElement = licensePending.FindElement(By.TagName("a"));
                                    Thread.Sleep(1000);

                                    acnhorElement.Click();

                                    Thread.Sleep(1000);

                                    IWebElement matchResultsElem = TestWebDriver.FindElement(By.XPath($"//*[@id='ctl00_MainContent_ucScreeningSteps_{regId}_ucScreeningResult_ddlMatchResults']"));

                                    if (matchResultsElem != null)
                                    {

                                    }

                                    SelectElement verificationResults = new SelectElement(TestWebDriver.FindElement(By.XPath($"//*[@id='ctl00_MainContent_ucScreeningSteps_{regId}_ucScreeningResult_ddlMatchResults']")));
                                    verificationResults.SelectByText("Verified");

                                    Thread.Sleep(1000);

                                    TestWebDriver.FindElement(By.XPath($"//*[@id='ctl00_MainContent_ucScreeningSteps_{regId}_ucScreeningResult_txtAdverseAction']")).Set("Test", true);

                                    IWebElement fileUploadElement = TestWebDriver.CreateSmartElement(By.XPath($"//*[@id='ctl00_MainContent_ucUploadDocument_filUploadFile']")).Element;

                                    if (fileUploadElement != null)
                                    {
                                        fileUploadElement.SendKeys(filePath);
                                        Thread.Sleep(4000);

                                        TestWebDriver.FindElement(By.XPath($"//*[@id='ctl00_MainContent_ucUploadDocument_UploadButton']")).Click();

                                        Thread.Sleep(3000);

                                        PageHelper.WaitUntilElementIsVisible(TestWebDriver, By.XPath("//*[@id='ctl00_MainContent_ucUploadDocument_gvUploadedDocs']"), TimeoutConfiguration.Element);

                                        TestWebDriver.FindElement(By.XPath($"//*[@id='ctl00_MainContent_ucScreeningSteps_{regId}_ucScreeningResult_btnConfirm']")).Click();

                                        Thread.Sleep(4000);

                                        var newScreeningDetails = TestWebDriver.FindElement(By.XPath($"//*[@id='ctl00_MainContent_ucScreeningSteps_{regId}_ucScreeningActivities_grdScreeningDetails']"));
                                        isBroken = true;
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }
            }


        }

        private IWebElement GetRegistrationTitle()
        {
            IWebElement element = PageHelper.IsElementExist(TestWebDriver, By.XPath("//*[@id='ctl00_MainContent_ucRegistrationNavigation_lblTitlePS']")) ?
                   TestWebDriver.FindElement(By.XPath("//*[@id='ctl00_MainContent_ucRegistrationNavigation_lblTitlePS']")) : null;

            if (element != null)
            {
                return element;
            }
            return null;
        }

        private bool RunWorkflow(string regId)
        {

            bool status = false;

            TestWebDriver.FindElement(By.Id("menu")).Click();

            PageHelper.WaitUntilElementIsVisible(TestWebDriver, By.XPath($"//a[@title='Workflow Engine']"), TimeoutConfiguration.Element);

            IWebElement workflowSearch = TestWebDriver.FindElement(By.XPath($"//a[@title='Workflow Engine']"));

            if (workflowSearch != null)
            {
                workflowSearch.Click();
            }

            PageHelper.WaitUntilElementIsVisible(TestWebDriver, By.XPath($"//*[@id='btnRefresh']"), TimeoutConfiguration.Element);

            WebDriverWait wait = new WebDriverWait(TestWebDriver, TimeSpan.FromSeconds(30));
            wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementToBeClickable(By.XPath("//*[@id='gvAwaitingAction_ctl00_ctl02_ctl02_FilterTextBox_REGISTRATION_ID']"))).Set(regId, true);
            wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementToBeClickable(By.XPath("//*[@id='gvAwaitingAction_ctl00_ctl02_ctl02_Filter_REGISTRATION_ID']"))).Click();
            wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementToBeClickable(By.XPath($"//*[@id='gvAwaitingAction_rfltMenu_detached']/ul/li/a[span[text()='EqualTo']]"))).Click();

            bool isRecordProcessed = false;
            bool isNoRecordsFound = false;

            while (!isRecordProcessed)
            {
                try
                {
                    IWebElement resultGridMain = TestWebDriver.FindElement(By.XPath("//*[@id='gvAwaitingAction_ctl00']//tr[contains(@class,'rgNoRecords')]"));
                    if (resultGridMain != null || resultGridMain.IsElementVisible())
                    {
                        isNoRecordsFound = true;
                        isRecordProcessed = true;
                    }
                }
                catch
                {

                }
                if (!isNoRecordsFound)
                {
                    IWebElement resultGrid = TestWebDriver.FindElement(By.XPath("//*[@id='gvAwaitingAction_ctl00']//a[text()='Process']"));
                    if (resultGrid.IsElementVisible())
                        resultGrid.Click();
                    else
                        isRecordProcessed = true;
                }
            }
            wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementToBeClickable(By.XPath($"//*[@id='btnLogout']"))).Click();
            return status;
        }


    }
}
