using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TC.MemberEligibilitySearch.Models
{
    public class SearchEligibility
    {
        public string RegID { get; set; }
        public string MedicaidBillingNumber { get; set; }
        public string DateOfBirth { get; set; }
        public string FromDOS { get; set; }
        public string ToDOS { get; set; }
        public string ProcedureCode { get; set; }
        public string SSN { get; set; }
    }
}
