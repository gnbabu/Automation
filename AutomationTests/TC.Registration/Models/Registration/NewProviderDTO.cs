using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TC.ProviderDataEntry.Models
{
    public class NewProviderDTO
    {
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string LastName { get; set; }
        public string TaxID { get; set; }
        public string NPI { get; set; }
        public string Gender { get; set; }
        public string DateofBirth { get; set; }
        public string Zipcode { get; set; }
        public string ZipcodeExt { get; set; }
        public string Taxonomy { get; set; }

        public string ProviderType { get; set; }

        public string AutomationAPI { get; set; }
    }
}
