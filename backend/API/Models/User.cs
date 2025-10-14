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
        public required string FullName { get; set; }
        [Required]
        public required string Password { get; set; }
        [Required]
        public AdminLevels AdminLevel { get; set; } = 0;
        [Required]
        public required byte[] Picture { get; set; }


        public List<RememberMe> RememberMe { get; set; } = new List<RememberMe>();
        public List<Key> Keys { get; set; } = new List<Key>();
        public List<Course> Courses { get; set; } = new List<Course>();
        public List<Subject> Teaches { get; set; } = new List<Subject>();
    }
}
