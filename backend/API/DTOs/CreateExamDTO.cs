namespace API.DTOs
{
    public class CreateExamDTO
    {
        public required int CourseId { get; set; }
        public required string ClassroomRoomId { get; set; }
        public required DateTime Date { get; set; }
        public required TimeSpan Duration { get; set; }
        public required TimeSpan EnterSpan { get; set; }
        public required TimeSpan ExitSpan { get; set; }
    }
}
