using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

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

        public List<DateTime>? Date { get; set; } = new List<DateTime>();
        public TimeSpan? Duration { get; set; }
        [JsonIgnore]
        public List<User> Students { get; set; } = new List<User>();
    }
}
