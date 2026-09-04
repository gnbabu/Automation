using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TC.ProviderDataEntry.Models
{
    public class FederalDEARegistrationDTO
    {
        public string DEARegistration { get; set; }
        public string DEANumber { get; set; }
        public string DEAState { get; set; }
        public string IssueDate { get; set; }
        public string ExpirationDate { get; set; }
        public string DEAStatus { get; set; }
        public string NameOfProvider { get; set; }
        public string Comments { get; set; }
    }
}
