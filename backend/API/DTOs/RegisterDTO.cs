namespace API.DTOs
{
    public class RegisterDTO
    {
        public required string NeptunId { get; set; }
        public required string FullName { get; set; }
        public required string Password { get; set; }
        public required string Picture { get; set; }
    }
}
