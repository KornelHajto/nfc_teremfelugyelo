namespace API.DTOs
{
    public class AddCourseDatesDTO
    {
        public required int Id { get; set; }
        public required DateTime StartDate { get; set; }
        public required DateTime EndDate { get; set; }
    }
}
