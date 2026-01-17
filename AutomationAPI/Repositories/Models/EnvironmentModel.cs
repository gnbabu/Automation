using System.ComponentModel.DataAnnotations;

namespace AutomationAPI.Repositories.Models
{
    public class EnvironmentModel
    {
        public int EnvironmentId { get; set; }
        public string EnvironmentName { get; set; }
        public string Description { get; set; }
        public bool IsActive { get; set; }

        public int CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }

        // Joined User info
        public string UserName { get; set; }
        public string Email { get; set; }
    }



    public class EnvironmentRequestDto
    {
        public int? EnvironmentId { get; set; }

        [Required]
        [MaxLength(50)]
        public string EnvironmentName { get; set; }

        [MaxLength(255)]
        public string Description { get; set; }

        [Required]
        public int CreatedBy { get; set; }   // FK → aut.User(UserID)

        public bool? IsActive { get; set; }
    }

}
