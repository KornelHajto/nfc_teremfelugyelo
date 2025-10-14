using System.ComponentModel.DataAnnotations;

namespace API.Models
{
    public class RememberMe
    {
        [Required]
        public required string RememberHash { get; set; }
    }
}
