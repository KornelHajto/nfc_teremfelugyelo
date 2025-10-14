using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace API.Models
{
    public class Key
    {
        [Key]
        public int Id { get; set; }

        public string Hash { get; set; }

        public Log? LastUsed { get; set; }

        public DateTime? Expiration { get; set; }
    }
}
