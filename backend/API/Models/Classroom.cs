using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace API.Models
{
    public class Classroom
    {
        [Key]
        public required string RoomId { get; set; }

        [Required]
        public List<User> InRoom { get; set; } = new List<User>();
    }
}
