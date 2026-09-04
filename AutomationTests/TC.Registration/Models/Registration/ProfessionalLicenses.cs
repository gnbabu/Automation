using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TC.ProviderDataEntry.Models
{
    public class ProfessionalLicenses
    {
        public string State { get; set; }
        public string LicenseBoardName { get; set; }
        public string LicenseNumber { get; set; }
        public string EffectiveDate { get; set; }
        public string ExpirationDate { get; set; }
    }
}
