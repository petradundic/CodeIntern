using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeIntern.Models
{
    public class Internship
    {
        [Key]
        public int InternshipId { get; set; }

        [ForeignKey("CompanyId")]
        public int CompanyId { get; set; }

        [Required]

        public string Title { get; set; }

        [Required]
        public string Description { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        [Required]
        public string Location { get; set; }


        [Required]
        public bool IsPaid { get; set; }

        public decimal? PayPerHour { get; set; }

        [Required]
        public int NumberOfOpenings { get; set; }

        [Required]
        public DateTime CreatedDate { get; set; }

        public Company Company { get; set; }
    }
}
