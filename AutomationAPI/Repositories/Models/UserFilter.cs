namespace AutomationAPI.Repositories.Models
{
    public class UserFilter
    {
        public string? Search { get; set; }
        public int? Status { get; set; }
        public int? Role { get; set; }
        public int? Priority { get; set; }
    }
}
