using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace API.Models
{
    public class Subject
    {
        [Key]
        public string Id { get; set; }
        [Required]
        public required string Name { get; set; }
        [Required]
        [JsonIgnore]
        public List<User> Teachers { get; set; } = new List<User>();
    }
}
