using System.ComponentModel.DataAnnotations;

namespace API.Models
{
    public enum ExamStatusTypes
    {
        Approved,
        Waiting,
        Denied
    }


    public class ExamAttendance
    {
        public int Id { get; set; }
        [Required]
        public required User User { get; set; }
        [Required]
        public required Exam Exam { get; set; }
        [Required]
        public required DateTime Arrival { get; set; } = DateTime.Now;
        [Required]
        public required ExamStatusTypes Status { get; set; }
    }
}
