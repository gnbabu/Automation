using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TC.PriorAuthSearch.Models
{
    public class SearchPA
    {
        public string PriorAuthorizationNumber { get; set; }
        public string PatientTrackingNumber { get; set; }
        public string MedicaidBillingNumber { get; set; }
        public string ICDProcedureCode { get; set; }
        public string DateofBirth { get; set; }
        public string ProcedureCode { get; set; }
        public string PASubmissionDate { get; set; }
        public string RevenueCode { get; set; }
        public string Status { get; set; }
        public string DiagnosisCode { get; set; }
        public string OrderingProviderNPI { get; set; }
        public string PAEffectiveDate { get; set; }
        public string PayerName { get; set; }
        public string PAExpirationDate { get; set; }
        public string AssignmentType { get; set; }
        public string RegID { get; set; }
    }
}
