using Selenium.BaseComponents.Utilities;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using TC.ProviderDataEntry.Models;
using System.Net.NetworkInformation;

namespace TC.ProviderDataEntry.Utilities
{
    public static class DataRepository
    {
        public static TC.ProviderDataEntry.Models.Registration GetRegistrationData()
        {
            DataSet ds = ExcelReader.GetExcelFileAsDataSet("C:\\Projects\\RegistationTestData.xlsx", true);

            TC.ProviderDataEntry.Models.Registration registration = new TC.ProviderDataEntry.Models.Registration();

            var properties = typeof(TC.ProviderDataEntry.Models.Registration).GetProperties();
            foreach (var propertyInfo in properties)
            {
                string key = propertyInfo.Name;
                DataTable dt = ds.Tables[key];

                if (key.Equals("NewProviderDTO"))
                    registration.NewProviderDTO = Mapper.BindData<NewProviderDTO>(dt);

                if (key.Equals("ProviderContactInfo"))
                    registration.ProviderContactInfo = Mapper.BindData<ProviderContactInfo>(dt);

                if (key.Equals("PrimaryAddressServiceDTO"))
                    registration.PrimaryAddressServiceDTO = Mapper.BindData<PrimaryAddressServiceDTO>(dt);

                if (key.Equals("BillingAndPaymentAddressDTO"))
                    registration.BillingAndPaymentAddressDTO = Mapper.BindData<BillingAndPaymentAddressDTO>(dt);

                if (key.Equals("OtherServiceLocationsDTO"))
                    registration.OtherServiceLocationsDTO = Mapper.BindData<OtherServiceLocationsDTO>(dt);

                if (key.Equals("Adress1099"))
                    registration.Adress1099 = Mapper.BindData<Adress1099>(dt);

                if (key.Equals("HomeOfficeAddressDTO"))
                    registration.HomeOfficeAddressDTO = Mapper.BindData<HomeOfficeAddressDTO>(dt);

                if (key.Equals("SpecialitiesDTO"))
                    registration.SpecialitiesDTO = Mapper.BindData<SpecialitiesDTO>(dt);

                if (key.Equals("TaxonomiesDTO"))
                    registration.TaxonomiesDTO = Mapper.BindData<TaxonomiesDTO>(dt);

                if (key.Equals("ProfessionalLicenses"))
                    registration.ProfessionalLicenses = Mapper.BindData<ProfessionalLicenses>(dt);

                if (key.Equals("BoardCertificateDTO"))
                    registration.BoardCertificateDTO = Mapper.BindData<BoardCertificateDTO>(dt);

                if (key.Equals("CorrespondenceAddressDTO"))
                    registration.CorrespondenceAddressDTO = Mapper.BindData<CorrespondenceAddressDTO>(dt);

                if (key.Equals("CLIACertificationsDTO"))
                    registration.CLIACertificationsDTO = Mapper.BindData<CLIACertificationsDTO>(dt);

                if (key.Equals("MediCareNumberDTO"))
                    registration.MediCareNumberDTO = Mapper.BindData<MediCareNumberDTO>(dt);

                if (key.Equals("GroupFacilityHospital"))
                    registration.GroupFacilityHospital = Mapper.BindData<GroupFacilityHospital>(dt);

                if (key.Equals("MCPAffiliationDTO"))
                    registration.MCPAffiliationDTO = Mapper.BindData<MCPAffiliationDTO>(dt);

                if (key.Equals("StateCDSNumberDTO"))
                    registration.StateCDSNumberDTO = Mapper.BindData<StateCDSNumberDTO>(dt);

                if (key.Equals("FederalDEARegistrationDTO"))
                    registration.FederalDEARegistrationDTO = Mapper.BindData<FederalDEARegistrationDTO>(dt);

                if (key.Equals("ProfessionalLiabilityInsurance"))
                    registration.ProfessionalLiabilityInsurance = Mapper.BindData<ProfessionalLiabilityInsurance>(dt);

                if (key.Equals("EducationDTO"))
                    registration.EducationDTO = Mapper.BindData<EducationDTO>(dt);

                if (key.Equals("MalPracticeClaimsHistoryDTO"))
                    registration.MalPracticeClaimsHistoryDTO = Mapper.BindData<MalPracticeClaimsHistoryDTO>(dt);

                if (key.Equals("WorkHistoryDTO"))
                    registration.WorkHistoryDTO = Mapper.BindData<WorkHistoryDTO>(dt);

                if (key.Equals("W9FormDTO"))
                    registration.W9FormDTO = Mapper.BindData<W9FormDTO>(dt);

                if (key.Equals("EFTBankingDTO"))
                    registration.EFTBankingDTO = Mapper.BindData<EFTBankingDTO>(dt);

                if (key.Equals("RequiredDocumentsDTO"))
                    registration.RequiredDocumentsDTO = Mapper.BindData<RequiredDocumentsDTO>(dt);

                if (key.Equals("AgreementsDTO"))
                    registration.AgreementsDTO = Mapper.BindData<AgreementsDTO>(dt);
            }

            return registration;
        }

        public static object GetAutomationData(string flowName)
        {

            APIGatway aPIGatway = new APIGatway();
            List<AutomationData> automationDatas = aPIGatway.GetAutomationData(flowName).Result;

            if (automationDatas != null && automationDatas.Count() > 0 && flowName.Equals("Registration"))
            {
               TC.ProviderDataEntry.Models.Registration registration = new Models.Registration();

                foreach (AutomationData data in automationDatas)
                {
                    string key = data.SectionName;
                    List<AutomationContent> dt = data.automationContents;


                    if (key.Equals("NewProviderDTO"))
                        registration.NewProviderDTO = Mapper.BindData<NewProviderDTO>(dt);

                    if (key.Equals("ProviderContactInfo"))
                        registration.ProviderContactInfo = Mapper.BindData<ProviderContactInfo>(dt);

                    if (key.Equals("PrimaryAddressServiceDTO"))
                        registration.PrimaryAddressServiceDTO = Mapper.BindData<PrimaryAddressServiceDTO>(dt);

                    if (key.Equals("BillingAndPaymentAddressDTO"))
                        registration.BillingAndPaymentAddressDTO = Mapper.BindData<BillingAndPaymentAddressDTO>(dt);

                    if (key.Equals("OtherServiceLocationsDTO"))
                        registration.OtherServiceLocationsDTO = Mapper.BindData<OtherServiceLocationsDTO>(dt);

                    if (key.Equals("Adress1099"))
                        registration.Adress1099 = Mapper.BindData<Adress1099>(dt);

                    if (key.Equals("HomeOfficeAddressDTO"))
                        registration.HomeOfficeAddressDTO = Mapper.BindData<HomeOfficeAddressDTO>(dt);

                    if (key.Equals("SpecialitiesDTO"))
                        registration.SpecialitiesDTO = Mapper.BindData<SpecialitiesDTO>(dt);

                    if (key.Equals("TaxonomiesDTO"))
                        registration.TaxonomiesDTO = Mapper.BindData<TaxonomiesDTO>(dt);

                    if (key.Equals("ProfessionalLicenses"))
                        registration.ProfessionalLicenses = Mapper.BindData<ProfessionalLicenses>(dt);

                    if (key.Equals("BoardCertificateDTO"))
                        registration.BoardCertificateDTO = Mapper.BindData<BoardCertificateDTO>(dt);

                    if (key.Equals("CorrespondenceAddressDTO"))
                        registration.CorrespondenceAddressDTO = Mapper.BindData<CorrespondenceAddressDTO>(dt);

                    if (key.Equals("CLIACertificationsDTO"))
                        registration.CLIACertificationsDTO = Mapper.BindData<CLIACertificationsDTO>(dt);

                    if (key.Equals("MediCareNumberDTO"))
                        registration.MediCareNumberDTO = Mapper.BindData<MediCareNumberDTO>(dt);

                    if (key.Equals("GroupFacilityHospital"))
                        registration.GroupFacilityHospital = Mapper.BindData<GroupFacilityHospital>(dt);

                    if (key.Equals("MCPAffiliationDTO"))
                        registration.MCPAffiliationDTO = Mapper.BindData<MCPAffiliationDTO>(dt);

                    if (key.Equals("StateCDSNumberDTO"))
                        registration.StateCDSNumberDTO = Mapper.BindData<StateCDSNumberDTO>(dt);

                    if (key.Equals("FederalDEARegistrationDTO"))
                        registration.FederalDEARegistrationDTO = Mapper.BindData<FederalDEARegistrationDTO>(dt);

                    if (key.Equals("ProfessionalLiabilityInsurance"))
                        registration.ProfessionalLiabilityInsurance = Mapper.BindData<ProfessionalLiabilityInsurance>(dt);

                    if (key.Equals("EducationDTO"))
                        registration.EducationDTO = Mapper.BindData<EducationDTO>(dt);

                    if (key.Equals("MalPracticeClaimsHistoryDTO"))
                        registration.MalPracticeClaimsHistoryDTO = Mapper.BindData<MalPracticeClaimsHistoryDTO>(dt);

                    if (key.Equals("WorkHistoryDTO"))
                        registration.WorkHistoryDTO = Mapper.BindData<WorkHistoryDTO>(dt);

                    if (key.Equals("W9FormDTO"))
                        registration.W9FormDTO = Mapper.BindData<W9FormDTO>(dt);

                    if (key.Equals("EFTBankingDTO"))
                        registration.EFTBankingDTO = Mapper.BindData<EFTBankingDTO>(dt);

                    if (key.Equals("RequiredDocumentsDTO"))
                        registration.RequiredDocumentsDTO = Mapper.BindData<RequiredDocumentsDTO>(dt);

                    if (key.Equals("AgreementsDTO"))
                        registration.AgreementsDTO = Mapper.BindData<AgreementsDTO>(dt);
                }

                return registration;
            }
           return null;
        }

    }
}