namespace AutomationAPI.Repositories.Models
{
    public class ReleaseModel
    {
        public int ReleaseId { get; set; }
        public string ReleaseName { get; set; }
        public string Description { get; set; }

        public string ReleaseLifecycle { get; set; }
        public bool IsActive { get; set; }

        public string SignOffStatus { get; set; }
        public string SignedOffBy { get; set; }
        public DateTime? SignedOffOn { get; set; }

        public DateTime CreatedOn { get; set; }
        public string CreatedBy { get; set; }
    }

    public class ReleaseRequestDto
    {
        public int? ReleaseId { get; set; }
        // Required for both create & update
        public string ReleaseName { get; set; }

        // Optional
        public string Description { get; set; }

        // Used only for update (ignored during create)
        public string ReleaseLifecycle { get; set; }

        // Used only for update (ignored during create)
        public bool? IsActive { get; set; }
        public string CreatedBy { get; set; }
    }

}
