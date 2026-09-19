using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TC.ProviderDataEntry.Models
{
    public class ProfessionalLiabilityInsurance
    {
        public string MalpracticeInsurance { get; set; }
        public string SelfInsurance { get; set; }
        public string PolicyNumber { get; set; }
        public string EffectiveDate { get; set; }
        public string OrifinalEffectiveDate { get; set; }
        public string ExpirationDate { get; set; }
        public string TypeOfCoverage { get; set; }
        public string UnlimitedCoverage { get; set; }
        public string TailCoverage { get; set; }
        public string SelfInsuredName { get; set; }
        public string CarrierAddress1 { get; set; }
        public string CarrierAddress2 { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string County { get; set; }
        public string Zip { get; set; }
        public string PolicyHolder { get; set; }
        public string CoverageAmountPerOccurance { get; set; }
        public string CoverageAmountPerAggregrate { get; set; }
        public string ExplanationMalPraticeInsurance { get; set; }
    }
}
