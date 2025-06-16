namespace AutomationAPI.Repositories.Models
{
    public class AuthResponse
    {
        public int StatusCode { get; set; }
        public string Message { get; set; }
        public User User { get; set; }
        public string Token { get; set; }
    }

}
