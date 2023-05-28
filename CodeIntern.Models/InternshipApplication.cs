using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeIntern.Models
{
    public class InternshipApplication
    {
        [Key]
        public int InternshipApplicationId { get; set; }
        [Required]
        public int InternshipId { get; set; }
        [Required]

        public string FirstName { get; set; }
        [Required]
        public string LastName { get; set; }
        [Required]
        public string Email { get; set; }
        [Required]
       public string CVUrl { get; set; } = string.Empty;

        public DateTime? DateCreated { get; set; }
        public string? Status { get; set; }

        
    }
}
