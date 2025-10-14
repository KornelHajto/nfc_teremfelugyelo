using System.ComponentModel.DataAnnotations;

namespace API.Models
{
    public class Key
    {
        [Key]
        public string Hash { get; set; }

        public Log? LastUsed { get; set; }
    }
}
