using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TC.ProviderDataEntry.Models
{
    public class EducationDTO
    {
        public string EducationType { set; get; }
        public string NameOfSchool { set; get; }
        public string StartDate { set; get; }
        public string EndDate { set; get; }
        public string CertificateAwarded { set; get; }
        public string Speciality { set; get; }
        public string Address1 { set; get; }
        public string Address2 { set; get; }
        public string City { set; get; }
        public string State { set; get; }
        public string ZipCode { set; get; }
        public string Country { set; get; }
        public string PhoneNumber { set; get; }
        public string Fax { set; get; }
        public string AdditionalInformation { set; get; }
    }
}
