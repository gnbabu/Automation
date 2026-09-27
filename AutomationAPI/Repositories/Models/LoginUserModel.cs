using System.ComponentModel.DataAnnotations;

namespace AutomationAPI.Repositories.Models
{
    public class LoginUserModel
    {
        public int LoginUserId { get; set; }
        public int EnvironmentId { get; set; }
        public int? PortalUserId { get; set; }
        public string? PortalUserName { get; set; }
        public string UserRole { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime? ModifiedOn { get; set; }
    }

    public class LoginUserRequestDto
    {
        public int? LoginUserId { get; set; }

        [Required]
        public int EnvironmentId { get; set; }

        public int? PortalUserId { get; set; }

        [Required]
        [MaxLength(50)]
        public string UserRole { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string UserName { get; set; } = string.Empty;
        
        public string? Password { get; set; }

        public bool? IsActive { get; set; }

        public int? CreatedBy { get; set; }
        public int? ModifiedBy { get; set; }
    }
    
    public class LoginUserCredentials
    {
        public int LoginUserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}
