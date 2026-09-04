using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TC.ProviderDataEntry.Models
{
    public class AgreementsDTO
    {
        public string MedicaidPrioviderIntialTermsAndConditions { get; set; }
        public string MedicaidPrioviderFullTermsAndConditions { get; set; }
        public string MedicaidPrioviderProvisionCheck { get; set; }
        public string Voluntarilysurrendered { get; set; }
        public string Voluntarilysurrendered_Comments { get; set; }
        public string InVoluntarilySuspended { get; set; }
        public string InVoluntarilySuspended_Comments { get; set; }
        public string Resignfromaninternship { get; set; }
        public string Resignfromaninternship_Comments { get; set; }
        public string InsuranceCancelledOrSuspended { get; set; }
        public string InsuranceCancelledOrSuspended_Comments { get; set; }
        public string InfoReportedToNPDB { get; set; }
        public string InfoReportedToNPDB_Comments { get; set; }
        public string DirectOrIndirectOwnerShip { get; set; }
        public string DirectOrIndirectOwnerShip_Comments { get; set; }
        public string ConvictedOfCriminalOffense { get; set; }
        public string ConvictedOfCriminalOffense_Comments { get; set; }
        public string ConvictedViolenceOfLaw { get; set; }
        public string ConvictedViolenceOfLaw_Comments { get; set; }
        public string ProviderAgreementAttestation { get; set; }
        public string NameOfThePersonAttesting { get; set; }

        public string SanctionIndividual_No { get; set; }

        public string SanctionIndividual_Yes { get; set; }

        public string IndividualProviderQuestion_1 { get; set; }

        public string IndividualProviderQuestion_2 { get; set; }

        public string IndividualProviderQuestion_3 { get; set; }

        public string IndividualProviderQuestion_4 { get; set; }
    }
}
