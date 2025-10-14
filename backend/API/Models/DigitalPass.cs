using System.ComponentModel.DataAnnotations;

namespace API.Models
{
    public class DigitalPass
    {
        [Required]
        public string Key { get; set; }
        public DateTime LastUsed { get; set; }
        [Required]
        public DateTime Expiration { get; set; }
    }
}
