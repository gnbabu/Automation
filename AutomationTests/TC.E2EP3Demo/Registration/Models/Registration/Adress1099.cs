using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TC.ProviderDataEntry.Models
{
    public class Adress1099
    {
        public string Name { set; get; }

        public string Address1 { get; set; }
        public string Address2 { get; set; }
        public string City { get; set; }
        public string County { get; set; }
        public string State { get; set; }
        public string Zipcode { get; set; }
        public string ZipcodeExt { get; set; }
        public string Phone1 { get; set; }
        public string Phone1Ext { get; set; }
        public string Phone2 { get; set; }
        public string Phone2Ext { get; set; }
        public string SameBillingLocation { get; set; }
        public string SamePracticeLocation { get; set; }
        public string TaxExempt { get; set; }
        public string W9Form { get; set; }
        public string Form147 { get; set; }
        public string Email { get; set; }

    }
}
