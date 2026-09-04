using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace TC.ProviderDataEntry.Models
{
    public static class RegistrationData
    {
        public static NewProviderDTO GetNewProviderDTO()
        {
            return new NewProviderDTO()
            {
                FirstName = "KRYSTAL",
                MiddleName = "PABROS",
                LastName = "AOAY",
                TaxID = "1003001389",
                NPI = "1003001389",
                DateofBirth = "06/06/1986",
                ProviderType = "18",
                Gender = "Female",
                Zipcode = "90405",
                ZipcodeExt= "9989",
                Taxonomy="1"

            };
        }
        public static ProviderContactInfo GetproviderContactInfo()
        {
            return new ProviderContactInfo()
            {
                PrimaryContactName = "KRYSTAL",
                Address1 = "2717 W Royal Ln",
                Address2 = "APT 2301",
                City = "Irving",
                State = "TX",
                County = "Dallas County",
                Phone1 = "4699909858",
                Phone1Ext = "3465",
                EmailAddress1 = "testetst@maximus.com",
                Zipcode = "75063",
                ZipcodeExt = "345",
            };
        }

        public static PrimaryAddressServiceDTO GetPrimaryAddressServiceDTO()
        {
            return   new PrimaryAddressServiceDTO()
            {
                Address1 = "2717 W Royal Ln",
                Address2 = "APT 2301",
                City = "Irving",
                State = "TX",
                County = "Dallas County",
                Phone1 = "4699909858",
                Phone1Ext = "3465",
                EmailAddress1 = "testetst@maximus.com",
                Zipcode = "75063",
                ZipcodeExt = "345",
            };
        }

        public static BillingAndPaymentAddressDTO GetBillingAndPaymentAddressDTO()
        {
            return new BillingAndPaymentAddressDTO()
            {
                primaryContactFirstName = "KRYSTAL",
                primaryContactLastName = "AOAY",
                Address1 = "2717 W Royal Ln",
                Address2 = "APT 2301",
                City = "Irving",
                State = "TX",
                County = "Dallas County",
                Phone1 = "4699909858",
                Phone1Ext = "3465",
                EmailAddress1 = "testetst@maximus.com",
                Zipcode = "75063",
                ZipcodeExt = "345",
            };
        }

        public static OtherServiceLocationsDTO GetOtherServiceLocationsDTO()
        {
            return new OtherServiceLocationsDTO()
            {

                Name = "KRYSTAL PABROS AOAY",
                Address1 = "2717 W Royal Ln",
                Address2 = "APT 2301",
                City = "Irving",
                State = "TX",
                County = "Dallas County",
                Phone1 = "4699909858",
                Phone1Ext = "3465",
                Zipcode = "75063",
                ZipcodeExt = "345",
            };
        }

        public static Adress1099 GetAddress1099Information()
        {
            return new Adress1099()
            {

                Name = "KRYSTAL PABROS AOAY",
                Address1 = "2717 W Royal Ln",
                Address2 = "APT 2301",
                City = "Irving",
                State = "TX",
                County = "Dallas County",
                Phone1 = "4699909858",
                Phone1Ext = "3465",
                Zipcode = "75063",
                ZipcodeExt = "345",
                Email = "testetst@maximus.com",
            };
        }
        public static HomeOfficeAddressDTO GetHomeOfficeAddressInformation()
        {
            return new HomeOfficeAddressDTO()
            {
                FirstName = "KRYSTAL",
                MiddleName = "PABROS",
                LastName = "AOAY",
                Address1 = "2717 W Royal Ln",
                Address2 = "APT 2301",
                City = "Irving",
                State = "TX",
                County = "Dallas County",
                Phone1 = "4699909858",
                Phone1Ext = "3465",
                Email = "testetst@maximus.com",
                Zipcode = "75063",
                ZipcodeExt = "345",

            };
         }

        public static SpecialitiesDTO GetSpecialitiesInformation() {
            return new SpecialitiesDTO()
            {
                Specality = "226 - ADDICTION MEDICINE",
                DesignatedPrimarySpecality = "true",
                StartDate = "10/8/2023",
                Enddate = "10/8/2030",

            };
        }

        public static TaxonomiesDTO GetTaxonomiesInformation()
        {
            return new TaxonomiesDTO()
            {
                Taxonomy = "226 - ADDICTION MEDICINE",
                IsPrimaryTaxonomy = "true",
                StartDate = "10/8/2023",
                EndDate = "10/8/2030",
                AddNewtaxonomy = "false",

            };
        }

        public static ProfessionalLicenses GetProfessionalLicenses()
        {
            return new ProfessionalLicenses()
            {
                State= "Texas",
                LicenseBoardName= "Medical Board",
                LicenseNumber ="TestLicense357253",
                EffectiveDate = "10/8/2023",
                ExpirationDate = "10/8/2030",

            };
        }

        public static BoardCertificateDTO GetBoardCertificateDTO()
        {
            return new BoardCertificateDTO()
            {
                AddNewCertificate = "false",

            };
        }

        public static CorrespondenceAddressDTO GetCorrespondenceAddressDTO()
        {
            return new CorrespondenceAddressDTO()
            {

                FirstName = "KRYSTAL",
                MiddleName = "PABROS",
                LastName = "AOAY",
                Address1 = "2717 W Royal Ln",
                Address2 = "APT 2301",
                City = "Irving",
                State = "TX",
                County = "Dallas County",
                Phone1 = "4699909858",
                Phone1Ext = "3465",
                EmailAddress1 = "testetst@maximus.com",
                Zipcode = "75063",
                ZipcodeExt = "345",
            };
        }

        public static CLIACertificationsDTO GetCLIACertificationsDTO()
        {
            return new CLIACertificationsDTO()
            {
                AddNewCLIACertificate = "false",

            };
        }

        public static MediCareNumberDTO GetMediCareNumberDTO()
        {
            return new MediCareNumberDTO()
            {
                AddNewMediCareNumber = "false",
            };
        }
        public static GroupFacilityHospital GetGroupFacilityHospitalAffiliationsDTO()
        {
            return new GroupFacilityHospital()
            {
                AddFacility = "false",
            };
        }
        public static MCPAffiliationDTO GetMCPAffiliationDTO()
        {
            return new MCPAffiliationDTO()
            {
                AddMCPAffiliation = "false",
            };
        }
        public static StateCDSNumberDTO GetStateCDSNumberDTO()
        {
            return new StateCDSNumberDTO()
            {
                AddStateCDSNumber = "false",
            };
        }

        public static FederalDEARegistrationDTO GetFederalDEARegistrationDTO()
        {
            return new FederalDEARegistrationDTO()
            {
                DEARegistration = "No",
                DEANumber = "12345678",
                DEAState = "Texas",
                IssueDate = "10/10/2023",
                ExpirationDate = "10/8/2030",
                DEAStatus = "Active",
                NameOfProvider = "Federal DEA",
                Comments = "Automation Testing"
            };
        }
        public static ProfessionalLiabilityInsurance GetProfessionalLiabilityInsuranceDTO()
        {
            return new ProfessionalLiabilityInsurance()
            {
                MalpracticeInsurance = "Yes",
                SelfInsurance = "Yes",
                PolicyNumber = "1234567",
                EffectiveDate = "10/10/2023",
                OrifinalEffectiveDate = "10/10/2023",
                ExpirationDate = "10/8/2030",
                TypeOfCoverage = "Individual",
                UnlimitedCoverage = "Yes",
                TailCoverage = "Yes",
                SelfInsuredName = "KRYSTAL PABROS ADAY",
                CarrierAddress1 = "2717 W Royal Ln",
                CarrierAddress2 = "APT 2301",
                City = "Irving",
                State = "TX",
                County = "Dallas County",
                Zip ="75063",
                PolicyHolder = "KRYSTAL PABROS ADAY",
                CoverageAmountPerAggregrate = "100000",
                CoverageAmountPerOccurance = "10000",

            };
        }

        public static EducationDTO GetEducationDTO()
        {
            return new EducationDTO()
            {
                EducationType = "Internship",
                NameOfSchool = "Ohio School",
                StartDate = "10/10/2019",
                EndDate = "10/8/2023",
                CertificateAwarded = "Bachelor of Arts (BA)",
                Address1 = "2717 W Royal Ln",
                Address2 = "APT 2301",
                City = "Irving",
                State = "Texas",
                Country = "UNITED STATES",
                ZipCode = "75063",
                PhoneNumber = "123456789",

            };
        }

        public static MalPracticeClaimsHistoryDTO GetMalPracticeClaimsHistoryDTO()
        {
            return new MalPracticeClaimsHistoryDTO()
            {
                ProfessionalLiabilityPast10Years = "Yes",
                DateOfOccurence = "10/13/2023",
                DateClaimFiled = "10/13/2023",
                StatusOfTheClaim = "Open",
                ProfessionalLiabilityCarrier = "Involved",
                Address1 = "2717 W Royal Ln",
                Address2 = "APT 2301",
                City = "Irving",
                State = "TX",
                ZipCode = "75063",
                Phone = "1234567890",
                PhoneExtension = "345",
                AllegationAgainstYou = "No allegations Filed",
                WereYou = "Primary Defendant",
                YourRoleInCase = "Nothing",
                KnowledgeCaseInNPDB = "No",
            };
        }

        public static WorkHistoryDTO GetWorkHistoryDTO()
        {
            return new WorkHistoryDTO()
            {
                currentEmployer = "true",
                EmployerName = "Ohio Maximus",
                StartDate = "10/13/2018",
                EndDate = "10/12/2022",
                OrganizationName = "Ohio Maximus",
                Address1 = "2717 W Royal Ln",
                Address2 = "APT 2301",
                City = "Irving",
                State = "TX",
                County = "Dallas County",
                ZipCode = "75063",
                Phone = "1234567890",
                PhoneExtension = "345",
                EmailAddress1 = "testetst@maximus.com",
                MilitaryReserve = "No",
                GapStartDate = "10/13/2022",
                GapEndDate = "08/13/2023",
                ReasonForGap = "Family Reasons",
            };
        }

        public static W9FormDTO GetW9FormDTO()
        {
            return new W9FormDTO()
            {
                //Category Values are Individual/sole proprietor,C Corporation,S Corporation,Partnership,Trust/Estate,Limited Liability C Corporation,Limited Liability S Corporation,Limited Liability Partnership,Other
                //FormType = W9,Form 147
                CategoryType = "Limited Liability C Corporation",
                FormType = "Form 147",
                FilePath = "C:\\Projects\\SAM572 SMR V2.docx",
            };
        }

        public static EFTBankingDTO GetEFTBankingDTO()
        {
            return new EFTBankingDTO()
            {
                AddDateToPage = "true",
                SupplementalPoolPayments = "No",
                BankingOutsideOfUnitedStates = "true",
                FinancialInstitutionName = "Ohio Maximus",
                FinancialInstitutionRoutingNumber = "1234567",
                ConfirmFinancialInstitutionRoutingNumber = "1234567",
                AccountNumber = "123456789",
                ConfirmAccountNumber = "123456789",
                AccountType = "Checking",
                ProviderContactFirstName = "KRYSTAL",
                MiddleName = "PABROS",
                LastName = "ADAY",
                PhoneNumber = "1234567890",
                Extension = "385",
                EmailAddress = "testing@ohio.com",
                FaxNumber = "1234567890",
                InformationProvided = "true",
            };
        }

        public static RequiredDocumentsDTO GetRequiredDocumentsDTO()
        {
            return new RequiredDocumentsDTO()
            {
               AddDetailsOnPage = "true",
               FilePath = "C:\\Projects\\SAM572 SMR V2.docx",
               Name = "W9 Form",
               Description = "W9 Form PDF file",
            };
        }

        public static AgreementsDTO GetAgreementsDTO()
        {
            return new AgreementsDTO()
            {
                MedicaidPrioviderIntialTermsAndConditions = "true",
                MedicaidPrioviderFullTermsAndConditions = "true",
                MedicaidPrioviderProvisionCheck = "true",
                Voluntarilysurrendered = "No",
                InVoluntarilySuspended = "No",
                Resignfromaninternship = "No",
                InsuranceCancelledOrSuspended = "No",
                InfoReportedToNPDB = "No",
                DirectOrIndirectOwnerShip = "No",
                ConvictedOfCriminalOffense = "No",
                ConvictedViolenceOfLaw = "No",
                ProviderAgreementAttestation = "true",
                NameOfThePersonAttesting = "KRYSTAL PABROS ADAY",
                SanctionIndividual_No = "No"
            };
        }
    }
}
