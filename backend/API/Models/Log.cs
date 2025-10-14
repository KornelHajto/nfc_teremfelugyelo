using System.ComponentModel.DataAnnotations;

namespace API.Models
{
    public enum EnterTypes
    {
        Enter,
        Exit,
        Unauthorized,
        Denied
    }

    public class Log
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public required DateTime Date { get; set; }
        [Required]
        public required User User { get; set; }
        [Required]
        public required Classroom Classroom { get; set; }
        [Required]
        public required EnterTypes EnterType { get; set; }
        public string? Comment { get; set; }
    }
}
