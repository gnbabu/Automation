using System;

namespace TestDataModels
{
    public class MemberEligibilitySearch
    {
        public EligibilityProvider Provider { get; set; } = new()
        {
            ProviderMedicaidId = "2422659",
        };

        public EligibilitySearchCriteria SearchCriteria { get; set; } = new()
        {
            MedicaidBillingNumber = "103798930699",
            DateOfBirth = new DateTime(1960, 10, 21),
            FromDateOfService = new DateTime(2026, 1, 1),
            ToDateOfService = new DateTime(2026, 10, 14),
        };
    }

    public class EligibilityProvider
    {
        public string ProviderMedicaidId { get; set; }
    }

    public class EligibilitySearchCriteria
    {
        public string MedicaidBillingNumber { get; set; }
        public string Ssn { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public DateTime? FromDateOfService { get; set; }
        public DateTime? ToDateOfService { get; set; }
    }
}
