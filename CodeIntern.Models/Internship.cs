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

        [Required]

        public string CompanyId { get; set; }

        [Required]
        [Display(Name = "Company Name")]
        public string CompanyName { get; set; }

        [Required]
        public string Title { get; set; }

        [Required]
        public string Description { get; set; }

        [Required]
        [Display(Name = "Start Date")]
        public DateTime StartDate { get; set; }

        [Required]
        [Display(Name = "End Date")]
        public DateTime EndDate { get; set; }

        [Required]
        public string Location { get; set; }


        [Required]
        [Display(Name = "Is Paid")]
        public bool IsPaid { get; set; }

        [Display(Name = "Pay Per Hour (eur)")]
        public decimal? PayPerHour { get; set; }

        [Required]
        [Display(Name = "Number Of Opened Positions")]
        public int NumberOfOpenings { get; set; }

        [Required]
        public DateTime CreatedDate { get; set; }

        public int NumOfApplications { get; set; }
        public string? Position { get; set; }

        [Display(Name = "Programming Language")]
        public string? ProgLanguage { get; set; }
        public string? Technology { get; set; }
        [Display(Name = "Work Place")]
        public string? WorkPlace { get; set; }

    }
}
