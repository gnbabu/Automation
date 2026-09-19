using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TC.ProviderDataEntry.Models
{
    public class GroupFacilityHospital
    {
        public string MedicaidID { get; set; }
        public string NPI { get; set; }
        public string InPatientSettings { get; set; }
        public string HospitalPrivileges { get; set; }
        public string HospitalPrivileges_Reason { get; set; }
        public string PrimaryFacility { get; set; }
        public string MediCaidID { get; set; }
        public string FacilityName { get; set; }
        public string StaffCategory { get; set; }
        public string StatusOfPrivileges { get; set; }
        public string StartDate { get; set; }
        public string RestrictionOfPrivileges { get; set; }
        public string RestrictionOfPrivileges_Reason { get; set; }
        public string AddFacility { get; set; }
    }
}
