using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TC.ProviderDataEntry.Models
{
    public class ProviderContactInfo
    {
        public string PrimaryContactName { get; set; }
        public string Title { get; set; }
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
        public string EmailAddress1 { get; set; }
        public string EmailAddress2 { get; set; }
        public string Phone1MessageDisclaimer { get; set; }
        public string Phone2MessageDisclaimer { get; set; }
    }
}
