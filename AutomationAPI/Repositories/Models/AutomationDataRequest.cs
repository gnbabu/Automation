namespace AutomationAPI.Repositories.Models
{
    public class AutomationDataRequest
    {
        public int? Id{ get; set; }
        public int? SectionId { get; set; }
        public string? TestContent { get; set; }
        public int? UserId { get; set; }
    }
}
