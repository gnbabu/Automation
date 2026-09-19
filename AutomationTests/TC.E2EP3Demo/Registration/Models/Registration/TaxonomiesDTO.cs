using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TC.ProviderDataEntry.Models
{
    public class TaxonomiesDTO
    {
        public string Taxonomy { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public string IsPrimaryTaxonomy { get; set; }
        public string AddNewtaxonomy { get; set; }
    }
}
