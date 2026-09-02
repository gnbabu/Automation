namespace AutomationAPI.Repositories.Models
{
    // Deliberately has no UserId field - the caller's identity always comes from the
    // JWT's ClaimTypes.NameIdentifier claim (see UsersController.UpdateOwnProfile), never
    // from the request body, so this endpoint can't be used to edit another user's row.
    public class UpdateOwnProfileRequest
    {
        public string? Photo { get; set; }
        public string? PhoneNumber { get; set; }
        public int? TimeZone { get; set; }
    }
}
