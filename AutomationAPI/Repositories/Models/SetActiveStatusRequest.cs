namespace AutomationAPI.Repositories.Models
{
    public class SetActiveStatusRequest
    {
        public bool Active { get; set; }
        public int UserId { get; set; }
    }
}
