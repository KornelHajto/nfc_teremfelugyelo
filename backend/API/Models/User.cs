using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

public enum AdminLevels
{
    Student,
    Teacher,
    Admin
}

namespace API.Models
{
    public class User
    {
        [Key]
        public required string NeptunId { get; set; }
        [Required]
        public required string Password { get; set; }
        [Required]
        public required AdminLevels AdminLevel { get; set; } = 0;

        public RememberMe? RememberMe { get; set; }
        public DigitalPass? DigitalKey { get; set; }
        public PhysicalPass? PhysicalKey { get; set; }

    }
}
