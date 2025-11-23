namespace AutomationAPI.Repositories.Models
{
    public class User
    {
        public int? UserId { get; set; }
        public string UserName { get; set; }
        public string? Password { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string? Photo { get; set; }
        public int? RoleId { get; set; }
        public string? RoleName { get; set; }
        public bool Active { get; set; }
        public int? PriorityId { get; set; }
        public string? PriorityName { get; set; }
        public DateTime? LastLogin { get; set; }
        public int? TimeZone { get; set; }
        public string? TimeZoneName { get; set; }
        public string? Teams { get; set; }
        public int? Status { get; set; }
        public string? StatusName { get; set; }
        public string? PhoneNumber { get; set; }
        public bool? TwoFactor { get; set; }
        public string? PasswordHash { get; set; }

    }
    public class UserStatus
    {
        public int StatusId { get; set; }
        public string StatusName { get; set; } = string.Empty;
    }
    public class AppTimeZone
    {
        public int TimeZoneId { get; set; }
        public string TimeZoneName { get; set; } = string.Empty;
        public int UTCOffsetMinutes { get; set; }
        public string? Description { get; set; }
    }
    public class PriorityStatus
    {
        public int PriorityId { get; set; }
        public string PriorityName { get; set; } = string.Empty;
    }

}
