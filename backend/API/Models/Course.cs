using System.ComponentModel.DataAnnotations;

namespace API.Models
{
    public class Course
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public required string Name { get; set; }
        [Required]
        public required Subject Subject { get; set; }
        [Required]
        public required Classroom Classroom { get; set; }
        [Required]
        public List<DateTime>? Date { get; set; } = new List<DateTime>();
        [Required]
        public TimeSpan? Duration { get; set; }
    }
}
