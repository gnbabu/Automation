using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TC.ProviderDataEntry.Models
{
    public class CLIACertificationsDTO
    {
        public string CLIANumber { get; set; }
        public string CLIACertificationType { get; set; }
        public string CLIAEffectiveDate { get; set; }
        public string CLIAExpirationdate { get; set; }
        public string AddNewCLIACertificate { get; set; }
    }
}
