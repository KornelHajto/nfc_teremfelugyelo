using API.Models;

namespace API.DTOs
{
    public class EditAttendanceDTO
    {
        public required int Id { get; set; }
        public required AttendanceTypes AttendanceType { get; set; }
        public required string Comment { get; set; }
    }
}
