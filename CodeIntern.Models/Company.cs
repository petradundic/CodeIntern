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
        public string Website { get; set; }

        public string Country { get; set; }
        [Required]
        public string City { get; set; }
        [Required]
        public string Address { get; set; }
        [Required]
        public string Industry { get; set; }
        public string Description { get; set; }

        public bool? RegistrationRequest { get; set; }   
        public DateTime? RegistrationReqDate { get; set;}
        public DateTime? RegistrationApprovedDate { get; set; }

    }
}
