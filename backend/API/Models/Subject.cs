using System.ComponentModel.DataAnnotations;

namespace API.Models
{
    public class Subject
    {
        [Key]
        public string Id { get; set; }
        [Required]
        public required string Name { get; set; }
        [Required]
        public required List<User> Teachers { get; set; } = new List<User>();
    }
}
