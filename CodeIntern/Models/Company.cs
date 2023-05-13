using System.ComponentModel.DataAnnotations;

namespace CodeIntern.Models
{
    public class Company
    {
        [Key]
        public int CompanyId { get; set; }

        [Required]
        public string CompanyName { get; set; }

        [Required]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }

        [Required]
        public string Website { get; set; }

        public string Address { get; set; }
        [Required]
        public string Industry { get; set; }

    }
}
