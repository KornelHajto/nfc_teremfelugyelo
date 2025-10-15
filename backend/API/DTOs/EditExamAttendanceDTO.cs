using API.Models;

namespace API.DTOs
{
    public class EditExamAttendanceDTO
    {
        public required int Id { get; set; }
        public required ExamStatusTypes Status { get; set; }
    }
}
