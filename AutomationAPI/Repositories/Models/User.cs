﻿namespace AutomationAPI.Repositories.Models
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
    }
}
