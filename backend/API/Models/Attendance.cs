using System.ComponentModel.DataAnnotations;

namespace API.Models
{
    public enum AttendanceTypes
    {
        Arrived,
        Late,
        AbsenceWithExcuse,
        AbsenceWithoutExcuse
    }

    public class Attendance
    {
        public int Id { get; set; }
        [Required]
        public required User User { get; set; }
        [Required]
        public required Subject Subject { get; set; }
        [Required]
        public required DateTime Arrival { get; set; } = DateTime.Now;
        [Required]
        public required AttendanceTypes AttendanceType { get; set; }
        public string? Comment { get; set; }
    }
}
