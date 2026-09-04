using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TC.ProviderDataEntry.Models
{
    public class EFTBankingDTO
    {
        public string AddDateToPage { get; set; }
        public string SupplementalPoolPayments { get; set; }
        public string BankingOutsideOfUnitedStates { get; set; }
        public string FinancialInstitutionName { get; set; }
        public string FinancialInstitutionRoutingNumber { get; set; }
        public string ConfirmFinancialInstitutionRoutingNumber { get; set; }
        public string AccountNumber { get; set; }
        public string ConfirmAccountNumber { get; set; }
        public string AccountType { get; set; }
        public string ProviderContactFirstName { get; set; }
        public string MiddleName { get; set; }
        public string LastName { get; set; }
        public string PhoneNumber { get; set; }
        public string Extension { get; set; }
        public string EmailAddress { get; set; }
        public string FaxNumber { get; set; }
        public string InformationProvided { get; set; }
    }
}
