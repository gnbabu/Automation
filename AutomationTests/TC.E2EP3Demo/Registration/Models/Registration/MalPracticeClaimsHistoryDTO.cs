using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TC.ProviderDataEntry.Models
{
    public class MalPracticeClaimsHistoryDTO
    {
        public string ProfessionalLiabilityPast10Years { get; set; }
        public string DateOfOccurence { get; set; }
        public string DateClaimFiled { get; set; }
        public string StatusOfTheClaim { get; set; }
        public string ClaimSettled { get; set; }
        public string ProfessionalLiabilityCarrier { get; set; }
        public string Address1 { get; set; }
        public string Address2 { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string ZipCode { get; set; }
        public string Phone { get; set; }
        public string PhoneExtension { get; set; }
        public string PolicyNumber { get; set; }
        public string MethodOfResolution { get; set; }
        public string AmountOfSettlement { get; set; }
        public string AllegationAgainstYou { get; set; }
        public string WereYou { get; set; }
        public string NoOfOtherDefendants { get; set; }
        public string YourRoleInCase { get; set; }
        public string DescribeAllegedInjury { get; set; }
        public string AllegedInjuryResults { get; set; }
        public string KnowledgeCaseInNPDB { get; set; }
    }
    
}
