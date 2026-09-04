using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TC.ProviderDataEntry.Models
{
    public class StateCDSNumberDTO
    {
        public string AddStateCDSNumber { get; set; }
        public string CDSNumber { get; set; }
        public string State { get; set; }
        public string DateIssued { get; set; }
        public string ExpirationDate { get; set; }
        public string stateCDSDocumentPath { get; set; }
    }
}
