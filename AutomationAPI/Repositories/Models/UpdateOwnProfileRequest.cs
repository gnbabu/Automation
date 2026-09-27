namespace AutomationAPI.Repositories.Models
{
    public class UpdateOwnProfileRequest
    {
        public string? Photo { get; set; }
        public string? PhoneNumber { get; set; }
        public int? TimeZone { get; set; }
    }
}
