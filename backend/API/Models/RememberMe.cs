using System.ComponentModel.DataAnnotations;

namespace API.Models
{
    public class RememberMe
    {
        [Key]
        public required string RememberHash { get; set; }
    }
}
