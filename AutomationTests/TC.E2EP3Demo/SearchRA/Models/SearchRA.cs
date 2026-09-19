using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TC.SearchRA.Models
{
    public class SearchRA
    {
        public string RegID { get; set; }
        public string RANumber { get; set; }

        public string ICN { get; set; }

        public string DestinationPayerName { get; set; }

        public string ReportRunDateFrom { get; set; }

        public string ToDate { get; set; }
    }
}
