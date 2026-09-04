using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TC.ProviderDataEntry.Models
{
    public class WorkHistoryDTO
    {
        public string currentEmployer { set; get; }
        public string EmployerName { set; get; }
        public string StartDate { set; get; }
        public string EndDate { set; get; }
        public string OrganizationName { set; get; }
        public string Address1 { set; get; }
        public string Address2 { set; get; }
        public string City { get; set; }
        public string State { get; set; }
        public string ZipCode { get; set; }
        public string Phone { get; set; }
        public string PhoneExtension { get; set; }
        public string Fax { get; set; }
        public string County { get; set; }
        public string ContactName { get; set; }
        public string EmailAddress1 { get; set; }
        public string EmailAddress2 { get; set; }
        public string AdditionalInformation { get; set; }
        public string ReasonForDeparture { get; set; }
        public string MilitaryReserve { get; set; }
        public string GapStartDate { get; set; }
        public string GapEndDate { get; set; }
        public string ReasonForGap { get; set; }
    }
}
