using Microsoft.VisualBasic;
using System.ComponentModel.DataAnnotations;

namespace API.Models
{
    public class Exam
    {
        public int Id { get; set; }
        [Required]
        public required Course Course { get; set; }
        [Required]
        public required Classroom Classroom { get; set; }
        [Required]
        public required DateTime Date { get; set; }
        [Required]
        public required TimeSpan Duration { get; set; }
        [Required]
        public required TimeSpan EnterSpan { get; set; }
        [Required]
        public required TimeSpan ExitSpan { get; set; }
    }
}
