namespace API.DTOs
{
    public class CreateCourseDTO
    {
        public required string Name { get; set; }
        public required string SubjectId { get; set; }
        public required string ClassroomId { get; set; }
    }
}
