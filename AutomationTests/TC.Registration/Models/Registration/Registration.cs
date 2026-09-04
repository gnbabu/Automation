using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TC.ProviderDataEntry.Models
{
    public class Registration
    {
        public NewProviderDTO NewProviderDTO { get; set; }
        public ProviderContactInfo ProviderContactInfo { get; set; }
        public PrimaryAddressServiceDTO PrimaryAddressServiceDTO { get; set; }
        public BillingAndPaymentAddressDTO BillingAndPaymentAddressDTO { get; set; }
        public OtherServiceLocationsDTO OtherServiceLocationsDTO { get; set; }
        public Adress1099 Adress1099 { get; set; }
        public HomeOfficeAddressDTO HomeOfficeAddressDTO { get; set; }
        public SpecialitiesDTO SpecialitiesDTO { get; set; }
        public TaxonomiesDTO TaxonomiesDTO { get; set; }
        public ProfessionalLicenses ProfessionalLicenses { get; set; }
        public BoardCertificateDTO BoardCertificateDTO { get; set; }
        public CorrespondenceAddressDTO CorrespondenceAddressDTO { get; set; }
        public CLIACertificationsDTO CLIACertificationsDTO { get; set; }
        public MediCareNumberDTO MediCareNumberDTO { get; set; }
        public GroupFacilityHospital GroupFacilityHospital { get; set; }
        public MCPAffiliationDTO MCPAffiliationDTO { get; set; }
        public StateCDSNumberDTO StateCDSNumberDTO { get; set; }
        public FederalDEARegistrationDTO FederalDEARegistrationDTO { get; set; }
        public ProfessionalLiabilityInsurance ProfessionalLiabilityInsurance { get; set; }
        public EducationDTO EducationDTO { get; set; }
        public MalPracticeClaimsHistoryDTO MalPracticeClaimsHistoryDTO { get; set; }
        public WorkHistoryDTO WorkHistoryDTO { get; set; }
        public W9FormDTO W9FormDTO { get; set; }
        public EFTBankingDTO EFTBankingDTO { get; set; }
        public RequiredDocumentsDTO RequiredDocumentsDTO { get; set; }
        public AgreementsDTO AgreementsDTO { get; set; }

    }

    
}
